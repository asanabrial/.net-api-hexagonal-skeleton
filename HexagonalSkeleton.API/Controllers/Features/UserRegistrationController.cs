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
            
            var response = _mapper.Map<LoginResponse>(result);
            
            return Created($"/api/users/{result.User.Id}", response);
        }
    }
}
