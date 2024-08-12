using HexagonalSkeleton.API.Features.User.Domain;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class represents a command for logging in a user.
    /// </summary>
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
        string aboutMe) : IRequest<IResult>
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; } = email;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; } = password;

        /// <summary>
        /// Gets or sets the confirmation password.
        /// </summary>
        public string PasswordConfirmation { get; set; } = passwordConfirmation;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// Gets or sets the surname.
        /// </summary>
        public string Surname { get; set; } = surname;

        /// <summary>
        /// Gets or sets the birthdate.
        /// </summary>
        public DateTime Birthdate { get; set; } = birthdate;

        public string PhoneNumber { get; set; } = phoneNumber;

        public double Latitude { get; set; } = latitude;
        public double Longitude { get; set; } = longitude;

        public string AboutMe { get; set; } = aboutMe;

        /// <summary>
        /// Method to convert the DTO to a domain entity
        /// </summary>
        /// <returns></returns>
        public UserEntity ToDomainEntity()
        {
            return new UserEntity(
                email: Email,
                passwordSalt: Password,
                passwordHash: PasswordConfirmation,
                name: Name,
                surname: Surname,
                birthdate: Birthdate,
                phoneNumber: PhoneNumber,
                latitude : Latitude,
                longitude: Longitude,
                aboutMe: AboutMe
            );
        }
    }
}
