namespace HexagonalSkeleton.Domain.Ports
{
    /// <summary>
    /// Interface segregation: Separate interface for user existence checks
    /// Follows ISP by providing only existence-related operations
    /// </summary>
    public interface IUserExistenceChecker
    {
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
