using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniStrava.Models.DBObjects
{
    public enum ActivityType
    {
        running,
        cycling,
        walking,
        hike,
        workout
    }

    [Table("Activities")]
    public class Activity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(200)]
        public string? Name { get; set; }

        [Required, MaxLength(50)]
        public ActivityType ActivityType { get; set; }   // zapis do NVARCHAR przez konwersję

        [Required]
        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset? EndTime { get; set; }

        public int? DurationSeconds { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DistanceMeters { get; set; } = 0;

        public int? AveragePaceSecPerKm { get; set; }

        [Column(TypeName = "decimal(6,3)")]
        public decimal? AverageSpeedMps { get; set; }

        public string? Notes { get; set; }

        [MaxLength(1024)]
        public string? PhotoUrl { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<TrackPoint> TrackPoints { get; set; } = new List<TrackPoint>();
        public ICollection<ActivityPhoto> ActivityPhotos { get; set; } = new List<ActivityPhoto>();
    }
}
