
using System.ComponentModel.DataAnnotations;
using Library.Entities.Base;

namespace Library.Entities.Models
{
    public class Publisher : AuditBase, IArchivable
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;
        public List<InventoryRecord> InventoryRecords { get; set; } = new();
    }
}
