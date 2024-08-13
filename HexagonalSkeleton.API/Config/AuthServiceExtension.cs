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
        public static IServiceCollection AddAuthentication(this IServiceCollection services, AppSettings configuration)
        {
            // Add and configure JWT authentication.
            services.AddAuthentication(o =>
            {
                // Set default schemes for authentication, challenge, and the default scheme.
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                // Set the parameters for token validation.
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    // Set the valid issuer and audience from configuration.
                    ValidIssuer = configuration.Jwt.Issuer,
                    ValidAudience = configuration.Jwt.Audience,

                    // Set the symmetric security key for signing the token.
                    IssuerSigningKey = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(configuration.Jwt.Secret)),

                    // Set validation rules: issuer, audience, lifetime, and issuer signing key should be validated.
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                };
            });

            return services;
        }
    }
}
