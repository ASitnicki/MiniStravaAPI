using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniStrava.Models.DBObjects
{
    [Table("ApiClients")]
    public class ApiClient
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(200)]
        public string? ClientName { get; set; }

        public byte[]? ApiKeyHash { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
