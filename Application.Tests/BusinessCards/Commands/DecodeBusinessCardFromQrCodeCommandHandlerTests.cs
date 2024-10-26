using Application.UseCases.BusinessCard.Commands.DecodeQrCode;
using Domain.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Drawing;
using System.Text;
using ZXing;
using ZXing.Windows.Compatibility;

namespace Application.UnitTests.BusinessCards.Commands
{
    public class DecodeBusinessCardFromQrCodeCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_ReturnSuccess_WhenValidPngQrCode()
        {
            // Arrange
            var qrCodeData = "{\"Name\":\"Omran Alksour\",\"Email\":\"omranalksour@example.com\"}";
            var pngFile = CreateMockFileWithQrCode(qrCodeData, "image/png");

            var command = new DecodeBusinessCardFromQrCodeCommand(pngFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Omran Alksour");
            result.Value.Email.Should().Be("omranalksour@example.com");
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccess_WhenValidJpegQrCode()
        {
            // Arrange
            var qrCodeData = "{\"Name\":\"Omran Alksour\",\"Email\":\"omranalksour@example.com\"}";
            var jpegFile = CreateMockFileWithQrCode(qrCodeData, "image/jpeg");

            var command = new DecodeBusinessCardFromQrCodeCommand(jpegFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Omran Alksour");
            result.Value.Email.Should().Be("omranalksour@example.com");
        }

        [Fact]
          public async Task Handle_Should_ReturnFailure_WhenUnsupportedFileType()
        {
            // Arrange
            var bmpFile = CreateMockFile("dummy data", "image/bmp");

            var command = new DecodeBusinessCardFromQrCodeCommand(bmpFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.File.UnsupportedFileFormat);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenInvalidQrCodeContent()
        {
            // Arrange
            var invalidQrCodeData = "Invalid QR Code Data";
            var pngFile = CreateMockFileWithQrCode(invalidQrCodeData, "image/png");

            var command = new DecodeBusinessCardFromQrCodeCommand(pngFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.QrCode.InvalidBusinessCardData);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenFileExceedsSizeLimit()
        {
            // Arrange
            var largeFile = CreateMockFile("dummy data exceeding size limit", "image/png", sizeInBytes: 2 * 1024 * 1024); // 2MB

            var command = new DecodeBusinessCardFromQrCodeCommand(largeFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.File.FileSizeExceeded);
        }

        #region Helper Methods

        private IFormFile CreateMockFile(string content, string contentType, int sizeInBytes = 1024)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(contentBytes);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.FileName).Returns("mockFile.txt");
            fileMock.Setup(f => f.Length).Returns(sizeInBytes);

            return fileMock.Object;
        }

        private IFormFile CreateMockFileWithQrCode(string qrCodeData, string contentType)
        {
            var stream = new MemoryStream();
            var bitmap = GenerateQrCodeBitmap(qrCodeData);
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.FileName).Returns("mockFile.png");
            fileMock.Setup(f => f.Length).Returns(stream.Length);

            return fileMock.Object;
        }

        private Bitmap GenerateQrCodeBitmap(string qrCodeData)
        {
            var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE };
            return writer.Write(qrCodeData);
        }

        private DecodeBusinessCardFromQrCodeCommandHandler CreateHandler()
        {
            return new DecodeBusinessCardFromQrCodeCommandHandler();
        }

        #endregion
    }
}
