using System.ComponentModel.DataAnnotations;

namespace Library.Common.RabbitMqMessages.LoggingMessages
{
    public class WarningLogMessage
    {
        [Required]
        public Guid Guid { get; set; }

        [Required(ErrorMessage = "Request message is required.")]
        [StringLength(1000, ErrorMessage = "Request message cannot exceed 1000 characters.")]
        public required string Request { get; set; }

        [Required(ErrorMessage = "Warning message is required.")]
        [StringLength(1000, ErrorMessage = "Warning message cannot exceed 1000 characters.")]
        public required string WarningMessage { get; set; }

        [StringLength(4000, ErrorMessage = "Response body cannot exceed 4000 characters.")]
        public string? Response { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters.")]
        public required string ServiceName { get; set; }

        [Required]
        public required string Level { get; set; } = MyLogLevel.Warning;

        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}
