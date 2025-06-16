namespace HexagonalSkeleton.Domain.Services
{
    /// <summary>
    /// Domain service for user business logic
    /// </summary>
    public class UserDomainService
    {
        public static User CreateUser(
            string email,
            string passwordSalt,
            string passwordHash,
            string name,
            string surname,
            DateTime birthdate,
            string phoneNumber,
            double latitude,
            double longitude,
            string aboutMe)
        {
            var user = new User(
                email,
                passwordSalt,
                passwordHash,
                name,
                surname,
                birthdate,
                phoneNumber,
                latitude,
                longitude,
                aboutMe);

            // Domain events can be added here
            return user;
        }

        public static void UpdateUserProfile(
            User user,
            string name,
            string surname,
            DateTime birthdate,
            string aboutMe)
        {
            user.Name = name;
            user.Surname = surname;
            user.Birthdate = birthdate;
            user.AboutMe = aboutMe;
        }

        public static void SetLastLogin(User user)
        {
            user.LastLogin = DateTime.UtcNow;
        }

        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@");
        }
    }
}
