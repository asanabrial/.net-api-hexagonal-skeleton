using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Domain.Ports
{    /// <summary>
    /// Port for user read operations (Query side)
    /// </summary>
    public interface IUserReadRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    }
}
