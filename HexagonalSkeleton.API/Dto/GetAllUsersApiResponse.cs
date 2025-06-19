namespace HexagonalSkeleton.API.Dto
{
    /// <summary>
    /// API response for getting all users
    /// </summary>
    public class GetAllUsersApiResponse
    {
        public IList<UserApiDto> Users { get; set; } = new List<UserApiDto>();
    }
}
