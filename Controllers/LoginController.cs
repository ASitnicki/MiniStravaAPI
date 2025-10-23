using Microsoft.AspNetCore.Mvc;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Services;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
    }
}
