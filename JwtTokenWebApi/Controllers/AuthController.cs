using JwtTokenWebApi.DTOs;
using JwtTokenWebApi.Entities.Concrete;
using JwtTokenWebApi.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtTokenWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthSevice _authSevice;
        private readonly IUserService _userService;

        public AuthController(IAuthSevice authSevice, IUserService userService)
        {
            _authSevice = authSevice;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(UserDto request)
        {
            var user = await _authSevice.RegisterUser(request);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(UserDto request)
        {
            var response = await _authSevice.LoginUser(request);
            if (response.IsSuccessfull)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet, Authorize]
        public ActionResult<object> GetUserRole()
        {
            var User = _userService.GetUserRole();

            return Ok(User);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var response = await _authSevice.RefreshToken();

            if (response.IsSuccessfull)
                return Ok(response);

            return BadRequest(response);
        }

    }
}
