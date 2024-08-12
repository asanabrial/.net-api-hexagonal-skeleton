namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UploadProfileImageCommandResult(string? urlProfileImage)
    {
        public string? UrlProfileImage { get; set; } = urlProfileImage;
    }
}
