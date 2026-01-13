using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace MiniStrava.Models.DBObjects
{
    [Table("TrackPoints")]
    public class TrackPoint
    {
        [Key]
        public long Id { get; set; } // BIGINT IDENTITY

        [Required]
        public Guid ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        [Required]
        public int Sequence { get; set; } // 1..N

        [Required]
        public DateTimeOffset Timestamp { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Column(TypeName = "decimal(7,2)")]
        public decimal? ElevationMeters { get; set; }

        [Column(TypeName = "decimal(6,3)")]
        public decimal? SpeedMps { get; set; }
    }
}
