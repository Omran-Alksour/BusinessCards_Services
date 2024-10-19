using Application.Abstractions.Messaging;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using Domain.Shared;
using ZXing;
using Domain.Errors;
using Domain.ValueObjects;
using Domain.Enums;

namespace Application.UseCases.BusinessCard.Commands.GenerateQrCode
{ 
    public sealed class GenerateQrCodeCommandHandler : ICommandHandler<GenerateQrCodeCommand, byte[]>
    {
        public async Task<Result<byte[]>> Handle(GenerateQrCodeCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                          !Email.IsValid(request.Email) ||
                           string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                           request.DateOfBirth == default ||
                           string.IsNullOrWhiteSpace(request.Address)||
                            !Enum.IsDefined(typeof(Gender), request.Gender)
                           )
            {
                return Domain.Shared.Result.Failure<byte[]>(ApplicationErrors.QrCode.QrCodeGenerationFailed);
            }

            var businessCardInfo = new Responses.BusinessCard(request.Name, (int)request.Gender, request.Email, request.PhoneNumber, request.DateOfBirth, request.Address, "");

            string businessCardJson = Newtonsoft.Json.JsonConvert.SerializeObject(businessCardInfo);

            var writer = new ZXing.BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(businessCardJson);

            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return stream.ToArray(); 
                }
            }
        }
    }
}
