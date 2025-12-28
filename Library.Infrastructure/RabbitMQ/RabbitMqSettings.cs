
using System.ComponentModel.DataAnnotations;

namespace Library.Infrastructure.RabbitMQ
{
    public class RabbitMqSettings
    {
        [Required]
        [StringLength(100)]
        public required string HostName { get; set; }

        [Required]
        [StringLength(50)]
        public required string UserName { get; set; }

        [Required]
        [StringLength(50)]
        public required string Password { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; }

        [Required]
        [StringLength(100)]
        public required string MessageQueue { get; set; }

        [Required]
        [StringLength(100)]
        public required string ExceptionQueue { get; set; }

        [Required]
        [StringLength(100)]
        public required string FailedQueue { get; set; }
    }
}
