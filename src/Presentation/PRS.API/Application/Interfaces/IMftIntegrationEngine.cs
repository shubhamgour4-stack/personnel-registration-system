using System.IO;
using System.Threading.Tasks;

namespace PRS.Application.Interfaces
{
    public interface IMftIntegrationEngine
    {
        Task IngestAndProcessAsync(string fileName, Stream stream, string operatorIdentity);
        Task ReprocessFailedFileAsync(long fileId, string operatorIdentity);
    }
}