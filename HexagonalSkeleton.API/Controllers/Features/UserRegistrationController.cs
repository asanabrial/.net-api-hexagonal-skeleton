using HexagonalSkeleton.API.Models;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.Application.Services.Features;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;

namespace HexagonalSkeleton.API.Controllers.Features
{
    /// <summary>
    /// User Registration Business Feature Controller
    /// Follows Screaming Architecture principles by making business intention clear
    /// Handles user registration without immediate authentication
    /// </summary>
    [ApiController]
    [Route("api/registration")]
    [Produces("application/json")]
    public class UserRegistrationController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public UserRegistrationController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Register a new user with immediate authentication (default endpoint)
        /// Core business operation: User Registration + Authentication
        /// Returns authentication token for immediate use
        /// This is the default POST behavior for /api/user route for backward compatibility
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Created user information with authentication token</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterUser(CreateUserRequest request)
        {
            var command = _mapper.Map<RegisterUserCommand>(request);
            var result = await _mediator.Send(command);
            
            // Map from RegisterDto to LoginResponse for backward compatibility with tests
            var response = _mapper.Map<LoginResponse>(result);
            
            return Created($"/api/users/{result.User.Id}", response);
        }

        /// <summary>
        /// Check if a user with the given email already exists
        /// Business operation: User Uniqueness Validation
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-email-availability")]
        [ProducesResponseType(typeof(UserAvailabilityResponse), StatusCodes.Status200OK)]
        public IActionResult CheckEmailAvailability([FromQuery] string email)
        {
            // Implementation would use a specific query for email checking
            // This is a placeholder for the business intention
            return Ok(new UserAvailabilityResponse 
            { 
                IsAvailable = true, 
                PropertyName = "email", 
                Value = email 
            });
        }

        /// <summary>
        /// Check if a username is available
        /// Business operation: Username Uniqueness Validation
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-username-availability")]
        [ProducesResponseType(typeof(UserAvailabilityResponse), StatusCodes.Status200OK)]
        public IActionResult CheckUsernameAvailability([FromQuery] string username)
        {
            // Implementation would use a specific query for username checking
            // This is a placeholder for the business intention
            return Ok(new UserAvailabilityResponse 
            { 
                IsAvailable = true, 
                PropertyName = "username", 
                Value = username 
            });
        }
    }
}
