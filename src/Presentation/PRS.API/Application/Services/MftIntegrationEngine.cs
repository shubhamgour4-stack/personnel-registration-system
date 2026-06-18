using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRS.Application.Interfaces;
using PRS.Core.Entities;

namespace PRS.Application.Services
{
    public class MftIntegrationEngine : IMftIntegrationEngine
    {
        private readonly IMftRepository _mftRepo;
        private readonly IMftCsvParser _csvParser;
        private readonly PRS.Infrastructure.Data.ApplicationDbContext _context;

        private static readonly Regex FileNameRegex = new(@"^PERS_([A-Z]{3})_(\d{12})\.csv$", RegexOptions.Compiled);

        public MftIntegrationEngine(
            IMftRepository mftRepo, 
            IMftCsvParser csvParser, 
            PRS.Infrastructure.Data.ApplicationDbContext context)
        {
            _mftRepo = mftRepo;
            _csvParser = csvParser;
            _context = context;
        }

        public async Task IngestAndProcessAsync(string fileName, Stream stream, string operatorIdentity)
        {
            var startTime = DateTime.UtcNow;
            var match = FileNameRegex.Match(fileName);

            if (!match.Success)
                throw new ArgumentException("FILE FORMAT IS INCORRECT");

            string countryCode = match.Groups[1].Value;
            string timestampStr = match.Groups[2].Value;

            if (!DateTime.TryParseExact(timestampStr, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out DateTime fileTimestamp))
                throw new ArgumentException("FILE FORMAT IS INCORRECT");

            // Validate using your exact GlobalCountry dataset collection
            var countryExists = await _context.Set<GlobalCountry>().AnyAsync(m => m.Country_Code == countryCode);
            if (!countryExists)
                throw new ArgumentException("FILE FORMAT IS INCORRECT");

            bool isLatest = await _mftRepo.IsLatestVersionAsync(countryCode, fileTimestamp);
            if (!isLatest)
                throw new InvalidOperationException("LATER VERSION FILE ALREADY PROCESSED");

            var history = new MftFileHistory
            {
                FileName = fileName,
                CountryCode = countryCode,
                FileTimestamp = fileTimestamp,
                ProcessingStatus = "RECEIVED",
                StartTime = startTime,
                ProcessedBy = operatorIdentity,
                CreatedDate = startTime
            };

            history = await _mftRepo.CreateHistoryAsync(history);

            try
            {
                var stagingTable = await _csvParser.ParseMftCsvAsync(stream, history.FileId, countryCode);
                history.RecordsReceived = stagingTable.Rows.Count;
                history.ProcessingStatus = "VALIDATING";
                await _mftRepo.UpdateHistoryAsync(history);

                await _mftRepo.BulkInsertStagingAsync(stagingTable);

                await ExecuteValidationEngineAsync(history.FileId);
                await ExecuteOperationalPersistencePipelineAsync(history.FileId, fileName, operatorIdentity);
                await FinalizeFileHistorySummaryAsync(history.FileId);
            }
            catch (Exception)
            {
                history.ProcessingStatus = "FAILED";
                history.EndTime = DateTime.UtcNow;
                await _mftRepo.UpdateHistoryAsync(history);
                throw;
            }
        }

        public async Task ReprocessFailedFileAsync(long fileId, string operatorIdentity)
        {
            var history = await _mftRepo.GetHistoryByIdAsync(fileId) 
                ?? throw new KeyNotFoundException("File pipeline tracking token context missing.");

            history.StartTime = DateTime.UtcNow;
            history.ProcessingStatus = "VALIDATING";
            await _mftRepo.UpdateHistoryAsync(history);

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE MFT_File_Staging SET Validation_Status = 'PENDING', Validation_Message = NULL, Processing_Status = 'RECEIVED' WHERE File_ID = {0}", fileId);

            await ExecuteValidationEngineAsync(fileId);
            await ExecuteOperationalPersistencePipelineAsync(fileId, history.FileName, operatorIdentity);
            await FinalizeFileHistorySummaryAsync(fileId);
        }

