using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniStrava.Models.DBObjects
{
    public enum SyncStatus
    {
        running,
        completed,
        failed
    }

    [Table("SyncSessions")]
    public class SyncSession
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public DateTimeOffset StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        [Required, MaxLength(50)]
        public SyncStatus Status { get; set; } // zapis do NVARCHAR przez konwersję
    }
}
