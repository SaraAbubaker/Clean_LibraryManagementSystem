
using System.ComponentModel.DataAnnotations;

namespace Library.Infrastructure.Logging.DTOs
{
    public class MessageLogDto
    {
        [Required]
        public Guid Guid { get; set; }

        [Required(ErrorMessage = "Request message is required.")]
        [StringLength(1000, ErrorMessage = "Request message cannot exceed 1000 characters.")]
        public required string Request { get; set; }

        public string? Response { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
        public required string ServiceName { get; set; }

        [Required]
        public required string Level { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}
