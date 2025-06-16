namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Port for authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        Task<string> GenerateJwtTokenAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
        string HashPassword(string password, string salt);
        string GenerateSalt();
    }
}
