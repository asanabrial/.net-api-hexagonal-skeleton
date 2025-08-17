using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Port for authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        Task<TokenInfo> GenerateJwtTokenAsync(Guid userId, CancellationToken cancellationToken = default);
        TokenInfo GenerateJwtTokenFromUserData(Guid userId, string email, string fullName, string phoneNumber);
        Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
        string HashPassword(string password, string salt);
        string GenerateSalt();
    }
}
