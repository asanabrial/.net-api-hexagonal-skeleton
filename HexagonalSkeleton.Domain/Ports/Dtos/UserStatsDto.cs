namespace HexagonalSkeleton.Domain.Ports.Dtos
{
    /// <summary>
    /// DTO for user statistics
    /// </summary>
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double AverageAge { get; set; }
        public double AverageProfileCompleteness { get; set; }
    }
}
