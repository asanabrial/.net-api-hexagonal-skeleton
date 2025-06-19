namespace HexagonalSkeleton.API.Dto
{
    /// <summary>
    /// API response for user registration
    /// </summary>
    public class RegisterUserApiResponse
    {
        public string? AccessToken { get; set; }
        public UserApiDto? User { get; set; }
    }
}
