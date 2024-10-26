using Application.Abstractions.Caching;
using Application.Abstractions.Services;
using Application.UseCases.BusinessCard.Commands.Create;
using Domain.Entities.BusinessCard;
using Domain.Enums;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.BusinessCards.Commands;

public class BusinessCardCreateCommandHandlerTests
{
    private readonly Mock<IBusinessCardRepository> _businessCardRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBusinessCardFileProcessingService> _fileProcessingServiceMock;

    public BusinessCardCreateCommandHandlerTests()
    {
        _businessCardRepositoryMock = new Mock<IBusinessCardRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _fileProcessingServiceMock = new Mock<IBusinessCardFileProcessingService>();
    }
    [Fact]
    public async Task Handle_Should_ReturnSuccessResult_WhenCommandIsValid()
    {
        // Arrange
        var email = Email.Create("omranalksour@gmail.com").Value;
        var command = new BusinessCardCreateCommand(
            "Omran Alksour",
            Gender.Male,
            new DateTime(2024, 1, 1),
            email,
            "+962789079890",
            "Amman, Jordan",
            null,
            default
        );

        var businessCard = new BusinessCard(
            Guid.NewGuid(),
            command.Name,
            (int)command.Gender,
            command.Email.ToString(),
            command.Address,
            command.PhoneNumber,
            command.DateOfBirth,
            command.Photo
        );

        _businessCardRepositoryMock
            .Setup(repo => repo.CreateAsync(
                It.IsAny<string>(),
                It.IsAny<Gender>(),
                It.IsAny<Email>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Result.Success(businessCard));

        var handler = new BusinessCardCreateCommandHandler(
            _businessCardRepositoryMock.Object,
            _fileProcessingServiceMock.Object,
            _cacheServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Guid.Parse(businessCard.Id));
    }

    [Theory]
    [MemberData(nameof(GetInvalidBusinessCardEmail))]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailIsInvalid(string name, string email, string phoneNumber, Error expectedError)
    {
        // Arrange
        Result<Email> emailResult = email != null ? Email.Create(email) : Result.Failure<Email>(DomainErrors.ValueObject.Email.NullOrWhiteSpace);

        if (emailResult.IsFailure)
        {
            emailResult.Error.Should().Be(expectedError);
            return;
        }

        var command = new BusinessCardCreateCommand(
            name,
            Gender.Male,
            new DateTime(2024, 1, 1),
            emailResult.Value,
            phoneNumber,
            "Amman, Jordan",
            null,
            default
        );

        _businessCardRepositoryMock
            .Setup(repo => repo.CreateAsync(
                It.IsAny<string>(),
                It.IsAny<Gender>(),
                It.IsAny<Email>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Result.Failure<BusinessCard>(expectedError));

        var handler = new BusinessCardCreateCommandHandler(
            _businessCardRepositoryMock.Object,
            _fileProcessingServiceMock.Object,
            _cacheServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    public static IEnumerable<object[]> GetInvalidBusinessCardEmail()
    {
        yield return new object[] { "Omran Alksour", null, "+962789079890", DomainErrors.ValueObject.Email.NullOrWhiteSpace };
        yield return new object[] { "Omran Alksour", "invalidEmail", "+962789079890", DomainErrors.ValueObject.Email.InvalidFormat };
    }



    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task Handle_Should_ReturnFailureResult_WhenGenderIsInvalid(int invalidGender)
    {
        // Arrange
        var command = new BusinessCardCreateCommand(
            "Omran Alksour",
            (Gender)invalidGender,
            new DateTime(2024, 1, 1),
            Email.Create("omranalksour@gmail.com").Value,
            "+962789079890",
            "Amman, Jordan",
            null,
            default
        );

        var handler = new BusinessCardCreateCommandHandler(
            _businessCardRepositoryMock.Object,
            _fileProcessingServiceMock.Object,
            _cacheServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenPhotoExceedsSizeLimit()
    {
        // Arrange
        var largePhoto = new string('a', 1_400_001); // 1.4MB (exceeding 1MB limit)
        var command = new BusinessCardCreateCommand(
            "Omran Alksour",
            Gender.Male,
            new DateTime(2024, 1, 1),
            Email.Create("omranalksour@gmail.com").Value,
            "+962789079890",
            "Amman, Jordan",
            largePhoto,
            default
        );

        var handler = new BusinessCardCreateCommandHandler(
            _businessCardRepositoryMock.Object,
            _fileProcessingServiceMock.Object,
            _cacheServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenEmailIsDuplicate()
    {
        // Arrange
        var email = Email.Create("omranalksour@gmail.com").Value;
        var command = new BusinessCardCreateCommand(
            "Omran Alksour",
            Gender.Male,
            new DateTime(2024, 1, 1),
            email,
            "+962789079890",
            "Amman, Jordan",
            null,
            default
        );

        _businessCardRepositoryMock
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessCard> { new BusinessCard() });

        var handler = new BusinessCardCreateCommandHandler(
            _businessCardRepositoryMock.Object,
            _fileProcessingServiceMock.Object,
            _cacheServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
