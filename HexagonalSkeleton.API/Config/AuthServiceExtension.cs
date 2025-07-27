using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
            });

            return services;
        }
    }
}
