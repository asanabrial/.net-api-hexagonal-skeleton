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
        /// Refresh an existing authentication token
        /// Business operation: Token Renewal
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>New authentication tokens</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            // Implementation would depend on your refresh token strategy
            // This is a placeholder for the business intention
            throw new NotImplementedException("Refresh token functionality not implemented yet");
        }

        /// <summary>
        /// Logout user and invalidate session
        /// Business operation: User Logout
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // Implementation would depend on your logout strategy
            // This is a placeholder for the business intention
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
