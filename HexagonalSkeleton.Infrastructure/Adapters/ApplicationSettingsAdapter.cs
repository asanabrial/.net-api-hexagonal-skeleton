using HexagonalSkeleton.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace HexagonalSkeleton.Infrastructure.Adapters
{
    public class ApplicationSettingsAdapter : IApplicationSettings
    {
        private readonly IConfiguration _configuration;

        public ApplicationSettingsAdapter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Secret => _configuration["AppSettings:Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        public string Issuer => _configuration["AppSettings:Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        public string Audience => _configuration["AppSettings:Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        public string Pepper => _configuration["AppSettings:Pepper"] ?? throw new InvalidOperationException("Pepper not configured");
    }
}
