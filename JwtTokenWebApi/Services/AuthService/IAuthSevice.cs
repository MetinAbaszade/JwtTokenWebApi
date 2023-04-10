using JwtTokenWebApi.DTOs;

namespace JwtTokenWebApi.Services.AuthService
{
    public interface IAuthSevice
    {
        public Task<User> RegisterUser(UserDto request);
        public Task<AuthResponseDto> LoginUser(UserDto request);
        public Task<AuthResponseDto> RefreshToken();
    }
}
