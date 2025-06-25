using HexagonalSkeleton.API.Identity;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HexagonalSkeleton.API.Models.Common;

namespace HexagonalSkeleton.API.Controllers
{    /// <summary>
    /// User management API controller
    /// Implements Hexagonal Architecture with CQRS pattern using MediatR
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController(ISender mediator, IMapper mapper) : ControllerBase
    {        /// <summary>
        /// Authenticate user and initiate login session
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication result with token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var command = mapper.Map<LoginCommand>(request);
            var result = await mediator.Send(command);
            return Ok(mapper.Map<LoginResponse>(result));
        }        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Created user information</returns>
        [HttpPost]        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]        [ProducesResponseType(StatusCodes.Status400BadRequest)]        public async Task<IActionResult> Register(CreateUserRequest request)
        {
            var command = mapper.Map<RegisterUserCommand>(request);
            var result = await mediator.Send(command);
            
            // Use AutoMapper for the entire response now that we have nested structure
            var response = mapper.Map<LoginResponse>(result);
            
            return Created("", response);
        }/// <summary>
        /// Update user information (full update)
        /// </summary>
        /// <param name="request">Complete user data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(UpdateUserRequest request)
        {
            var command = mapper.Map<UpdateUserCommand>(request);
            var result = await mediator.Send(command);
            return Ok(mapper.Map<UserResponse>(result));
        }        /// <summary>
        /// Update user profile (partial update)
        /// </summary>
        /// <param name="request">Profile update data</param>
        /// <returns>Update result</returns>
        [HttpPatch("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var command = mapper.Map<UpdateProfileUserCommand>(request);
            // Set the user ID from the authenticated user context
            command.Id = User.GetUserId();
            var result = await mediator.Send(command);
            return Ok(mapper.Map<UserResponse>(result));
        }        /// <summary>
        /// Permanently delete a user (hard delete)
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await mediator.Send(new HardDeleteUserCommand(id));
            return Ok(mapper.Map<DeleteUserResponse>(result));
        }        /// <summary>
        /// Soft delete a user (logical deletion)
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpPost("{id:int}/soft-delete")]
        [Authorize]
        [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await mediator.Send(new SoftDeleteUserCommand(id));
            return Ok(mapper.Map<DeleteUserResponse>(result));
        }        /// <summary>
        /// Get user by identifier
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User information</returns>
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await mediator.Send(new GetUserQuery(id));
            return Ok(mapper.Map<UserResponse>(result));
        }        /// <summary>
        /// Get current authenticated user's information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMe()
        {
            var result = await mediator.Send(new GetUserQuery(User.GetUserId()));
            return Ok(mapper.Map<UserResponse>(result));
        }        /// <summary>
        /// Get all users with advanced pagination and filtering support
        /// 
        /// Filtering Options:
        /// - SearchTerm: Unified search across first name, last name, email, and phone (partial matching)
        /// - Age filters: MinAge, MaxAge, OnlyAdults (18+)
        /// - Status filters: OnlyActive, OnlyCompleteProfiles
        /// - Location filters: Latitude, Longitude, RadiusInKm
        /// 
        /// Examples:
        /// - GET /api/user?searchTerm=john (finds users with "john" in name, email, or phone)
        /// - GET /api/user?searchTerm=@example.com (finds users with "@example.com" in email)
        /// - GET /api/user?searchTerm=john&amp;onlyAdults=true&amp;minAge=25 (combines filters)
        /// </summary>
        /// <param name="request">Pagination and filtering parameters</param>
        /// <returns>Paginated list of users matching the criteria</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUsersRequest request)
        {
            var query = mapper.Map<GetAllUsersQuery>(request);
            var result = await mediator.Send(query);
            return Ok(mapper.Map<PagedResponse<UserResponse>>(result));
        }        /// <summary>
        /// Find nearby adult users with complete profiles
        /// Demonstrates advanced filtering using Specification pattern
        /// </summary>
        /// <param name="latitude">Center latitude for search</param>
        /// <param name="longitude">Center longitude for search</param>
        /// <param name="radiusInKm">Search radius in kilometers (default: 50)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of nearby adult users with complete profiles</returns>
        [HttpGet("nearby-adults")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNearbyAdultUsers(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusInKm = 50,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new FindNearbyAdultUsersQuery(latitude, longitude, radiusInKm)
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            
            var result = await mediator.Send(query);
            return Ok(mapper.Map<PagedResponse<UserResponse>>(result));
        }
    }
}
