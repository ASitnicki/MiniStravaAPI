using MiniStrava.Models.DBObjects;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Repositories;
using MiniStrava.Utils;
using System.Text.RegularExpressions;

namespace MiniStrava.Services
{
    public interface ILoginService
    {
        public RegisterResponse Register(RegisterRequests request);
        public LoginResponse Login(LoginRequest request);
    }
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        public LoginService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public RegisterResponse Register(RegisterRequests request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Login or password cannot be empty."
                };
            }
            if (request.Login.Length < 7)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Login is too short."
                };
            }
            string loginPattern = "^[a-zA-Z0-9]+$";
            if (!Regex.IsMatch(request.Login, loginPattern))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Login contains restricted characters."
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
            string passwordPattern1 = "^(?=.?[A-Z])$";
            if (!Regex.IsMatch(request.Password, passwordPattern1))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one upper character."
                };
            }
            string passwordPattern2 = "^(?=.?[a-z])$";
            if (!Regex.IsMatch(request.Password, passwordPattern2))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one lower character."
                };
            }
            string passwordPattern3 = "^(?=.?[0-9])$";
            if (!Regex.IsMatch(request.Password, passwordPattern3))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one digit."
                };
            }
            string passwordPattern4 = "^(?=.?[#?!@$%^&*-])$";
            if (!Regex.IsMatch(request.Password, passwordPattern4))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Password must contain at least one special character (#?!@$%^&*-)."
                };
            }
            User? user = _userRepository.GetByLogin(request.Login);
            if (user != null) 
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Login is already taken."
                };
            }
            string salt = PasswordTools.GenerateSalt();
            _userRepository.Add(new User 
            { 
                Login = request.Login, 
                HashPassword = PasswordTools.GenerateHash(request.Password, salt),
                Salt = salt
            });
            return new RegisterResponse
            {
                Success = true,
                Message = ""
            };
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
            User? user = _userRepository.GetByLogin(request.Login);
            if (user == null || PasswordTools.VerifyPassword(request.Password, user.Salt, user.HashPassword))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials."
                };
            }
            //TODO:
            //
            //Wygenerować JWT
        }
    }
}