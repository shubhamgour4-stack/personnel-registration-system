using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PRS.Core.Entities;

namespace PRS.Application.Interfaces
{
    public interface IMftRepository
    {
        Task<MftFileHistory> CreateHistoryAsync(MftFileHistory history);
        Task UpdateHistoryAsync(MftFileHistory history);
        Task<bool> IsLatestVersionAsync(string countryCode, DateTime currentTimestamp);
        Task BulkInsertStagingAsync(DataTable stagingData);
        Task<MftFileHistory?> GetHistoryByIdAsync(long fileId);
        Task<IEnumerable<MftFileHistory>> GetAllHistoryAsync();
        Task<IEnumerable<MftFileStaging>> GetStagingByFileIdAsync(long fileId);
        Task<IEnumerable<MftFileError>> GetErrorsByFileIdAsync(long fileId);
    }
}