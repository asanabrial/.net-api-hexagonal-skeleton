using AutoMapper;
using HexagonalSkeleton.API.Mapping;
using HexagonalSkeleton.API.Models;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.Mapping
{
    public class ApiMappingProfileTest
    {
        private readonly IMapper _mapper;

        public ApiMappingProfileTest()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ApiMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Should_Map_RegisterDto_To_UserRegistrationResponse()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600,
                User = new RegisterUserInfoDto
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com",
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            // Act
            var result = _mapper.Map<UserRegistrationResponse>(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(registerDto.User.Id, result.UserId);
            Assert.Equal(registerDto.User.Email, result.Email);
            Assert.Equal(registerDto.User.FirstName, result.FirstName);
            Assert.Equal(registerDto.User.LastName, result.LastName);
            Assert.Equal("User registered successfully", result.Message);
        }

        [Fact]
        public void Should_Validate_AutoMapper_Configuration()
        {
            // Act & Assert - This will throw if there are mapping configuration errors
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ApiMappingProfile>();
            });
            
            config.AssertConfigurationIsValid();
        }
    }
}
