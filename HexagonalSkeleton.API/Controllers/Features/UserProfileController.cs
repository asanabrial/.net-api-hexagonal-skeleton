using HexagonalSkeleton.API.Identity;
using HexagonalSkeleton.API.Models.Users;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.API.Controllers.Features
{
    /// <summary>
    /// User Profile Management Business Feature Controller
    /// Follows Screaming Architecture principles by focusing on profile management
    /// Handles personal profile operations for authenticated users
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    [Produces("application/json")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public UserProfileController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Get current authenticated user's profile information
        /// Core business operation: Profile Retrieval
        /// </summary>
        /// <returns>Current user profile information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _mediator.Send(new GetUserQuery(User.GetUserId()));
            return Ok(_mapper.Map<UserResponse>(result));
        }

        /// <summary>
        /// Update user personal information
        /// Business operation: Personal Info Update  
        /// </summary>
        /// <param name="request">Personal information update</param>
        /// <returns>Update confirmation</returns>
        [HttpPatch("personal-info")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePersonalInfo(UpdateProfileRequest request)
        {
            var command = _mapper.Map<UpdateProfileUserCommand>(request);
            command.Id = User.GetUserId();
            var result = await _mediator.Send(command);
            return Ok(_mapper.Map<UserResponse>(result));
        }
    }
}
