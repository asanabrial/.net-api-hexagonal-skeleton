﻿using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class SoftDeleteUserCommand(int id) : IRequest<IResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; } = id;
    }
}
