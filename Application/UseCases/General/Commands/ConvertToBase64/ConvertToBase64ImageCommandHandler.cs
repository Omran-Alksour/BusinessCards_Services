using Application.Abstractions.Messaging;
using Domain.Errors;
using Domain.Shared;

namespace Application.UseCases.General.Commands.ConvertToBase64
{
    public sealed class ConvertToBase64ImageCommandHandler : ICommandHandler<ConvertToBase64ImageCommand, string>
    {
        public ConvertToBase64ImageCommandHandler()
        {
        }

        public async Task<Result<string>> Handle(ConvertToBase64ImageCommand request, CancellationToken cancellationToken)
        {
            const long maxSizeInBytes = 1 * 1024 * 1024; // 1MB

            if (request.photoFile == null || request.photoFile.Length == 0)
            {
                return Result.Failure<string>(new Error(ApplicationErrors.File.NoFileUploaded.Code, ApplicationErrors.File.NoFileUploaded.Message));
            }
            if (request.photoFile.Length > maxSizeInBytes)
            {
                return Result.Failure<string>(ApplicationErrors.File.FileSizeExceeded);
            }
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Copy the image to memory
                    await request.photoFile.CopyToAsync(memoryStream, cancellationToken);
                    var imageBytes = memoryStream.ToArray();
                    var base64String = Convert.ToBase64String(imageBytes);

                    var fileExtension = Path.GetExtension(request.photoFile.FileName).ToLowerInvariant();
                    string? mimeType = fileExtension switch
                    {
                        ".png" => "image/png",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        _ => null
                    };

                    if (mimeType == null)
                    {
                        return Result.Failure<string>(new Error(ApplicationErrors.Image.UnsupportedImageFormat.Code, ApplicationErrors.Image.UnsupportedImageFormat.Message));
                    }

                    var base64Image = $"data:{mimeType};base64,{base64String}";

                    return Result.Success(base64Image);
                }
            }
            catch (Exception)
            {
                return Result.Failure<string>(new Error(ApplicationErrors.Image.ImageConversionError.Code, ApplicationErrors.Image.ImageConversionError.Message));
            }
        }
    }
}
