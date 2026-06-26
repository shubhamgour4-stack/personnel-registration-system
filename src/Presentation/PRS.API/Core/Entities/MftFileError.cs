using System;

namespace PRS.Core.Entities
{
    public class MftFileError
    {
        public long ErrorId { get; set; }
        public long FileId { get; set; }
        public int RowNumber { get; set; }
        public string? Guid { get; set; }
        public string ErrorMessage { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        public virtual MftFileHistory FileHistory { get; set; } = null!;
    }
}