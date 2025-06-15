using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateProfileUserCommand(int id, string aboutMe, string name, string surname, DateTime birthdate) : IRequest<ResultDto>
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
        public User ToDomainEntity()
        {
            var userEntity = new User
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
