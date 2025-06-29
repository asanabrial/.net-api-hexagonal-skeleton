using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Features.UserRegistration.Commands
{
    public class RegisterUserCommand : IRequest<RegisterDto>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordConfirmation { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime Birthdate { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string AboutMe { get; set; } = string.Empty;

        // Constructor for compatibility (optional)
        public RegisterUserCommand() { }

        // Constructor with parameters for direct instantiation if needed
        public RegisterUserCommand(
            string email,
            string password, 
            string passwordConfirmation, 
            string name,
            string surname,
            DateTime birthdate,
            string phoneNumber,
            double latitude,
            double longitude,
            string aboutMe)
        {
            Email = email;
            Password = password;
            PasswordConfirmation = passwordConfirmation;
            FirstName = name;
            LastName = surname;
            Birthdate = birthdate;
            PhoneNumber = phoneNumber;
            Latitude = latitude;
            Longitude = longitude;
            AboutMe = aboutMe;
        }
    }
}
