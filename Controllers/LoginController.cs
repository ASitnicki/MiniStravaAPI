using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Services;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

       [HttpPost("register")]
       public IActionResult Register(RegisterRequests request)
        {
            RegisterResponse response = _loginService.Register(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [Authorize]
        [HttpGet("test1")]
        public IActionResult Test1()
        {
            return Ok(_loginService.GetUsers());
        }
        [HttpGet("test2")]
        public IActionResult Test2()
        {
            return Ok(_loginService.GetUsers());
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            LoginResponse response = _loginService.Login(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
