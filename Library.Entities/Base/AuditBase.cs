using System.ComponentModel.DataAnnotations;

namespace Library.Entities.Base
{
    //Base Parent Class
    public abstract class AuditBase
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CreatedByUserId must be positive.")]
        public int? CreatedByUserId { get; set; } //nullable

        [Required]
        public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Range(1, int.MaxValue, ErrorMessage = "LastModifiedByUserId must be positive.")]
        public int? LastModifiedByUserId { get; set; } //nullable
        public DateOnly? LastModifiedDate { get; set; } //nullable
        public bool IsArchived { get; set; } = false;

        [Range(1, int.MaxValue, ErrorMessage = "ArchivedByUserId must be positive.")]
        public int? ArchivedByUserId { get; set; }
        public DateOnly? ArchivedDate { get; set; }

    }
}
