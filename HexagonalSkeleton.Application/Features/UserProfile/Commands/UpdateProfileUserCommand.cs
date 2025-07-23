using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserProfile.Commands
{
    public class UpdateProfileUserCommand : IRequest<UserProfileDto>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime Birthdate { get; set; }
        public string AboutMe { get; set; } = string.Empty;

        // Constructor for compatibility (optional)
        public UpdateProfileUserCommand() { }

        // Constructor with parameters for direct instantiation if needed
        public UpdateProfileUserCommand(Guid id, string aboutMe, string firstName, string lastName, string phoneNumber, DateTime birthdate)
        {
            Id = id;
            AboutMe = aboutMe;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Birthdate = birthdate;
        }
    }
}
