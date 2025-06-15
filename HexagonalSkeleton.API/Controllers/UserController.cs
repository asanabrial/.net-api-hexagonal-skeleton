using HexagonalSkeleton.API.Identity;
using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.controllers
{
    [ApiController]
    [Route(template:"api/[controller]")]
    public class UserController(ISender mediator) : ControllerBase
    {
        [HttpPost, Route(template: "[action]")]
        public async Task<IActionResult> Login(LoginQuery query) => Ok(await mediator.Send(query));

        [HttpPost]
        public async Task<IActionResult> Post(RegisterUserCommand command) => Ok(await mediator.Send(command));

        [HttpPut, Authorize]
        public async Task<IActionResult> Put(UpdateUserCommand command) => Ok(await mediator.Send(command));

        [HttpPatch, Authorize]
        public async Task<IActionResult> Patch(UpdateProfileUserCommand command) => Ok(await mediator.Send(command));

        [HttpDelete, Route(template: "hard/{id:int}"), Authorize]
        public async Task<IActionResult> HardDelete(int id) => Ok(await mediator.Send( new HardDeleteUserCommand(id)));

        [HttpDelete, Route(template: "soft/{id:int}"), Authorize]
        public async Task<IActionResult> SoftDelete(int id) => Ok(await mediator.Send(new SoftDeleteUserCommand(id)));

        [HttpGet, Route(template: "{id:int}"), Authorize]
        public async Task<IActionResult> Get(int id) => Ok(await mediator.Send(new GetUserQuery(id)));

        [HttpGet, Route(template: "me"), Authorize]  
        public async Task<IActionResult> Get() => Ok(await mediator.Send(new GetUserQuery(User.GetUserId())));

        [HttpGet, Route(template: "all"), Authorize]
        public async Task<IActionResult> GetAll() => Ok(await mediator.Send(new GetAllUsersQuery()));
    }
}
