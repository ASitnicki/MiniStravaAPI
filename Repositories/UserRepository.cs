using MiniStrava.Models.DBObjects;

namespace MiniStrava.Repositories
{
    public interface IUserRepository
    {
        public User? GetByLogin(string login);
        public void Add(User user);
    }
    public class UserRepository : IUserRepository
    {
        private static readonly List<User> _users = new List<User>();
        public User? GetByLogin(string login)
        {
            foreach (User user in _users) 
            {
                if (user.Login == login)
                {
                    return user;
                }
            }
            return null;
        }
        public void Add(User user)
        {
            _users.Add(user);
        }
    }
}
