using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using HexagonalSkeleton.Application.Features.UserAuthentication.Commands;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;

namespace HexagonalSkeleton.API.Controllers.Features
{
    /// <summary>
    /// User Authentication Business Feature Controller
    /// Follows Screaming Architecture principles by making authentication intentions clear
    /// Handles user authentication and authenticated registration
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class UserAuthenticationController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public UserAuthenticationController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Authenticate user and initiate login session
        /// Core business operation: User Authentication
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication result with token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var command = _mapper.Map<LoginCommand>(request);
            var result = await _mediator.Send(command);
            return Ok(_mapper.Map<LoginResponse>(result));
        }

        /// <summary>
        /// Register a new user with immediate authentication
        /// Core business operation: User Registration + Authentication
        /// Returns authentication token for immediate use
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Created user information with authentication token</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthenticatedRegistrationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(CreateUserRequest request)
        {
            var command = _mapper.Map<RegisterUserCommand>(request);
            var result = await _mediator.Send(command);
            
            var response = _mapper.Map<AuthenticatedRegistrationResponse>(result);
            
            return Created($"/api/users/{result.User.Id}", response);
        }
    }
}
