namespace HexagonalSkeleton.API.Config
{
    public class AppSettings()
    {
        public required string ContentUserFolder { get; set; }
        public required string ContentImgFolder { get; set; }
        public required string[] AllowedFileExtensions { get; set; }
        public required string Pepper { get; set; }
        public required TokenSecuritySettings Jwt { get; set; }
    }

    public class TokenSecuritySettings()
    {
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string Secret { get; set; }
    }
}
