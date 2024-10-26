using Application.Abstractions.Services;
using Application.UseCases.BusinessCard.Commands.Import;
using Domain.Entities.BusinessCard;
using Domain.Enums;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using Xunit;
namespace Application.UnitTests.BusinessCards.Commands;

public class BusinessCardsImportCommandHandlerTests
{
    private readonly Mock<IBusinessCardRepository> _businessCardRepositoryMock;
    private readonly Mock<IBusinessCardFileProcessingService> _fileProcessingServiceMock;

    public BusinessCardsImportCommandHandlerTests()
    {
        _businessCardRepositoryMock = new Mock<IBusinessCardRepository>();
        _fileProcessingServiceMock = new Mock<IBusinessCardFileProcessingService>();
    }

    private BusinessCardsImportCommandHandler CreateHandler()
    {
        return new BusinessCardsImportCommandHandler(
            _businessCardRepositoryMock.Object,
            _fileProcessingServiceMock.Object
        );
    }


    [Fact]
    public async Task ImportFile_Should_ReturnSuccess_WhenImportingValidCommaSeparatedCsv()
    {
        var csvData = "Name,Gender,Email,Phone,DateOfBirth,Address\nOmran Alksour,1,omranalksour@gmail.com,+962789079890,2024-1-1,\"Amman, Jordan\"";
        var file = CreateMockFile(csvData, "application/csv", "contacts.csv");

        await ExecuteImportFileTest(file, csvData, ParseCsvToBusinessCards(csvData));
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenImportingValidSemicolonSeparatedCsv()
    {
        var csvData = "Name;Gender;Email;Phone;DateOfBirth;Address\nOmran Alksour;1;omranalksour@gmail.com;+962789079890;2024-1-1;Amman, Jordan";
        var file = CreateMockFile(csvData, "application/csv", "contacts.csv");

        await ExecuteImportFileTest(file, csvData, ParseCsvToBusinessCards(csvData));
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenImportingValidXmlWithUtf16()
    {
        var xmlData = "<BusinessCards><BusinessCard><Name>عمران الكسور</Name><Gender>1</Gender><Email>omranalksour@gmail.com</Email><Phone>+962789079890</Phone><DateOfBirth>2024-1-1</DateOfBirth><Address>عمان, الأردن</Address></BusinessCard></BusinessCards>";
        var file = CreateMockFile(xmlData, "application/xml", "contacts.xml");

        await ExecuteImportFileTest(file, xmlData, ParseXmlToBusinessCards(xmlData));
    }

    private async Task ExecuteImportFileTest(IFormFile file, string fileData, List<BusinessCard> expectedParsedCards)
    {
        // Setup common parsing mock
        _fileProcessingServiceMock
            .Setup(fps => fps.ParseFileAsync(file, default))
            .ReturnsAsync(Result.Success(expectedParsedCards));

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
            .ReturnsAsync((string name, Gender gender, Email email, string address, string phoneNumber, DateTime dateOfBirth, string photo, CancellationToken _) =>
                Result.Success(new BusinessCard(Guid.NewGuid(), name, (int)gender, email.Value, address, phoneNumber, dateOfBirth, photo))
            );

        var command = new BusinessCardsImportCommand(file);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value["success"].Count.Should().Be(expectedParsedCards.Count);
        result.Value["failed"].Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUnsupportedFileExtension()
    {
        // Arrange
        var file = CreateMockFile("Invalid content", "text/plain", "contacts.txt");

        var command = new BusinessCardsImportCommand(file);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.File.UnsupportedFileFormat);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenMissingFileInRequest()
    {
        // Arrange
        var command = new BusinessCardsImportCommand(null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.File.EmptyFile);
    }

    [Theory]
    [InlineData("Name,Gender\nOmran,Alksour", "application/csv", "contacts.csv")] // Incomplete CSV data
    [InlineData("<InvalidXml></BusinessCard>", "application/xml", "contacts.xml")] // Malformed XML
    public async Task Handle_Should_ReturnFailure_WhenInvalidCsvOrXmlFormat(string invalidData, string contentType, string fileName)
    {
        // Arrange
        var file = CreateMockFile(invalidData, contentType, fileName);

        _fileProcessingServiceMock
            .Setup(fps => fps.ParseFileAsync(It.Is<IFormFile>(f => f == file), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<BusinessCard>>(ApplicationErrors.File.InvalidFormat));

        var command = new BusinessCardsImportCommand(file);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.File.UnsupportedFileFormat);
    }


    private IFormFile CreateMockFile(string content, string contentType, string fileName)
    {
        var fileMock = new Mock<IFormFile>();
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);

        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(contentBytes.Length);

        return fileMock.Object;
    }


    private List<BusinessCard> ParseCsvToBusinessCards(string csvData)
    {
        return new List<BusinessCard>
        {
            new BusinessCard(Guid.NewGuid(), "Omran Alksour", 1, "omranalksour@gmail.com", "Amman, Jordan", "+962789079890", DateTime.UtcNow, null)
        };
    }

    private List<BusinessCard> ParseXmlToBusinessCards(string xmlData)
    {
        return new List<BusinessCard>
        {
            new BusinessCard(Guid.NewGuid(), "عمران الكسور", 1, "omranalksour@gmail.com", "عمان, الأردن", "+962789079890", DateTime.UtcNow, null)
        };
    }
}