        private async Task ExecuteValidationEngineAsync(long fileId)
        {
            var stagingRecords = await _context.Set<MftFileStaging>().Where(s => s.FileId == fileId).ToListAsync();

            // Load records using correct entity collection names and property lookups
            var statusList = await _context.Set<EmploymentStatus>().ToListAsync();
            var validStatuses = statusList.Select(e => e.Employment_Status.ToLower().Trim()).ToHashSet();

            var losList = await _context.Set<LineOfService>().ToListAsync();
            var validServices = losList.Select(l => l.Line_Of_Service.ToLower().Trim()).ToHashSet();

            var gradeList = await _context.Set<Grade>().ToListAsync();
            var validGrades = gradeList.Select(g => g.Rank.ToLower().Trim()).ToHashSet();

            var targetFileCountry = stagingRecords.FirstOrDefault()?.CountryCode.ToString().Trim();
            
            var officeList = await _context.Set<WorkOfficeLocation>()
                .Where(o => o.Country_Code == targetFileCountry)
                .ToListAsync();
            var validOffices = officeList.Select(o => o.Work_Office_Description.ToLower().Trim()).ToHashSet();

            var guidList = await _context.Set<PersonnelGuid>().ToListAsync();
            var currentGuids = guidList.Select(g => g.Guid.ToLower().Trim()).ToHashSet();

            Parallel.ForEach(stagingRecords, record =>
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(record.Guid) || !currentGuids.Contains(record.Guid.ToLower().Trim()))
                    errors.Add("GUID IS NOT PRESENT");

                if (string.IsNullOrWhiteSpace(record.EmploymentStatus) || !validStatuses.Contains(record.EmploymentStatus.ToLower().Trim()))
                    errors.Add("EMPLOYMENT STATUS IS NULL OR INVALID");

                if (string.IsNullOrWhiteSpace(record.WorkOffice) || !validOffices.Contains(record.WorkOffice.ToLower().Trim()))
                    errors.Add("WORK OFFICE IS NULL OR INVALID");

                if (string.IsNullOrWhiteSpace(record.LineOfService) || !validServices.Contains(record.LineOfService.ToLower().Trim()))
                    errors.Add("LINE OF SERVICE IS NULL OR INVALID");

                if (string.IsNullOrWhiteSpace(record.Grade) || !validGrades.Contains(record.Grade.ToLower().Trim()))
                    errors.Add("GRADE IS NULL OR INVALID");

                if (string.IsNullOrWhiteSpace(record.PortfolioRequired) || (record.PortfolioRequired.Trim() != "Y" && record.PortfolioRequired.Trim() != "N"))
                    errors.Add("PORTFOLIO REQUIRED IS NULL OR INVALID");

                if (errors.Any())
                {
                    record.ValidationStatus = "INVALID";
                    record.ValidationMessage = string.Join(" | ", errors);
                    record.ProcessingStatus = "FAILED";
                }
                else
                {
                    record.ValidationStatus = "VALID";
                    record.ProcessingStatus = "VALIDATED";
                }
                record.UpdatedDate = DateTime.UtcNow;
            });

