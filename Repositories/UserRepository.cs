using MiniStrava.Models.DBObjects;
using System;

namespace MiniStrava.Repositories
{
    public interface IUserRepository
    {
        public User? GetByEmail(string email);
        public void Add(User user);
        public List<User> GetAll();
    }
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public User? GetByEmail(string email)
        {
            User? user = _context.Users.FirstOrDefault(user => user.Email == email);
            return user;
        }
        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }
    }
}
