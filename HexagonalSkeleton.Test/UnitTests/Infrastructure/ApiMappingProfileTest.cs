using HexagonalSkeleton.API.Mapping;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using System.Reflection;
using Xunit;

namespace HexagonalSkeleton.Test.Unit.Mapping
{
    /// <summary>
    /// Tests for the hand-written API mappers (UserApiMappings / AuthApiMappings)
    /// that replaced the AutoMapper-based ApiMappingProfile.
    /// </summary>
    public class ApiMappingProfileTest
    {
        /// <summary>
        /// Builds a PagedQueryResult via its private constructor so the mappers can
        /// be tested without going through the domain factory pipeline.
        /// </summary>
        private static PagedQueryResult<T> BuildPagedResult<T>(
            IEnumerable<T> items,
            PaginationMetadata metadata)
        {
            var ctor = typeof(PagedQueryResult<T>).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(IEnumerable<T>), typeof(PaginationMetadata) },
                modifiers: null)!;

            return (PagedQueryResult<T>)ctor.Invoke(new object[] { items, metadata });
        }

        [Fact]
        public void ToRegistrationResponse_MapsNestedUserAndMessage()
        {
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

            var result = registerDto.ToRegistrationResponse();

            Assert.NotNull(result);
            Assert.Equal(registerDto.User.Id, result.UserId);
            Assert.Equal(registerDto.User.Email, result.Email);
            Assert.Equal(registerDto.User.FirstName, result.FirstName);
            Assert.Equal(registerDto.User.LastName, result.LastName);
            Assert.Equal("User registered successfully", result.Message);
        }

        [Fact]
        public void RegisterUserDto_ToResponse_MapsTokenAndNestedRegisterUserInfo()
        {
            var registerDto = new RegisterUserDto
            {
                AccessToken = "access-token",
                TokenType = "Bearer",
                ExpiresIn = 1800,
                User = new RegisterUserInfoDto
                {
                    Id = Guid.NewGuid(),
                    Email = "reg@example.com",
                    FirstName = "Reg",
                    LastName = "User",
                    PhoneNumber = "+100",
                    AboutMe = "hello",
                    CreatedAt = DateTime.UtcNow
                }
            };

            AuthenticatedRegistrationResponse result = registerDto.ToResponse();

            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal(1800, result.ExpiresIn);
            Assert.NotNull(result.User);
            Assert.Equal(registerDto.User.Id, result.User.Id);
            Assert.Equal(registerDto.User.Email, result.User.Email);
            Assert.Equal(registerDto.User.FirstName, result.User.FirstName);
            Assert.Equal(registerDto.User.PhoneNumber, result.User.PhoneNumber);
            Assert.Equal(registerDto.User.AboutMe, result.User.AboutMe);
        }

        [Fact]
        public void AuthenticationDto_ToResponse_MapsTokenAndNestedUserInfo()
        {
            var dto = new AuthenticationDto
            {
                AccessToken = "login-token",
                TokenType = "Bearer",
                ExpiresIn = 7200,
                User = new AuthenticatedUserDto
                {
                    Id = Guid.NewGuid(),
                    Email = "login@example.com",
                    FirstName = "Log",
                    LastName = "In",
                    Latitude = 10.5,
                    Longitude = -20.3,
                    CreatedAt = DateTime.UtcNow
                }
            };

            LoginResponse result = dto.ToResponse();

            Assert.Equal("login-token", result.AccessToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal(7200, result.ExpiresIn);
            Assert.NotNull(result.User);
            Assert.Equal(dto.User.Id, result.User.Id);
            Assert.Equal(dto.User.Email, result.User.Email);
            Assert.Equal(dto.User.Latitude, result.User.Latitude);
            Assert.Equal(dto.User.Longitude, result.User.Longitude);
        }

        [Fact]
        public void GetUserDto_ToResponse_MapsAllFlatProperties()
        {
            var dto = new GetUserDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Roe",
                Email = "jane@example.com",
                PhoneNumber = "+200",
                Birthdate = new DateTime(1990, 1, 1),
                Latitude = 1.1,
                Longitude = 2.2,
                AboutMe = "about",
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            UserResponse result = dto.ToResponse();

            Assert.Equal(dto.Id, result.Id);
            Assert.Equal(dto.FirstName, result.FirstName);
            Assert.Equal(dto.LastName, result.LastName);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.PhoneNumber, result.PhoneNumber);
            Assert.Equal(dto.Birthdate, result.Birthdate);
            Assert.Equal(dto.Latitude, result.Latitude);
            Assert.Equal(dto.Longitude, result.Longitude);
            Assert.Equal(dto.AboutMe, result.AboutMe);
            Assert.Equal(dto.LastLogin, result.LastLogin);
            Assert.Equal(dto.CreatedAt, result.CreatedAt);
            Assert.Equal(dto.UpdatedAt, result.UpdatedAt);
        }

        [Fact]
        public void UserProfileDto_ToResponse_MapsFlatProperties()
        {
            var dto = new UserProfileDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Pro",
                LastName = "File",
                Email = "pro@example.com"
            };

            UserResponse result = dto.ToResponse();

            Assert.Equal(dto.Id, result.Id);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.FirstName, result.FirstName);
        }

        [Fact]
        public void UpdateUserDto_ToResponse_MapsFlatProperties()
        {
            var dto = new UpdateUserDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Up",
                LastName = "Date",
                Email = "up@example.com"
            };

            UserResponse result = dto.ToResponse();

            Assert.Equal(dto.Id, result.Id);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.LastName, result.LastName);
        }

        [Fact]
        public void UserDeletionDto_ToResponse_MapsUserId()
        {
            var dto = new UserDeletionDto { UserId = Guid.NewGuid() };

            DeleteUserResponse result = dto.ToResponse();

            Assert.Equal(dto.UserId, result.UserId);
        }

        [Fact]
        public void LoginRequest_ToCommand_MapsCredentialsAndIgnoresRememberMe()
        {
            var request = new LoginRequest
            {
                Email = "user@example.com",
                Password = "secret",
                RememberMe = true
            };

            var command = request.ToCommand();

            Assert.Equal(request.Email, command.Email);
            Assert.Equal(request.Password, command.Password);
        }

        [Fact]
        public void CreateUserRequest_ToCommand_MapsAllProperties()
        {
            var request = new CreateUserRequest
            {
                FirstName = "New",
                LastName = "User",
                Email = "new@example.com",
                Password = "pass",
                PasswordConfirmation = "pass",
                PhoneNumber = "+300",
                Birthdate = new DateTime(2000, 5, 5),
                Latitude = 3.3,
                Longitude = 4.4,
                AboutMe = "bio"
            };

            var command = request.ToCommand();

            Assert.Equal(request.FirstName, command.FirstName);
            Assert.Equal(request.LastName, command.LastName);
            Assert.Equal(request.Email, command.Email);
            Assert.Equal(request.Password, command.Password);
            Assert.Equal(request.PasswordConfirmation, command.PasswordConfirmation);
            Assert.Equal(request.PhoneNumber, command.PhoneNumber);
            Assert.Equal(request.Birthdate, command.Birthdate);
            Assert.Equal(request.Latitude, command.Latitude);
            Assert.Equal(request.Longitude, command.Longitude);
            Assert.Equal(request.AboutMe, command.AboutMe);
        }

        [Fact]
        public void GetAllUsersRequest_ToQuery_MapsPagingAndFilters()
        {
            var request = new GetAllUsersRequest
            {
                PageNumber = 2,
                PageSize = 25,
                SearchTerm = "john",
                SortBy = "lastName",
                SortDirection = "desc",
                MinAge = 18,
                MaxAge = 65,
                OnlyAdults = true,
                OnlyActive = false,
                OnlyCompleteProfiles = true,
                Latitude = 5.5,
                Longitude = 6.6,
                RadiusInKm = 50
            };

            var query = request.ToQuery();

            Assert.Equal(request.PageNumber, query.PageNumber);
            Assert.Equal(request.PageSize, query.PageSize);
            Assert.Equal(request.SearchTerm, query.SearchTerm);
            Assert.Equal(request.SortBy, query.SortBy);
            Assert.Equal(request.SortDirection, query.SortDirection);
            Assert.Equal(request.MinAge, query.MinAge);
            Assert.Equal(request.MaxAge, query.MaxAge);
            Assert.Equal(request.OnlyAdults, query.OnlyAdults);
            Assert.Equal(request.OnlyActive, query.OnlyActive);
            Assert.Equal(request.OnlyCompleteProfiles, query.OnlyCompleteProfiles);
            Assert.Equal(request.Latitude, query.Latitude);
            Assert.Equal(request.Longitude, query.Longitude);
            Assert.Equal(request.RadiusInKm, query.RadiusInKm);
        }

        [Fact]
        public void UpdateUserRequest_ToCommand_MapsPropertiesAndId()
        {
            var request = new UpdateUserRequest
            {
                Id = Guid.NewGuid(),
                FirstName = "Edit",
                LastName = "Me",
                PhoneNumber = "+400",
                Birthdate = new DateTime(1995, 3, 3),
                Latitude = 7.7,
                Longitude = 8.8,
                AboutMe = "edited"
            };

            var command = request.ToCommand();

            Assert.Equal(request.Id, command.Id);
            Assert.Equal(request.FirstName, command.FirstName);
            Assert.Equal(request.LastName, command.LastName);
            Assert.Equal(request.PhoneNumber, command.PhoneNumber);
            Assert.Equal(request.Birthdate, command.Birthdate);
            Assert.Equal(request.Latitude, command.Latitude);
            Assert.Equal(request.Longitude, command.Longitude);
            Assert.Equal(request.AboutMe, command.AboutMe);
        }

        [Fact]
        public void UpdateUserRequest_ToCommand_NullBirthdate_FallsBackToDefault()
        {
            var request = new UpdateUserRequest
            {
                Id = Guid.NewGuid(),
                FirstName = "No",
                LastName = "Birthdate",
                PhoneNumber = "+401",
                Birthdate = null
            };

            var command = request.ToCommand();

            Assert.Equal(default, command.Birthdate);
        }

        [Fact]
        public void UpdateProfileRequest_ToCommand_MapsPropertiesAndLeavesIdDefault()
        {
            var request = new UpdateProfileRequest
            {
                FirstName = "Self",
                LastName = "Service",
                PhoneNumber = "+500",
                Birthdate = new DateTime(1988, 8, 8),
                AboutMe = "me"
            };

            var command = request.ToCommand();

            Assert.Equal(Guid.Empty, command.Id);
            Assert.Equal(request.FirstName, command.FirstName);
            Assert.Equal(request.LastName, command.LastName);
            Assert.Equal(request.PhoneNumber, command.PhoneNumber);
            Assert.Equal(request.Birthdate, command.Birthdate);
            Assert.Equal(request.AboutMe, command.AboutMe);
        }

        [Fact]
        public void UpdateProfileRequest_ToCommand_NullValues_FallBackToDefaults()
        {
            var request = new UpdateProfileRequest();

            var command = request.ToCommand();

            Assert.Equal(string.Empty, command.FirstName);
            Assert.Equal(string.Empty, command.LastName);
            Assert.Equal(string.Empty, command.PhoneNumber);
            Assert.Equal(string.Empty, command.AboutMe);
            Assert.Equal(default, command.Birthdate);
        }

        [Fact]
        public void PagedQueryResult_ToResponse_WithItemMapper_MapsItemsAndMetadata()
        {
            var items = new List<GetAllUsersDto>
            {
                new() { Id = Guid.NewGuid(), Email = "a@example.com", FirstName = "A" },
                new() { Id = Guid.NewGuid(), Email = "b@example.com", FirstName = "B" }
            };
            var metadata = new PaginationMetadata(
                PageNumber: 2,
                PageSize: 10,
                TotalCount: 42,
                TotalPages: 5,
                HasNextPage: true,
                HasPreviousPage: true);
            var paged = BuildPagedResult(items, metadata);

            var response = paged.ToResponse(dto => dto.ToResponse());

            Assert.Equal(2, response.Data.Count());
            Assert.Equal(42, response.TotalCount);
            Assert.Equal(2, response.PageNumber);
            Assert.Equal(10, response.PageSize);
            Assert.Collection(response.Data,
                first => Assert.Equal("a@example.com", first.Email),
                second => Assert.Equal("b@example.com", second.Email));
        }

        [Fact]
        public void PagedQueryResult_ToResponse_SameType_MapsItemsAndMetadata()
        {
            var items = new List<string> { "one", "two", "three" };
            var metadata = new PaginationMetadata(
                PageNumber: 1,
                PageSize: 3,
                TotalCount: 3,
                TotalPages: 1,
                HasNextPage: false,
                HasPreviousPage: false);
            var paged = BuildPagedResult(items, metadata);

            var response = paged.ToResponse();

            Assert.Equal(3, response.TotalCount);
            Assert.Equal(1, response.PageNumber);
            Assert.Equal(3, response.PageSize);
            Assert.Equal(items, response.Data);
        }
    }
}
