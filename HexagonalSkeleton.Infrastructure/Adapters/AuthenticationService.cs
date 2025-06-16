using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.CommonCore.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HexagonalSkeleton.Infrastructure.Adapters
{    public class AuthenticationService : IAuthenticationService
    {
        private readonly IApplicationSettings _appSettings;
        private readonly IUserReadRepository _userReadRepository;

        public AuthenticationService(IApplicationSettings appSettings, IUserReadRepository userReadRepository)
        {
            _appSettings = appSettings;
            _userReadRepository = userReadRepository;
        }

        public async Task<string> GenerateJwtTokenAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}".Trim())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userReadRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
                return false;

            var hashedPassword = HashPassword(password, user.PasswordSalt ?? string.Empty);
            return hashedPassword == user.PasswordHash;
        }        public string HashPassword(string password, string salt)
        {
            var pepper = _appSettings.Pepper ?? 
                throw new InvalidOperationException("Pepper configuration is required");
            
            return PasswordHasher.ComputeHash(password, salt, pepper) ?? 
                throw new InvalidOperationException("Password hash generation failed");
        }

        public string GenerateSalt()
        {
            return PasswordHasher.GenerateSalt();
        }
    }
}
