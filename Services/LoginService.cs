using MiniStrava.Models.DBObjects;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Repositories;
using MiniStrava.Utils;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniStrava.Services
{
    public interface ILoginService
    {
        public RegisterResponse Register(RegisterRequests request);
        public LoginResponse Login(LoginRequest request);
        public List<User> GetUsers();
        public RegisterResponse ChangePassword(ChangePasswordRequest request);
    }
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _http;

        public LoginService(IUserRepository userRepository, IJwtService jwtService, IHttpContextAccessor http)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _http = http;
        }
        public RegisterResponse Register(RegisterRequests request)
        {
            Console.WriteLine("krok1");
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email or password cannot be empty."
                };
            }
            Console.WriteLine("krok2");
            string EmailPattern = "^\\S+@\\S+\\.\\S+$";
            if (!Regex.IsMatch(request.Email, EmailPattern))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email is invalid."
                };
            }
            if (request.Password.Length < 7)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password is too short."
                };
            }
            string passwordPatternUpperChar = "^(?=.*[A-Z]).+$";
            if (!Regex.IsMatch(request.Password, passwordPatternUpperChar))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one upper character."
                };
            }
            string passwordPatternLowerChar = "^(?=.*[a-z]).*$";
            if (!Regex.IsMatch(request.Password, passwordPatternLowerChar))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one lower character."
                };
            }
            string passwordPatternOneDigit = "^(?=.*\\d).+$";
            if (!Regex.IsMatch(request.Password, passwordPatternOneDigit))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one digit."
                };
            }
            //string passwordPatternSpecialChar = "^(?=.?[#?!@$%^&*-])$";
            string passwordPatternSpecialChar = "^(?=.*[^A-Za-z0-9]).+$";
            if (!Regex.IsMatch(request.Password, passwordPatternSpecialChar))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one special character (#?!@$%^&*-)."
                };
            }
            Console.WriteLine("krok3");
            User? user = _userRepository.GetByEmail(request.Email);
            if (user != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email is already taken."
                };
            }
            Console.WriteLine("krok4");
            string salt = PasswordTools.GenerateSalt();
            _userRepository.Add(new User
            {
                Email = request.Email,
                PasswordHash = Encoding.UTF8.GetBytes(PasswordTools.GenerateHash(request.Password, salt)),
                PasswordSalt = Encoding.UTF8.GetBytes(salt),
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                HeightCm = request.HeightCm,
                WeightKg = (decimal)request.WeightKg,
                AvatarUrl = request.AvatarUrl,
                IsAdmin = false,
                PreferredLanguage = request.PreferredLanguage,
                CreatedAt = DateTimeOffset.Now,
            });
            Console.WriteLine("krok5");
            return new RegisterResponse
            {
                Success = true,
                Message = ""
            };
        }

        public RegisterResponse ChangePassword(ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return new RegisterResponse { Success = false, Message = "CurrentPassword and NewPassword are required." };
            }

            var email = _http.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return new RegisterResponse { Success = false, Message = "Unauthorized." };
            }

            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                return new RegisterResponse { Success = false, Message = "User not found." };
            }

            var saltStr = Encoding.UTF8.GetString(user.PasswordSalt);
            var hashStr = Encoding.UTF8.GetString(user.PasswordHash);

            if (!PasswordTools.VerifyPassword(request.CurrentPassword, saltStr, hashStr))
            {
                return new RegisterResponse { Success = false, Message = "Current password is incorrect." };
            }

            //string passwordPatternSpecialChar = "^(?=.?[#?!@$%^&*-])$";
            string passwordPatternSpecialChar = "^(?=.*[^A-Za-z0-9]).+$";
            if (!Regex.IsMatch(request.NewPassword, passwordPatternSpecialChar))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one special character (#?!@$%^&*-)."
                };
            }

            var newSalt = PasswordTools.GenerateSalt();
            user.PasswordSalt = Encoding.UTF8.GetBytes(newSalt);
            user.PasswordHash = Encoding.UTF8.GetBytes(PasswordTools.GenerateHash(request.NewPassword, newSalt));

            user.MustChangePassword = false; // <-- najważniejsze dla “pierwszego logowania”

            
            _userRepository.Update(user);

            return new RegisterResponse { Success = true, Message = "" };
        }
        public LoginResponse Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Login and password are required."
                };
            }
            User? user = _userRepository.GetByEmail(request.Login);
            if (user == null || !PasswordTools.VerifyPassword(request.Password, Encoding.UTF8.GetString(user.PasswordSalt), Encoding.UTF8.GetString(user.PasswordHash)))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials."
                };
            }
            string jwtToken = _jwtService.GenerateToken(request.Login, user.IsAdmin);
            return new LoginResponse
            {
                Success = true,
                Message = "",
                JWTToken = jwtToken,
                MustChangePassword = user.MustChangePassword
            };


        }
        public List<User> GetUsers()
        {
            return _userRepository.GetAll();
        }
    }
}