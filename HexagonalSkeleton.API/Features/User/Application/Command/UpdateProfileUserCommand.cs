using HexagonalSkeleton.API.Features.User.Domain;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UpdateProfileUserCommand(int id, string aboutMe, string name, string surname, DateTime birthdate) : IRequest<IResult>
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Surname { get; set; } = surname;
        public DateTime Birthdate { get; set; } = birthdate;
        public string AboutMe { get; set; } = aboutMe;


        /// <summary>
        /// Method to convert the DTO to a domain entity
        /// </summary>
        /// <returns></returns>
        public UserEntity ToDomainEntity()
        {
            var userEntity = new UserEntity
            {
                Name = Name,
                Surname = Surname,
                Birthdate = Birthdate,
                AboutMe = AboutMe
            };

            return userEntity;
        }
    }
}
