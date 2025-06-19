using HexagonalSkeleton.Application.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateProfileUserCommand(int id, string aboutMe, string firstName, string lastName, DateTime birthdate) : IRequest<UpdateProfileUserCommandResult>
    {
        public int Id { get; set; } = id;
        public string FirstName { get; set; } = firstName;
        public string LastName { get; set; } = lastName;
        public DateTime Birthdate { get; set; } = birthdate;
        public string AboutMe { get; set; } = aboutMe;
    }
}
