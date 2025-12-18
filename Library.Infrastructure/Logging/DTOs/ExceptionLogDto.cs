
using Library.Infrastructure.Logging.Models;
using System.ComponentModel.DataAnnotations;
using LogLevel = Library.Infrastructure.Logging.Models.LogLevel;

namespace Library.Infrastructure.Logging.DTOs
{
    public class ExceptionLogDto
    {
        [Required]
        public Guid Guid { get; set; }

        [Required(ErrorMessage = "Exception message is required.")]
        [StringLength(1000, ErrorMessage = "Exception message cannot exceed 1000 characters.")]
        public required string ExceptionMessage { get; set; }

        [Required(ErrorMessage = "Stack trace is required.")]
        [StringLength(4000, ErrorMessage = "Stack trace cannot exceed 4000 characters.")]
        public required string StackTrace { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
        public required string ServiceName { get; set; }

        [Required]
        public LogLevel Level { get; set; } = LogLevel.Exception; //default

        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}
