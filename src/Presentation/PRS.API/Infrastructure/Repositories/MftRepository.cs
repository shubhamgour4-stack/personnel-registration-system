using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PRS.Application.Interfaces;
using PRS.Core.Entities;
using PRS.Infrastructure.Data; // Ensure this is importing your data folder namespace

namespace PRS.Infrastructure.Repositories
{
    public class MftRepository : IMftRepository
    {
        // Change this field from DbContext to ApplicationDbContext
        private readonly ApplicationDbContext _context; 
        private readonly string _connectionString;

        // Change the parameter type from DbContext to ApplicationDbContext here:
        public MftRepository(ApplicationDbContext context) 
        {
            _context = context;
            _connectionString = context.Database.GetConnectionString() 
                ?? throw new InvalidOperationException("Operational database connection matrix is unconfigured.");
        }
        
        // ... rest of your repository code stays exactly the same

        public async Task<MftFileHistory> CreateHistoryAsync(MftFileHistory history)
        {
            _context.Set<MftFileHistory>().Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task UpdateHistoryAsync(MftFileHistory history)
        {
            _context.Entry(history).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsLatestVersionAsync(string countryCode, DateTime currentTimestamp)
        {
            // Set-based check: Ensure no COMPLETED processing runs exist with a newer time signature
            var maxTimestamp = await _context.Set<MftFileHistory>()
                .Where(h => h.CountryCode == countryCode && h.ProcessingStatus == "COMPLETED")
                .MaxAsync(h => (DateTime?)h.FileTimestamp);

            return maxTimestamp == null || currentTimestamp > maxTimestamp.Value;
        }

        public async Task BulkInsertStagingAsync(DataTable stagingData)
        {
            // Bypassing EF Core change tracker: Streaming records directly over binary pipe
            using var bulkCopy = new SqlBulkCopy(_connectionString, SqlBulkCopyOptions.Default)
            {
                DestinationTableName = "dbo.MFT_File_Staging",
                BatchSize = 5000,          // Streams records in highly stable batches
                BulkCopyTimeout = 300      // Prevents execution timeouts during heavy network traffic
            };

            // Mapping in-memory DataTable parameters tightly to the database storage profile columns
            bulkCopy.ColumnMappings.Add("File_ID", "File_ID");
            bulkCopy.ColumnMappings.Add("Row_Number", "Row_Number");
            bulkCopy.ColumnMappings.Add("GUID", "GUID");
            bulkCopy.ColumnMappings.Add("Employment_Status", "Employment_Status");
            bulkCopy.ColumnMappings.Add("Work_Office", "Work_Office");
            bulkCopy.ColumnMappings.Add("Line_Of_Service", "Line_Of_Service");
            bulkCopy.ColumnMappings.Add("Grade", "Grade");
            bulkCopy.ColumnMappings.Add("Portfolio_Required", "Portfolio_Required");
            bulkCopy.ColumnMappings.Add("Country_Code", "Country_Code");
            bulkCopy.ColumnMappings.Add("Validation_Status", "Validation_Status");
            bulkCopy.ColumnMappings.Add("Processing_Status", "Processing_Status");
            bulkCopy.ColumnMappings.Add("Created_Date", "Created_Date");
            bulkCopy.ColumnMappings.Add("Updated_Date", "Updated_Date");

            if (stagingData.Rows.Count > 0)
            {
                await bulkCopy.WriteToServerAsync(stagingData);
            }
        }

        public async Task<MftFileHistory?> GetHistoryByIdAsync(long fileId) =>
            await _context.Set<MftFileHistory>().FindAsync(fileId);

        public async Task<IEnumerable<MftFileHistory>> GetAllHistoryAsync() =>
            await _context.Set<MftFileHistory>().OrderByDescending(h => h.CreatedDate).ToListAsync();

        public async Task<IEnumerable<MftFileStaging>> GetStagingByFileIdAsync(long fileId) =>
            await _context.Set<MftFileStaging>()
                .Where(s => s.FileId == fileId)
                .OrderBy(s => s.RowNumber)
                .ToListAsync();

        public async Task<IEnumerable<MftFileError>> GetErrorsByFileIdAsync(long fileId) =>
            await _context.Set<MftFileError>()
                .Where(e => e.FileId == fileId)
                .OrderBy(e => e.RowNumber)
                .ToListAsync();
    }
}