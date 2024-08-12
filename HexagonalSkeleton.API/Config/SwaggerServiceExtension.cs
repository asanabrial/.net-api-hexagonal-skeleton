using Microsoft.OpenApi.Models;

namespace HexagonalSkeleton.API.Config
{
    public static class SwaggerServiceExtension
    {
        /// <summary>
        /// Extension method for IServiceCollection to configure Swagger.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Return the service collection for further configuration.</returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            // Define the OpenAPI Security Scheme which describes how the API is protected.
            var securityScheme = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JSON Web Token based security",
            };

            // Define the OpenAPI Security Requirement which provides the list of required security schemes for the API.
            var securityReq = new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            };

            var contact = new OpenApiContact()
            {
                Name = "Alexis Sanabria Lopez",
                Url = new Uri("https://www.linkedin.com/in/alexis-sanabria-lopez/"),
                Email = "asanabrial@outlook.com"
            };

            var info = new OpenApiInfo()
            {
                Version = "v1",
                Title = "HexagonalSkeleton API",
                Description = "",
                Contact = contact,
            };

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", info);
                o.AddSecurityDefinition("Bearer", securityScheme);
                o.AddSecurityRequirement(securityReq);
            });

            return services;
        }
    }
}
