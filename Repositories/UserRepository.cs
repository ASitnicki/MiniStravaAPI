using MiniStrava.Models.DBObjects;

namespace MiniStrava.Repositories
{
    public interface IUserRepository
    {
        public User? GetByLogin(string login);
        public void Add(User user);
        public User? Test();
    }
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public User? GetByLogin(string login)
        {
            User user = _context.Users.First(user => user.Email == login);
            return user;
        }
        public void Add(User user)
        {
            _context.Users.Add(user);
        }
        public User? Test()
        {
            return _context.Users.FirstOrDefault();
        }
    }
}