            await _context.SaveChangesAsync();
        }

        private async Task ExecuteOperationalPersistencePipelineAsync(long fileId, string fileName, string updaterIdentity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var validStaging = await _context.Set<MftFileStaging>()
                    .Where(s => s.FileId == fileId && s.ValidationStatus == "VALID" && s.ProcessingStatus == "VALIDATED")
                    .ToListAsync();

                var invalidStaging = await _context.Set<MftFileStaging>()
                    .Where(s => s.FileId == fileId && s.ValidationStatus == "INVALID")
                    .ToListAsync();

                if (invalidStaging.Any())
                {
                    var errorLogs = invalidStaging.Select(inv => new MftFileError
                    {
                        FileId = fileId,
                        RowNumber = inv.RowNumber,
                        Guid = inv.Guid,
                        ErrorMessage = inv.ValidationMessage ?? "Unspecified Domain Validation Restriction Fault.",
                        CreatedDate = DateTime.UtcNow
                    });
                    await _context.Set<MftFileError>().AddRangeAsync(errorLogs);
                }

                foreach (var record in validStaging)
                {
                    // Look up matching record in unique PersonnelGuid table via string key
                    var guidEntity = await _context.Set<PersonnelGuid>()
                        .FirstOrDefaultAsync(g => g.Guid.ToLower() == record.Guid!.ToLower().Trim());

                    if (guidEntity == null) continue;

                    // Locate or initialize matching entry in PersonnelGlobal table using mapped integer surrogate ID keys
                    var globalEntity = await _context.Set<PersonnelGlobal>()
                        .FirstOrDefaultAsync(p => p.Personnel_Guid_ID == guidEntity.Unique_ID);

                    bool isNewProfile = false;
                    if (globalEntity == null)
                    {
                        isNewProfile = true;
                        globalEntity = new PersonnelGlobal
                        {
                            Personnel_Guid_ID = guidEntity.Unique_ID,
                            Created_Date = DateTime.UtcNow
                        };
                    }

                    // Look up operational relational master data entries to parse strings to specific surrogate IDs
                    var officeEntity = await _context.Set<WorkOfficeLocation>().FirstAsync(o => o.Work_Office_Description.ToLower() == record.WorkOffice!.ToLower().Trim());
                    var gradeEntity = await _context.Set<Grade>().FirstAsync(g => g.Rank.ToLower() == record.Grade!.ToLower().Trim());
                    var losEntity = await _context.Set<LineOfService>().FirstAsync(l => l.Line_Of_Service.ToLower() == record.LineOfService!.ToLower().Trim());
                    var statusEntity = await _context.Set<EmploymentStatus>().FirstAsync(s => s.Employment_Status.ToLower() == record.EmploymentStatus!.ToLower().Trim());

                    // Audit state variations
                    if (!isNewProfile)
                    {
                        LogAuditDifferential(record.Guid!, "EmploymentStatus", globalEntity.Employment_Status_ID.ToString(), statusEntity.Employment_Status_ID.ToString(), fileName, updaterIdentity);
                        LogAuditDifferential(record.Guid!, "WorkOffice", globalEntity.Work_Office_Location_ID.ToString(), officeEntity.Work_Office_ID.ToString(), fileName, updaterIdentity);
                        LogAuditDifferential(record.Guid!, "Grade", globalEntity.Grade_ID.ToString(), gradeEntity.Grade_ID.ToString(), fileName, updaterIdentity);
                        LogAuditDifferential(record.Guid!, "LineOfService", globalEntity.Line_Of_Service_ID.ToString(), losEntity.LOS_ID.ToString(), fileName, updaterIdentity);
                        LogAuditDifferential(record.Guid!, "PortfolioRequired", globalEntity.Portfolio_Required, record.PortfolioRequired, fileName, updaterIdentity);
                    }

                    // Update production entity values
                    globalEntity.Work_Office_Location_ID = officeEntity.Work_Office_ID;
                    globalEntity.Grade_ID = gradeEntity.Grade_ID;
                    globalEntity.Line_Of_Service_ID = losEntity.LOS_ID;
                    globalEntity.Employment_Status_ID = statusEntity.Employment_Status_ID;
                    globalEntity.Portfolio_Required = record.PortfolioRequired!.Trim();
                    globalEntity.Updated_Date = DateTime.UtcNow;

                    // Generate dynamic unique business identifier fingerprint keys
                    globalEntity.Pseudo_Party_ID = $"{officeEntity.Work_Office_Code.ToLower()}*{gradeEntity.Rank_Code.ToLower()}*{losEntity.LOS_CODE.ToLower()}*{record.PortfolioRequired!.ToLower().Trim()}";

                    if (isNewProfile)
                    {
                        await _context.Set<PersonnelGlobal>().AddAsync(globalEntity);
                    }

                    record.ProcessingStatus = "PROCESSED";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private void LogAuditDifferential(string guid, string field, string? oldVal, string? newVal, string file, string user)
        {
            if (oldVal != newVal)
            {
                _context.Set<PersonnelMftAudit>().Add(new PersonnelMftAudit
                {
                    Guid = guid,
                    UpdatedField = field,
                    OldValue = oldVal,
                    NewValue = newVal,
                    FileName = file,
                    ProcessedDate = DateTime.UtcNow,
                    UpdatedBy = user
                });
            }
        }

        private async Task FinalizeFileHistorySummaryAsync(long fileId)
        {
            var history = await _mftRepo.GetHistoryByIdAsync(fileId);
            if (history == null) return;

            var stagingStats = await _context.Set<MftFileStaging>()
                .Where(s => s.FileId == fileId)
                .GroupBy(s => s.ProcessingStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int processed = stagingStats.FirstOrDefault(s => s.Status == "PROCESSED")?.Count ?? 0;
            int failed = stagingStats.FirstOrDefault(s => s.Status == "FAILED")?.Count ?? 0;

            history.RecordsProcessed = processed;
            history.RecordsFailed = failed;
            history.EndTime = DateTime.UtcNow;

            if (failed == 0 && processed > 0)
                history.ProcessingStatus = "COMPLETED";
            else if (processed > 0 && failed > 0)
                history.ProcessingStatus = "PARTIAL_SUCCESS";
            else
                history.ProcessingStatus = "FAILED";

            await _mftRepo.UpdateHistoryAsync(history);
        }
    }
}