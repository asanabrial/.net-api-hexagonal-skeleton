using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserProfile.Commands
{
    public class UpdateProfileUserCommand : IRequest<UserProfileDto>
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime Birthdate { get; set; }
        public string AboutMe { get; set; } = string.Empty;

        // Constructor for compatibility (optional)
        public UpdateProfileUserCommand() { }

        // Constructor with parameters for direct instantiation if needed
        public UpdateProfileUserCommand(int id, string aboutMe, string firstName, string lastName, DateTime birthdate)
        {
            Id = id;
            AboutMe = aboutMe;
            FirstName = firstName;
            LastName = lastName;
            Birthdate = birthdate;
        }
    }
}
