using System;
using System.Collections.Generic;

namespace PRS.Core.Entities
{
    public class MftFileHistory
    {
        public long FileId { get; set; }
        public string FileName { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public DateTime FileTimestamp { get; set; }
        public int RecordsReceived { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsFailed { get; set; }
        public string ProcessingStatus { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ProcessedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        // Relational navigation properties
        public virtual ICollection<MftFileStaging> StagingRecords { get; set; } = new List<MftFileStaging>();
        public virtual ICollection<MftFileError> ErrorRecords { get; set; } = new List<MftFileError>();
    }
}