namespace MiniStrava.Models.DBObjects
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string HashPassword { get; set; }
        public string Salt { get; set; }
    }
}
