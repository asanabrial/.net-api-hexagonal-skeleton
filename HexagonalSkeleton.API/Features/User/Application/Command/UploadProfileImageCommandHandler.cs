using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Data;
using MediatR;
using Microsoft.Extensions.Options;
using HexagonalSkeleton.CommonCore.Extension;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UploadProfileImageCommandHandler(
        IWebHostEnvironment env,
        IHttpContextAccessor contextAccessor,
        IValidator<UploadProfileImageCommand> validator,
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> appSettings)
        : IRequestHandler<UploadProfileImageCommand, IResult>
    {
        public async Task<IResult> Handle(UploadProfileImageCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            // Check the allowed extensions
            var ext = Path.GetExtension(request.ProfileImage.FileName);
            if (!appSettings.Value.AllowedFileExtensions.Contains(ext))
                throw new ArgumentException($"Only {string.Join(",", appSettings.Value.AllowedFileExtensions)} are allowed.");

            var entity = await unitOfWork.Users.GetTrackedUserByIdAsync(id: request.UserId, cancellationToken: cancellationToken);
            var userFolder = appSettings.Value.ContentUserFolder;
            var userId = entity!.Id.ToString();
            var imgFolder = appSettings.Value.ContentImgFolder;
            var path = Path.Combine(env.WebRootPath, userFolder, userId, imgFolder);

            if (entity.ProfileImageName is not null)
            {
                var fileToDelete = Path.Combine(path, entity.ProfileImageName);
                if (File.Exists(fileToDelete))
                    File.Delete(fileToDelete);
            }
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // generate a unique filename
            entity.ProfileImageName = $"{Guid.NewGuid().ToString()}{ext}";
            var fileNameWithPath = Path.Combine(path, entity.ProfileImageName);
            await using var stream = new FileStream(fileNameWithPath, FileMode.Create);
            await request.ProfileImage.CopyToAsync(stream, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(new UploadProfileImageCommandResult($"{contextAccessor.GetApiBaseUrl()}/{userFolder}/{userId}/{imgFolder}/{entity.ProfileImageName}"));
        }
    }
}
