using HexagonalSkeleton.API.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AutoFixture;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Test.Unit
{
    public class UnitTestFixture : IUnitTestFixture
    {
        public UnitTestFixture()
        {
            Fixture = new Fixture();
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));

            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Configure AutoFixture for value objects
            Fixture.Register(() => new Email("test@example.com"));
            Fixture.Register(() => new FullName("John", "Doe"));
            Fixture.Register(() => new PhoneNumber("123456789"));
            Fixture.Register(() => new Location(40.7128, -74.0060));
        }

        public Fixture Fixture { get; set; }

        public IOptions<AppSettings> Settings { get; set; } = Options.Create(new AppSettings()
        {
            AllowedFileExtensions = [ ".jpg", ".jpeg", ".png", ".gif" ],
            ContentImgFolder = "img",
            ContentUserFolder = "user",
            Pepper = "48j7c43987h65v4",
            Jwt = new TokenSecuritySettings()
            {
                Secret = "484lñññññd768rmn765uneb5463fa338",
                Issuer = "Alexis",
                Audience = "HexagonalSkeleton"
            }
        });

        public bool ValidateToken(string tokenString) => new JwtSecurityTokenHandler().ValidateToken(tokenString,
            new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateTokenReplay = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Value.Jwt.Secret)),
                ValidAudience = Settings.Value.Jwt.Audience,
                ValidIssuer = Settings.Value.Jwt.Issuer,
            }, out _) is not null;
    }
}
