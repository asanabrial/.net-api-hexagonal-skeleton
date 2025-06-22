namespace HexagonalSkeleton.Domain.ValueObjects
{
    /// <summary>
    /// Value object that contains JWT token information including expiration details
    /// </summary>
    public class TokenInfo
    {
        public TokenInfo(string token, DateTime expiresAt)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            ExpiresAt = expiresAt;
            ExpiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;
        }

        /// <summary>
        /// JWT token string
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// When the token expires (UTC)
        /// </summary>
        public DateTime ExpiresAt { get; }

        /// <summary>
        /// Token expiration time in seconds from now
        /// </summary>
        public int ExpiresIn { get; }
    }
}
