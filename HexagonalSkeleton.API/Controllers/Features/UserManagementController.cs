using HexagonalSkeleton.API.Identity;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;

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
        /// Update user profile information
        /// Business operation: User Profile Update
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="request">Profile update data</param>
        /// <returns>Update result</returns>
        [HttpPut("{id:guid}/profile")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(Guid id, UpdateProfileRequest request)
        {
            var command = _mapper.Map<UpdateProfileUserCommand>(request);
            command.Id = id;
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new HardDeleteUserCommand(id));
            return NoContent();
        }

        /// <summary>
        /// Soft delete a user (logical deletion)
        /// Business operation: Administrative User Deactivation
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var result = await _mediator.Send(new SoftDeleteUserCommand(id));
            return Ok(_mapper.Map<DeleteUserResponse>(result));
        }
    }
}
