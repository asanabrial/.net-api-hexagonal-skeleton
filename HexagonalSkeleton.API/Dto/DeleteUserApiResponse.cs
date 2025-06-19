namespace HexagonalSkeleton.API.Dto
{
    /// <summary>
    /// API response for user deletion
    /// Success is implied by not throwing an exception
    /// </summary>
    public class DeleteUserApiResponse
    {
        public string Message { get; set; } = "User deleted successfully";
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    }
}
