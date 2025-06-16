using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.CommonCore.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    /// <summary>
    /// Authentication service adapter implementing the domain port
    /// Handles JWT token generation, password validation, and authentication concerns
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IApplicationSettings _appSettings;
        private readonly IUserReadRepository _userReadRepository;

        public AuthenticationService(IApplicationSettings appSettings, IUserReadRepository userReadRepository)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        }

        /// <summary>
        /// Generates a JWT token for authenticated user
        /// </summary>
        public async Task<string> GenerateJwtTokenAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));

            var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));

            if (user.IsDeleted)
                throw new InvalidOperationException("Cannot generate token for deleted user");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email.Value),
                    new Claim(ClaimTypes.Name, user.FullName.GetFullName()),
                    new Claim("phone", user.PhoneNumber.Value)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validates user credentials against stored hash
        /// </summary>
        public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var user = await _userReadRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null || user.IsDeleted)
                return false;

            var hashedPassword = HashPassword(password, user.PasswordSalt ?? string.Empty);
            return hashedPassword == user.PasswordHash;
        }

        /// <summary>
        /// Hashes password with salt and pepper for secure storage
        /// </summary>
        public string HashPassword(string password, string salt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

            var pepper = _appSettings.Pepper ?? 
                throw new InvalidOperationException("Pepper configuration is required for security");
            
            return PasswordHasher.ComputeHash(password, salt, pepper) ?? 
                throw new InvalidOperationException("Password hash generation failed");
        }

        /// <summary>
        /// Generates a cryptographically secure salt for password hashing
        /// </summary>
        public string GenerateSalt()
        {
            return PasswordHasher.GenerateSalt();
        }
    }
}
