using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace HexagonalSkeleton.API.Config
{
    public static class AuthServiceExtension
    {
        /// <summary>
        /// Extension method for IServiceCollection to add and configure authentication services.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration manager</param>
        /// <returns>Return the service collection for further configuration.</returns>
        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("AppSettings:Jwt");
            var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

            // Add and configure JWT authentication.
            services.AddAuthentication(o =>
            {
                // Set default schemes for authentication, challenge, and the default scheme.
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {                // Set the parameters for token validation.
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    // Set the valid issuer and audience from configuration.
                    ValidIssuer = issuer,
                    ValidAudience = audience,

                    // Set the symmetric security key for signing the token.
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

                    // Set validation rules: issuer, audience, lifetime, and issuer signing key should be validated.
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, // Enable lifetime validation
                    ValidateIssuerSigningKey = true,
                    
                    // Add these for better token validation
                    ClockSkew = TimeSpan.Zero, // Remove default 5 minute clock skew
                    RequireExpirationTime = true,
                    RequireSignedTokens = true
                };

                // Enable detailed logging for JWT authentication in development
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures with details
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError("JWT Authentication failed: {Exception}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        // Log successful token validation
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        
                        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
                        {
                            logger.LogInformation("JWT Token validated successfully for user: {UserId}", userId);
                            
                            // Verify user still exists and is active
                            using var scope = context.HttpContext.RequestServices.CreateScope();
                            var userReadRepository = scope.ServiceProvider.GetRequiredService<HexagonalSkeleton.Domain.Ports.IUserReadRepository>();
                            
                            try
                            {
                                var user = await userReadRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);
                                
                                if (user == null)
                                {
                                    logger.LogWarning("JWT Token validation failed: User {UserId} no longer exists or has been deleted", userId);
                                    context.Fail("User no longer exists or has been deleted");
                                    return;
                                }
                                
                                logger.LogDebug("User {UserId} validation successful: user is active", userId);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error validating user status for JWT token: {UserId}", userId);
                                context.Fail("Error validating user status");
                                return;
                            }
                        }
                        else
                        {
                            logger.LogWarning("JWT Token validation failed: Invalid or missing user ID claim");
                            context.Fail("Invalid user ID in token");
                        }
                    },
                    OnChallenge = context =>
                    {
                        // Log authorization challenges
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("JWT Authentication challenge: {Error} - {ErrorDescription}", 
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
