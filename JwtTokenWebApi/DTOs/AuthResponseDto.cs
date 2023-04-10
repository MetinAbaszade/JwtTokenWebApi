namespace JwtTokenWebApi.DTOs
{
    public class AuthResponseDto
    {
        public AuthResponseDto() { }

        public AuthResponseDto(string message) => Message = message;

        public AuthResponseDto(bool isSuccessfull, string message, string token)
        {
            IsSuccessfull = isSuccessfull;
            Message = message;
            Token = token;
        }

        public bool IsSuccessfull { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenExpires { get; set; }
    }
}
