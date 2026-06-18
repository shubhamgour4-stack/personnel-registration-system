using System;

namespace PRS.Core.Entities
{
    public class MftFileStaging
    {
        public long StagingId { get; set; }
        public long FileId { get; set; }
        public int RowNumber { get; set; }
        public string? Guid { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? WorkOffice { get; set; }
        public string? LineOfService { get; set; }
        public string? Grade { get; set; }
        public string? PortfolioRequired { get; set; }
        public char CountryCode { get; set; } // Tracks file origin country map
        public string ValidationStatus { get; set; } = "PENDING";
        public string? ValidationMessage { get; set; }
        public string ProcessingStatus { get; set; } = "RECEIVED";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Links staging records back to parent metadata
        public virtual MftFileHistory FileHistory { get; set; } = null!;
    }
}