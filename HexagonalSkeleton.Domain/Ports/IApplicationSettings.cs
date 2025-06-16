namespace HexagonalSkeleton.Domain.Ports
{
    public interface IApplicationSettings
    {
        string Secret { get; }
        string Issuer { get; }
        string Audience { get; }
        string Pepper { get; }
    }
}
