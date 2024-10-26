using Application.UseCases.General.Commands.ConvertToBase64;
using Domain.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace Application.UnitTests.General.Commands
{
    public class ConvertToBase64ImageCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_ReturnSuccess_WhenValidJpegImage()
        {
            // Arrange
            var jpegFile = CreateMockFile("dummy content", "image/jpeg", ".jpg");
            var command = new ConvertToBase64ImageCommand(jpegFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().StartWith("data:image/jpeg;base64,");
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenImageFileExceedsSizeLimit()
        {
            // Arrange
            var largeFile = CreateMockFile("dummy content exceeding size limit", "image/jpeg", ".jpg", sizeInBytes: 2 * 1024 * 1024); // 2MB
            var command = new ConvertToBase64ImageCommand(largeFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.File.FileSizeExceeded);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenUnsupportedFileType()
        {
            // Arrange
            var bmpFile = CreateMockFile("dummy content", "image/bmp", ".bmp");
            var command = new ConvertToBase64ImageCommand(bmpFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.Image.UnsupportedImageFormat);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenFileIsMissing()
        {
            // Arrange
            IFormFile missingFile = null;
            var command = new ConvertToBase64ImageCommand(missingFile);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.File.NoFileUploaded);
        }

        private ConvertToBase64ImageCommandHandler CreateHandler()
        {
            return new ConvertToBase64ImageCommandHandler();
        }

        private IFormFile CreateMockFile(string content, string contentType, string extension, int sizeInBytes = 1024)
        {
            var fileMock = new Mock<IFormFile>();
            var contentBytes = Encoding.UTF8.GetBytes(content.PadRight(sizeInBytes, ' '));
            var stream = new MemoryStream(contentBytes);

            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(contentBytes.Length);
            fileMock.Setup(f => f.FileName).Returns($"mockFile{extension}");

            return fileMock.Object;
        }
    }
}
