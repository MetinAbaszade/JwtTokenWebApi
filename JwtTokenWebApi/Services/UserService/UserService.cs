using System.Security.Claims;

namespace JwtTokenWebApi.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserRole()
        {
            var result = string.Empty;
            result = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
            return result;
        }
    }
}
