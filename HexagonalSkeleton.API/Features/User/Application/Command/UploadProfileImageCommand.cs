﻿using HexagonalSkeleton.API.Features.User.Domain;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UploadProfileImageCommand(
        IFormFile profileImage,
        int userId) : IRequest<IResult>
    {
        public IFormFile ProfileImage { get; set; } = profileImage;

        public int UserId { get; set; } = userId;

        /// <summary>
        /// Method to convert the DTO to a domain entity
        /// </summary>
        /// <returns></returns>
        public UserEntity ToDomainEntity()
        {
            return new UserEntity(
                userId: UserId,
                profileImage: ProfileImage
            );
        }
    }
}
