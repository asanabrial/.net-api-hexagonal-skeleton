using HexagonalSkeleton.API.Features.User.Application.Command;
using HexagonalSkeleton.API.Features.User.Application.Query;
using HexagonalSkeleton.API.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalSkeleton.API.Features.User.Infrastructure
{
    /// <summary>
    /// This class is a module that implements ICarterModule to provide routes for login.
    /// </summary>
    [ApiController]
    [Route(template:"api/[controller]")]
    public class UserController(ISender mediator) : ControllerBase
    {
        [HttpPost, Route(template: "[action]")]
        public async Task<IResult> Login(LoginQuery query) => await mediator.Send(query);

        [HttpPost, Authorize]
        public async Task<IResult> Post(RegisterUserCommand command) => await mediator.Send(command);

        [HttpPut, Authorize]
        public async Task<IResult> Put(UpdateUserCommand command) => await mediator.Send(command);

        [HttpPatch, Authorize]
        public async Task<IResult> Patch(PartialUpdateUserCommand command) => await mediator.Send(command);

        [HttpDelete, Route(template: "hard/{id:int}"), Authorize]
        public async Task<IResult> HardDelete(int id) => await mediator.Send( new HardDeleteUserCommand(id));

        [HttpDelete, Route(template: "soft/{id:int}"), Authorize]
        public async Task<IResult> SoftDelete(int id) => await mediator.Send(new SoftDeleteUserCommand(id));

        [HttpGet, Route(template: "{id:int}"), Authorize]
        public async Task<IResult> Get(int id) => await mediator.Send(new GetUserQuery(id));

        [HttpGet, Route(template: "me"), Authorize]  
        public async Task<IResult> Get() => await mediator.Send(new GetUserQuery(User.GetUserId()));

        [HttpGet, Route(template: "all"), Authorize]
        public async Task<IResult> GetAll() => await mediator.Send(new GetAllUsersQuery());
    }
}
