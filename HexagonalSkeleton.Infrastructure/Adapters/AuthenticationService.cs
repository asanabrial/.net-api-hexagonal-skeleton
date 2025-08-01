using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using HexagonalSkeleton.Infrastructure.Auth;
using Microsoft.Extensions.Logging;
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
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IApplicationSettings appSettings, 
            IUserReadRepository userReadRepository,
            IUserWriteRepository userWriteRepository,
            ILogger<AuthenticationService> logger)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
            _userWriteRepository = userWriteRepository ?? throw new ArgumentNullException(nameof(userWriteRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }        /// <summary>
        /// Generates a JWT token for authenticated user with expiration information
        /// </summary>
        public async Task<TokenInfo> GenerateJwtTokenAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);
            
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found during token generation.", userId);
                throw new InvalidOperationException($"User with ID {userId} not found. Cannot generate token for non-existent user.");
            }

            if (user.IsDeleted)
                throw new InvalidOperationException("Cannot generate token for deleted user");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_appSettings.Secret);
            
            var expiresAt = DateTime.UtcNow.AddDays(7);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email.Value),
                    new Claim(ClaimTypes.Name, user.FullName.GetFullName()),
                    new Claim("phone", user.PhoneNumber.Value)
                }),
                Expires = expiresAt,
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            return new TokenInfo(tokenString, expiresAt);
        }

        /// <summary>
        /// Validates user credentials against stored hash
        /// Accesses command-side repository for consistent authentication data
        /// </summary>
        public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            _logger.LogInformation("Validating credentials for email: {Email}", email);
            
            // Access the write repository directly for authentication
            // This ensures we have consistent authentication data 
            var user = await _userWriteRepository.GetUserByEmailAsync(email, cancellationToken);
            
            if (user == null)
            {
                _logger.LogWarning("User not found for authentication with email: {Email}", email);
                return false;
            }

            var passwordHash = user.PasswordHash;
            var passwordSalt = user.PasswordSalt;
            
            _logger.LogInformation("User found for validation: ID={UserId}, HasSalt={HasSalt}, HasHash={HasHash}", 
                user.Id, !string.IsNullOrWhiteSpace(passwordSalt), !string.IsNullOrWhiteSpace(passwordHash));

            // If salt is null or empty, authentication will always fail since a proper salt is required
            if (string.IsNullOrWhiteSpace(passwordSalt))
            {
                _logger.LogWarning("Salt is null or empty for user: {Email}", email);
                return false;
            }

            try
            {
                var hashedPassword = HashPassword(password, passwordSalt);
                var isMatch = hashedPassword == passwordHash;
                _logger.LogInformation("Password validation result: {Result} for user {Email}", isMatch ? "Success" : "Failed", email);
                
                if (!isMatch)
                {
                    _logger.LogWarning("Password mismatch for user {Email}. Hash lengths - Input: {InputHashLength}, Stored: {StoredHashLength}", 
                        email, hashedPassword?.Length ?? 0, passwordHash?.Length ?? 0);
                }
                
                return isMatch;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for user: {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Hashes password with salt and pepper for secure storage
        /// </summary>
        public string HashPassword(string password, string salt)
        {
            _logger.LogInformation("Hashing password with salt: {SaltLength} characters", salt?.Length ?? 0);
            
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Password is null or empty");
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }
            
            if (string.IsNullOrWhiteSpace(salt))
            {
                _logger.LogWarning("Salt is null or empty");
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));
            }

            _logger.LogInformation("Getting pepper from application settings");
            var pepper = _appSettings.Pepper;
            
            if (string.IsNullOrWhiteSpace(pepper))
            {
                _logger.LogError("Pepper configuration is missing");
                throw new InvalidOperationException("Pepper configuration is required for security");
            }
            
            _logger.LogInformation("Computing hash with password, salt, and pepper");
            var hash = PasswordHasher.ComputeHash(password, salt, pepper);
            
            if (hash == null)
            {
                _logger.LogError("Password hash generation failed");
                throw new InvalidOperationException("Password hash generation failed");
            }
            
            _logger.LogInformation("Successfully generated password hash: {HashLength} characters", hash.Length);
            return hash;
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
