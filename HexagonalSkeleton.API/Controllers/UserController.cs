using HexagonalSkeleton.API.Identity;
using HexagonalSkeleton.API.Dto;
using HexagonalSkeleton.API.Extensions;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Controllers
{    /// <summary>
    /// User management API controller
    /// Implements Hexagonal Architecture with CQRS pattern using MediatR
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController(ISender mediator, IMapper mapper) : ControllerBase
    {
        /// <summary>
        /// Authenticate user and initiate login session
        /// </summary>
        /// <param name="command">Login credentials</param>
        /// <returns>Authentication result with token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginCommand command)        {
            var result = await mediator.Send(command);
            return this.ToApiResult<LoginCommandResult, LoginApiResponse>(result, mapper);
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="command">User registration data</param>
        /// <returns>Created user information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {
            var result = await mediator.Send(command);
            return this.ToApiResult<RegisterUserCommandResult, RegisterUserApiResponse>(result, mapper);
        }

        /// <summary>
        /// Update user information (full update)
        /// </summary>
        /// <param name="command">Complete user data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(UpdateUserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(UpdateUserCommand command)        {
            var result = await mediator.Send(command);
            return this.ToApiResultWithNotFound<UpdateUserCommandResult, UpdateUserApiResponse>(result, mapper);
        }

        /// <summary>
        /// Update user profile (partial update)
        /// </summary>
        /// <param name="command">Profile update data</param>
        /// <returns>Update result</returns>
        [HttpPatch("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateProfileApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(UpdateProfileUserCommand command)
        {
            var result = await mediator.Send(command);
            return this.ToApiResultWithNotFound<UpdateProfileUserCommandResult, UpdateProfileApiResponse>(result, mapper);
        }

        /// <summary>
        /// Permanently delete a user (hard delete)
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(DeleteUserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await mediator.Send(new HardDeleteUserCommand(id));
            return this.ToApiResultWithNotFound<HardDeleteUserCommandResult, DeleteUserApiResponse>(result, mapper);
        }        /// <summary>
        /// Soft delete a user (logical deletion)
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Deletion result</returns>
        [HttpPost("{id:int}/soft-delete")]
        [Authorize]
        [ProducesResponseType(typeof(DeleteUserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(int id)        {
            var result = await mediator.Send(new SoftDeleteUserCommand(id));
            return this.ToApiResultWithNotFound<SoftDeleteUserCommandResult, DeleteUserApiResponse>(result, mapper);
        }

        /// <summary>
        /// Get user by identifier
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User information</returns>
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(GetUserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await mediator.Send(new GetUserQuery(id));
            return this.ToApiResultWithNotFound<GetUserQueryResult, GetUserApiResponse>(result, mapper);
        }        /// <summary>
        /// Get current authenticated user's information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(GetUserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMe()
        {
            var result = await mediator.Send(new GetUserQuery(User.GetUserId()));
            return this.ToApiResultWithNotFound<GetUserQueryResult, GetUserApiResponse>(result, mapper);
        }        /// <summary>
        /// Get all users (admin functionality)
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(GetAllUsersApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await mediator.Send(new GetAllUsersQuery());
            return this.ToApiResult<GetAllUsersQueryResult, GetAllUsersApiResponse>(result, mapper);
        }
    }
}
