using JwtTokenWebApi.Entities.Abstract;

namespace JwtTokenWebApi.Entities.Concrete
{
    public class RefreshToken : IEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Expires { get; set; }
    }
}
