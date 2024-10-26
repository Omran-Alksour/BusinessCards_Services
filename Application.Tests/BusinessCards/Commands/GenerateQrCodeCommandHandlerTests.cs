using Application.UseCases.BusinessCard.Commands.GenerateQrCode;
using Domain.Errors;
using Domain.Enums;
using FluentAssertions;

namespace Application.UnitTests.BusinessCards.Commands
{
    public class GenerateQrCodeCommandHandlerTests
    {
        private GenerateQrCodeCommandHandler CreateHandler()
        {
            return new GenerateQrCodeCommandHandler();
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccess_WhenValidBusinessCardData()
        {
            // Arrange
            var command = new GenerateQrCodeCommand(
                "Omran Alksour",
                Gender.Male,
                DateTime.Parse("2024-1-1"),
                "omranalksour@example.com",
                "+962789079890",
                "Amman, Jordan"
            );
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Length.Should().BeGreaterThan(0);
        }


        [Theory]
        [InlineData(null, Gender.Male, "2024-1-1", "omranalksour@example.com", "+962789079890", "Amman, Jordan")]
        [InlineData("Omran Alksour", Gender.Male, "2024-1-1", null, "+962789079890", "Amman, Jordan")]
        [InlineData("Omran Alksour", Gender.Male, "2024-1-1", "omranalksour@example.com", null, "Amman, Jordan")]
        public async Task Handle_Should_ReturnFailure_WhenRequiredFieldsAreMissing(string name, Gender gender, string dateOfBirth, string email, string phoneNumber, string address)
        {
            // Arrange
            var date = DateTime.Parse(dateOfBirth);
            var command = new GenerateQrCodeCommand(name, gender, date, email, phoneNumber, address);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.QrCode.QrCodeGenerationFailed);
        }

        [Theory]
        [InlineData("Omran Alksour", Gender.Male, "2024-1-1", "invalid-email-format", "+962789079890", "Amman, Jordan")] // Invalid email
        [InlineData("Omran Alksour", (Gender)999, "2024-1-1", "omranalksour@example.com", "+962789079890", "Amman, Jordan")] // Unsupported gender
        public async Task Handle_Should_ReturnFailure_WhenInvalidBusinessCardData(string name, Gender gender, string dateOfBirth, string email, string phoneNumber, string address)
        {
            // Arrange
            var date = DateTime.Parse(dateOfBirth);
            var command = new GenerateQrCodeCommand(name, gender, date, email, phoneNumber, address);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.QrCode.QrCodeGenerationFailed);
        }
    }
}
