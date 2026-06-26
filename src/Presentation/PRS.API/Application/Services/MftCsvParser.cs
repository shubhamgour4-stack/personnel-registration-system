using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using PRS.Application.Interfaces;

namespace PRS.Infrastructure.Services
{
    public class MftCsvParser : IMftCsvParser
    {
        public async Task<DataTable> ParseMftCsvAsync(Stream fileStream, long fileId, string countryCode)
        {
            var dataTable = CreateStagingDataTable();
            
            // Open a stream reader to process the raw binary file data line-by-line
            using var reader = new StreamReader(fileStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,            // Tells the engine to look for column names on row 1
                TrimOptions = TrimOptions.Trim,    // Automatically strip empty leading/trailing spaces
                MissingFieldFound = null,          // Tolerates incomplete lines without crashing the stack
                BadDataFound = null
            };

            using var csv = new CsvReader(reader, config);
            
            // Advance past the header row automatically
            await csv.ReadAsync();
            csv.ReadHeader();

            int currentLogicalRow = 2; // Row 1 is the physical CSV header row
            
            while (await csv.ReadAsync())
            {
                var row = dataTable.NewRow();
                
                // Populate structural staging parameters
                row["File_ID"] = fileId;
                row["Row_Number"] = currentLogicalRow;
                
                // Extract column entries by explicit lowercase header name references
                row["GUID"] = csv.GetField<string>("guid");
                row["Employment_Status"] = csv.GetField<string>("employment_status");
                row["Work_Office"] = csv.GetField<string>("work_office");
                row["Line_Of_Service"] = csv.GetField<string>("line_of_service");
                row["Grade"] = csv.GetField<string>("grade");
                row["Portfolio_Required"] = csv.GetField<string>("portfolio_required");
                
                row["Country_Code"] = countryCode;
                row["Validation_Status"] = "PENDING";
                row["Processing_Status"] = "RECEIVED";
                row["Created_Date"] = DateTime.UtcNow;
                row["Updated_Date"] = DateTime.UtcNow;

                dataTable.Rows.Add(row);
                currentLogicalRow++;
            }

            return dataTable;
        }

        private static DataTable CreateStagingDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("File_ID", typeof(long));
            dt.Columns.Add("Row_Number", typeof(int));
            dt.Columns.Add("GUID", typeof(string));
            dt.Columns.Add("Employment_Status", typeof(string));
            dt.Columns.Add("Work_Office", typeof(string));
            dt.Columns.Add("Line_Of_Service", typeof(string));
            dt.Columns.Add("Grade", typeof(string));
            dt.Columns.Add("Portfolio_Required", typeof(string));
            dt.Columns.Add("Country_Code", typeof(string));
            dt.Columns.Add("Validation_Status", typeof(string));
            dt.Columns.Add("Processing_Status", typeof(string));
            dt.Columns.Add("Created_Date", typeof(DateTime));
            dt.Columns.Add("Updated_Date", typeof(DateTime));
            return dt;
        }
    }
}