using Application.UseCases.BusinessCard.Queries.ExportBusinessCards;
using Domain.Errors;
using Domain.Repositories;
using FluentAssertions;
using Moq;
using System.Text;
using Domain.Entities.BusinessCard;


namespace Application.UnitTests.BusinessCards.Queries
{
    public class ExportBusinessCardsQueryHandlerTests
    {
        private readonly Mock<IBusinessCardRepository> _businessCardRepositoryMock;

        public ExportBusinessCardsQueryHandlerTests()
        {
            _businessCardRepositoryMock = new Mock<IBusinessCardRepository>();
        }

        [Fact]
        public async Task Handle_Should_ReturnCsvFormat_WhenExportingInCsvFormat()
        {
            // Arrange
            var query = new ExportBusinessCardsQuery(null, "csv");
            SetupBusinessCardsForExport();
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Value.Should().NotBeNullOrEmpty();
            Encoding.UTF8.GetString(result).Should().Contain(","); 
        }

        [Fact]
        public async Task Handle_Should_ReturnUtf16Xml_WhenExportingInXmlFormat()
        {
            // Arrange
            var query = new ExportBusinessCardsQuery(null, "xml");
            SetupBusinessCardsForExport(arabic: true);
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Value.Should().NotBeNullOrEmpty();
            var resultString = Encoding.UTF8.GetString(result);
            resultString.Should().Contain("encoding=\"utf-16\"");
            resultString.Should().Contain("عمران الكسور");
        }

        [Theory]
        [MemberData(nameof(GetSpecificIdTestCases))]
        public async Task Handle_Should_ExportOnlySpecificIds(List<Guid> ids)
        {
            // Arrange
            var query = new ExportBusinessCardsQuery(ids, "csv");

            var businessCards = ids.Select(id => new BusinessCard(id, "Name " + id, 1, $"email{id}@example.com", "Address", "Phone", DateTime.UtcNow, "Photo")).ToList();
            _businessCardRepositoryMock
                .Setup(repo => repo.GetByIdsAsync(ids, It.IsAny<CancellationToken>()))
                .ReturnsAsync(businessCards);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Value.Should().NotBeNullOrEmpty();

            var resultString = Encoding.UTF8.GetString(result.Value);

            foreach (var card in businessCards)
            {
                resultString.Should().Contain(card.Name);
                resultString.Should().Contain(card.Email);
            }
        }

        [Fact]
        public async Task Handle_Should_ExportAll_WhenNoIdsAreProvided()
        {
            // Arrange
            var query = new ExportBusinessCardsQuery(null, "csv");
            SetupBusinessCardsForExport();
            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Value.Should().NotBeNullOrEmpty();
            Encoding.UTF8.GetString(result).Should().Contain("Omran Alksour").And.Contain("John Doe");
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenUnsupportedFormatIsSpecified()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var query = new ExportBusinessCardsQuery(ids, "unsupported");

            _businessCardRepositoryMock
                .Setup(repo => repo.GetByIdsAsync(ids, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BusinessCard> // mock data
                {
                    new BusinessCard(ids[0], "Test1", 1, "test1@example.com", "Address1", "+111111111", DateTime.UtcNow, "photo1"),
                    new BusinessCard(ids[1], "Omran", 2, "omran@example.com", "Address2", "+222222222", DateTime.UtcNow, "photo2")
                });

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.File.UnsupportedExportFormat);
        }



        #region Helper Methods

        private ExportBusinessCardsQueryHandler CreateHandler()
        {
            return new ExportBusinessCardsQueryHandler(_businessCardRepositoryMock.Object);
        }

        private void SetupBusinessCardsForExport(bool arabic = false)
        {
            var businessCards = new List<BusinessCard>
        {
            new BusinessCard(Guid.NewGuid(), arabic ? "عمران الكسور" : "Omran Alksour", 1, "omranalksour@gmail.com", "Amman, Jordan", "+962789079890", DateTime.UtcNow, null),
            new BusinessCard(Guid.NewGuid(), "John Doe", 1, "john@example.com", "New York, USA", "+15551234567", DateTime.UtcNow, null)
        };

            _businessCardRepositoryMock
                .Setup(repo => repo.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(businessCards);
        }

        public static IEnumerable<object[]> GetSpecificIdTestCases()
        {
            yield return new object[] { new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
            yield return new object[] { new List<Guid> { Guid.NewGuid() } };
        }

        #endregion
    }

}
