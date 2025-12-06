using Microsoft.AspNetCore.Http.HttpResults;
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

       [HttpPost(Name = "register")]
       public IActionResult Register(RegisterRequests request)
        {
            RegisterResponse response = _loginService.Register(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok(_loginService.Test());
        }
    }
}
