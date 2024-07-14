using HexagonalSkeleton.API.Features.User.Domain;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class represents a command for logging in a user.
    /// </summary>
    public class PartialUpdateUserCommand(int id, string email, string name, string surname, DateTime birthdate) : IRequest<IResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int Id { get; set; } = id;

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; } = email;

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


        /// <summary>
        /// Method to convert the DTO to a domain entity
        /// </summary>
        /// <returns></returns>
        public UserEntity ToDomainEntity(UserEntity? userEntity = null)
        {
            userEntity ??= new UserEntity();

            userEntity.Email = Email;
            userEntity.Name = Name;
            userEntity.Surname = Surname;
            userEntity.Birthdate = Birthdate;

            return userEntity;
        }
    }
}
