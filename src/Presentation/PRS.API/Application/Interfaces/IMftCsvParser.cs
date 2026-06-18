using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace PRS.Application.Interfaces
{
    public interface IMftCsvParser
    {
        Task<DataTable> ParseMftCsvAsync(Stream fileStream, long fileId, string countryCode);
    }
}