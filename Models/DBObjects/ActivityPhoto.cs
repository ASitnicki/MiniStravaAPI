using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniStrava.Models.DBObjects
{
    [Table("ActivityPhotos")]
    public class ActivityPhoto
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        [Required, MaxLength(1024)]
        public string Url { get; set; } = null!;

        public DateTimeOffset UploadedAt { get; set; }
    }
}
