using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.LoggingMessages
{
    public class FailedLogMessage
    {
        [Required]
        public Guid Guid { get; set; }


        [Required(ErrorMessage = "Original message is required.")]
        [StringLength(4000, ErrorMessage = "Original message cannot exceed 4000 characters.")]
        public required string OriginalMessage { get; set; }


        [Required(ErrorMessage = "Failed message is required.")]
        [StringLength(1000, ErrorMessage = "Failed message cannot exceed 1000 characters.")]
        public required string FailedMessage { get; set; }


        [StringLength(4000, ErrorMessage = "Stack trace cannot exceed 4000 characters.")]
        public string? StackTrace { get; set; }


        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
        public required string ServiceName { get; set; }

        [Required]
        public required string Level { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
