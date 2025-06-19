using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class RegisterUserCommand(
        string email,
        string password, 
        string passwordConfirmation, 
        string name,
        string surname,
        DateTime birthdate,
        string phoneNumber,
        double latitude,
        double longitude,
        string aboutMe) : IRequest<RegisterUserCommandResult>
    {
        public string Email { get; set; } = email;
        public string Password { get; set; } = password;
        public string PasswordConfirmation { get; set; } = passwordConfirmation;
        public string Name { get; set; } = name;
        public string Surname { get; set; } = surname;
        public DateTime Birthdate { get; set; } = birthdate;
        public string PhoneNumber { get; set; } = phoneNumber;
        public double Latitude { get; set; } = latitude;
        public double Longitude { get; set; } = longitude;        public string AboutMe { get; set; } = aboutMe;
    }
}
