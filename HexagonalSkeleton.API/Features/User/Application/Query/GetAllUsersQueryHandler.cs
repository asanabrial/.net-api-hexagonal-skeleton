﻿using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.CommonCore.Extension;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    public class GetAllUsersQueryHandler(
        IUnitOfWork unitOfWork)
        : IRequestHandler<GetAllUsersQuery, IResult>
    {
        public async Task<IResult> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await unitOfWork.Users.GetAllUsersAsync(cancellationToken: cancellationToken);
            return !users.HasElements() ? Results.NotFound() : Results.Ok(users.Select(s => new GetAllUsersQueryResult(s)));
        }
    }
}
