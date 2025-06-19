namespace HexagonalSkeleton.API.Dto
{
    /// <summary>
    /// API response for user login
    /// </summary>
    public class LoginApiResponse
    {
        public string? AccessToken { get; set; }
        public UserApiDto? User { get; set; }
    }
}
