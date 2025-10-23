using System.Security.Cryptography;
using System.Text;

namespace MiniStrava.Utils
{
    public class PasswordTools
    {
        private const int saltSize = 16;
        private const int hashSize = 32;
        private const int iterations = 100000;

        public static string GenerateSalt()
        {
            byte[] salt = new byte[saltSize];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }
           return Convert.ToBase64String(salt);
        }
        public static string GenerateHash(string password, string salt)
        {
            byte[] hash;
            byte[] pass = Encoding.UTF8.GetBytes(password);
            byte[] sa = Encoding.UTF8.GetBytes(salt);
            using (Rfc2898DeriveBytes hasher =  new Rfc2898DeriveBytes(pass, sa, iterations, HashAlgorithmName.SHA256))
            {
                hash = hasher.GetBytes(hashSize);
            }
            return Convert.ToBase64String(hash);
        }
        public static bool VerifyPassword(string password, string salt, string hashPassword)
        {
            string hash = GenerateHash(password, salt);
            return hash == hashPassword;
        }
    }
}
