using AutoMapper;
using HexagonalSkeleton.Application.Dto;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Response model for successful user deletion
    /// </summary>
    [AutoMap(typeof(UserDeletionDto), ReverseMap = true)]
    public class DeleteUserResponse
    {
        /// <summary>
        /// ID of the deleted user
        /// </summary>
        public int UserId { get; set; }
    }
}
