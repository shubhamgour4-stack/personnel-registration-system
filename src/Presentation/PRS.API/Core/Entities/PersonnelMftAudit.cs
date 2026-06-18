using System;

namespace PRS.Core.Entities
{
    public class PersonnelMftAudit
    {
        public long AuditId { get; set; }
        public string Guid { get; set; } = null!;
        public string UpdatedField { get; set; } = null!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string FileName { get; set; } = null!;
        public DateTime ProcessedDate { get; set; }
        public string UpdatedBy { get; set; } = null!;
    }
}