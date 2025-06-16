using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{    public class UpdateUserCommand : IRequest<ResultDto>
    {
        public required int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime Birthdate { get; set; }
        public required string PhoneNumber { get; set; }
        public required double Latitude { get; set; }
        public required double Longitude { get; set; }
        public required string AboutMe { get; set; }
    }
}
