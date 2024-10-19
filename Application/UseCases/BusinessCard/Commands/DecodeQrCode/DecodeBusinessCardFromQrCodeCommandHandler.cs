using Application.Abstractions.Messaging;
using Domain.Errors;
using Domain.Shared;
using System.Drawing;
using ZXing.Windows.Compatibility;
using Result = Domain.Shared.Result;

namespace Application.UseCases.BusinessCard.Commands.DecodeQrCode;

public sealed class DecodeBusinessCardFromQrCodeCommandHandler : ICommandHandler<DecodeBusinessCardFromQrCodeCommand, Responses.BusinessCard>
{
    public async Task<Result<Responses.BusinessCard>> Handle(DecodeBusinessCardFromQrCodeCommand command, CancellationToken cancellationToken)
    {
        const long MaxFileSizeInBytes = 1 * 1024 * 1024; // 1 MB
        var supportedFileTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };

        if (command.File is null || command.File.Length == 0)
        {
            return Result.Failure<Responses.BusinessCard>(ApplicationErrors.File.EmptyFile);
        }
        if (command.File.Length > MaxFileSizeInBytes)
        {
            return Result.Failure<Responses.BusinessCard>(ApplicationErrors.File.FileSizeExceeded);
        }

        if (!supportedFileTypes.Contains(command.File.ContentType.ToLower()))
        {
            return Result.Failure<Responses.BusinessCard>(ApplicationErrors.File.UnsupportedFileFormat);
        }
        try
        {
            using (var stream = command.File.OpenReadStream())
            using (var bitmap = new Bitmap(stream))
            {
                // Use ZXing 
                var reader = new BarcodeReader();
                var qrResult = reader.Decode(bitmap);

                if (qrResult == null)
                {
                    return Result.Failure<Responses.BusinessCard>(ApplicationErrors.QrCode.InvalidQrCodeImage);
                }

                var businessCard = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses.BusinessCard>(qrResult.Text);

                if (businessCard == null)
                {
                    return Result.Failure<Responses.BusinessCard>(ApplicationErrors.QrCode.InvalidBusinessCardData);
                }

                return Result.Success(businessCard);
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<Responses.BusinessCard>(ApplicationErrors.QrCode.InvalidBusinessCardData);
        }
    }
}


