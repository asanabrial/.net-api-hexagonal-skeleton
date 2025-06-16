using HexagonalSkeleton.API.Identity;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Controllers
{
    /// <summary>
    /// User management API controller
    /// Implements Hexagonal Architecture with CQRS pattern using MediatR
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController(ISender mediator) : ControllerBase
    {
        /// <summary>
        /// Authenticate user and initiate login session
        /// </summary>
        /// <param name="command">Login credentials</param>
        /// <returns>Authentication result with token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="command">User registration data</param>
        /// <returns>Created user information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {
            var result = await mediator.Send(command);
            return result.IsValid ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result) : BadRequest(result);
        }

        /// <summary>
        /// Update user information (full update)
        /// </summary>
        /// <param name="command">Complete user data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(UpdateUserCommand command)
        {
            var result = await mediator.Send(command);
            return result.IsValid ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update user profile (partial update)
        /// </summary>
        /// <param name="command">Profile update data</param>
        /// <returns>Update result</returns>
        [HttpPatch("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(UpdateProfileUserCommand command)
        {
            var result = await mediator.Send(command);
            return result.IsValid ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Permanently delete a user (hard delete)
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await mediator.Send(new HardDeleteUserCommand(id));
            return result.IsValid ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Soft delete a user (logical deletion)
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpPost("{id:int}/soft-delete")]
        [Authorize]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await mediator.Send(new SoftDeleteUserCommand(id));
            return result.IsValid ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get user by identifier
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User information</returns>
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await mediator.Send(new GetUserQuery(id));
            return Ok(result);
        }

        /// <summary>
        /// Get current authenticated user's information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMe()
        {
            var result = await mediator.Send(new GetUserQuery(User.GetUserId()));
            return Ok(result);
        }

        /// <summary>
        /// Get all users (admin functionality)
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }
    }
}
