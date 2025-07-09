using HexagonalSkeleton.API.Identity;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Features.SocialNetwork.Queries;

namespace HexagonalSkeleton.API.Controllers.Features
{
    /// <summary>
    /// User Management Business Feature Controller
    /// Follows Screaming Architecture principles by focusing on administrative user management
    /// Handles CRUD operations, searches, and administrative functions
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public UserManagementController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Get user by identifier
        /// Core business operation: User Retrieval
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User information</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserQuery(id));
            return Ok(_mapper.Map<UserResponse>(result));
        }

        /// <summary>
        /// Get all users with advanced pagination and filtering support
        /// 
        /// Filtering Options:
        /// - SearchTerm: Unified search across first name, last name, email, and phone (partial matching)
        /// - Age filters: MinAge, MaxAge, OnlyAdults (18+)
        /// - Status filters: OnlyActive, OnlyCompleteProfiles
        /// - Location filters: Latitude, Longitude, RadiusInKm
        /// 
        /// Examples:
        /// - GET /api/users?searchTerm=john (finds users with "john" in name, email, or phone)
        /// - GET /api/users?searchTerm=@example.com (finds users with "@example.com" in email)
        /// - GET /api/users?searchTerm=john&amp;onlyAdults=true&amp;minAge=25 (combines filters)
        /// </summary>
        /// <param name="request">Pagination and filtering parameters</param>
        /// <returns>Paginated list of users matching the criteria</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUsersRequest request)
        {
            var query = _mapper.Map<GetAllUsersQuery>(request);
            var result = await _mediator.Send(query);
            return Ok(_mapper.Map<PagedResponse<UserResponse>>(result));
        }

        /// <summary>
        /// Find nearby adult users with complete profiles
        /// Business operation: Location-based User Discovery
        /// Demonstrates advanced filtering using Specification pattern
        /// </summary>
        /// <param name="latitude">Center latitude for search</param>
        /// <param name="longitude">Center longitude for search</param>
        /// <param name="radiusInKm">Search radius in kilometers (default: 50)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of nearby adult users with complete profiles</returns>
        [HttpGet("nearby-adults")]
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
            
            var result = await _mediator.Send(query);
            return Ok(_mapper.Map<PagedResponse<UserResponse>>(result));
        }

        /// <summary>
        /// Update user information (full update)
        /// Business operation: Administrative User Update
        /// </summary>
        /// <param name="request">Complete user data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(UpdateUserRequest request)
        {
            var command = _mapper.Map<UpdateUserCommand>(request);
            var result = await _mediator.Send(command);
            return Ok(_mapper.Map<UserResponse>(result));
        }

        /// <summary>
        /// Permanently delete a user (hard delete)
        /// Business operation: Administrative User Deletion
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new HardDeleteUserCommand(id));
            return Ok(_mapper.Map<DeleteUserResponse>(result));
        }

        /// <summary>
        /// Soft delete a user (logical deletion)
        /// Business operation: Administrative User Deactivation
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpPost("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var result = await _mediator.Send(new SoftDeleteUserCommand(id));
            return Ok(_mapper.Map<DeleteUserResponse>(result));
        }
    }
}
