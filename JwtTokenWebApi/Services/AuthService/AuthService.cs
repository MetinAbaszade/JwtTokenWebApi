using JwtTokenWebApi.Data;
using JwtTokenWebApi.Data.DataAccess.Abstract;
using JwtTokenWebApi.Data.DataAccess.Concrete.EntityFramework;
using JwtTokenWebApi.DTOs;
using JwtTokenWebApi.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;
using System.Numerics;
using System.Threading.Tasks;

namespace JwtTokenWebApi.Services.AuthService
{
    public class AuthService : IAuthSevice
    {
        private readonly IUserDal _userDal;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserDal userDal, 
                           IConfiguration configuration, 
                           IHttpContextAccessor httpContextAccessor)
        {
            _userDal = userDal;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDto> LoginUser(UserDto request)
        {
            var Users = await _userDal.GetList();
            var user = Users.FirstOrDefault(u => u.UserName == request.UserName);

            if (user == null)
            {
                return new AuthResponseDto("User Not Found!");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return new AuthResponseDto("Password is Wrong!");
            }

            var token = CreateToken(user);
            var refreshToken = GenerateRefreshToken();
            await SetRefreshToken(refreshToken, user);

            return new AuthResponseDto()
            {
                IsSuccessfull = true,
                Token = token,
                RefreshToken = refreshToken.Token,
                TokenExpires = refreshToken.Expires
            };
        }

        public async Task<User> RegisterUser(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User()
            {
                UserName = request.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            await _userDal.Add(user);

            return user;
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Appsettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Created = DateTime.Now,
                Expires = DateTime.Now.AddDays(1)
            };

            return refreshToken;
        }

        private async Task SetRefreshToken(RefreshToken refreshToken, User user)
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Expires = refreshToken.Expires
            };

            _httpContextAccessor?.HttpContext?.Response
                .Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            user.RefreshToken = refreshToken.Token;
            user.TokenCreated = refreshToken.Created;
            user.TokenExpires = refreshToken.Expires;

            await _userDal.Update(user);
        }

        public async Task<AuthResponseDto> RefreshToken()
        {
            var Users = await _userDal.GetList();
            var refreshToken = _httpContextAccessor?.HttpContext?.Request.Cookies["refreshToken"];
            var user = Users.FirstOrDefault(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                return new AuthResponseDto("Invalid RefreshToken!");
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return new AuthResponseDto("Token has expired!");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            await SetRefreshToken(newRefreshToken, user);

            return new AuthResponseDto
            {
                IsSuccessfull = true,
                Message = "Token Successfully refreshed!",
                Token = token,
                RefreshToken = newRefreshToken.Token,
                TokenExpires = newRefreshToken.Expires
            };
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // If we don't use Salt, then hashed password will be same always.
            // That's why we add different salt each time to the password and hash them as combined.
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
                // SequenceEqual vs Equals
            }
        }


        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 1000000).ToString();
        }


        public bool SendVerificationCode(string recipientemail)
        {
            try
            {
                // Configure the SMTP client
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587; // Gmail uses port 587 for TLS connections
                smtpClient.Credentials = new NetworkCredential("metinabaszade.h@gmail.com", "ghzwjllkuyvkfwsr");
                smtpClient.EnableSsl = true; // Enable SSL for Gmail

                MailMessage message = new MailMessage();
                message.From = new MailAddress("metinabaszade.h@gmail.com");
                message.Subject = "Verification Code for CarUniverse Account (Expires in 2 Minutes)";
                message.To.Add(recipientemail);
                message.IsBodyHtml = true;

                var VerificationCode = GenerateVerificationCode();
                message.Body = @$"<div style=""font-size: 20px;"">
Thank you for choosing CarUniverse as your preferred platform for all your automotive needs. As part of our security measures, we require all users to verify their account before gaining full access to our services.<br><br>

Your verification code is: <b>{VerificationCode}</b><br><br>

Please enter this code in the designated field on the CarUniverse app or website to complete the verification process. Please note that this code is only valid for the next 2 minutes. After that, it will expire and you will need to request a new code.<br><br>

If you did not request this verification code, please disregard this message.<br><br>

If you have any questions or concerns, please do not hesitate to contact our customer support team for assistance. We are available 24/7 to assist you.<br>

Thank you for choosing CarUniverse.<br><br>

Best regards,<br>
The CarUniverse Team
</div>";

              
      

                 
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
