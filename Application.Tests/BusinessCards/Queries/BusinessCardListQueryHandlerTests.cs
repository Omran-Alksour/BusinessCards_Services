using Application.UseCases.BusinessCard.Queries.List;
using Domain.Entities.BusinessCard;
using Domain.Enums;
using Domain.Repositories;
using Domain.Shared;
using FluentAssertions;
using Moq;

using BusinessCardResponse= Application.UseCases.BusinessCard.Responses.BusinessCard;

namespace Application.UnitTests.BusinessCards.Queries
{
    public class BusinessCardListQueryHandlerTests
    {
        private readonly Mock<IBusinessCardRepository> _businessCardRepositoryMock;

        public BusinessCardListQueryHandlerTests()
        {
            _businessCardRepositoryMock = new Mock<IBusinessCardRepository>();
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccess_WithDefaultPagination()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var query = new BusinessCardListQuery(false, pageNumber, pageSize, null, "LastUpdateAt", "desc");

            var businessCards = GetSampleBusinessCards(pageSize);
            var pagedResponse = new PagedResponse<BusinessCard>(businessCards, pageNumber, pageSize, businessCards.Count);

            _businessCardRepositoryMock
                .Setup(repo => repo.ListAsync(pageNumber, pageSize, null, "LastUpdateAt", "desc", It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResponse);

            var handler = new BusinessCardListQueryHandler(_businessCardRepositoryMock.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Data.Should().HaveCount(pageSize);
        }

        [Theory]
        [InlineData(2, 5)]
        [InlineData(3, 3)]
        public async Task Handle_Should_ReturnSuccess_WithCustomPagination(int pageNumber, int pageSize)
        {
            // Arrange
            var query = new BusinessCardListQuery(false, pageNumber, pageSize, null, "LastUpdateAt", "desc");
            var businessCards = GetSampleBusinessCards(pageSize);
            var pagedResponse = new PagedResponse<BusinessCard>(businessCards, pageNumber, pageSize, businessCards.Count);

            _businessCardRepositoryMock
                .Setup(repo => repo.ListAsync(pageNumber, pageSize, null, "LastUpdateAt", "desc", It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResponse);

            var handler = new BusinessCardListQueryHandler(_businessCardRepositoryMock.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.PageSize.Should().Be(pageSize);
            result.Value.PageNumber.Should().Be(pageNumber);
        }

        [Theory]
        [InlineData("Omran")]
        [InlineData("Amman")]
        public async Task Handle_Should_ReturnSuccess_WithFilteredSearchResults(string search)
        {
            // Arrange
            var query = new BusinessCardListQuery(false, 1, 10, search, "LastUpdateAt", "desc");
            var businessCards = GetSampleBusinessCards(10).Where(c => c.Name.Contains(search) || c.Address.Contains(search)).ToList();
            var pagedResponse = new PagedResponse<BusinessCard>(businessCards, 1, 10, businessCards.Count);

            _businessCardRepositoryMock
                .Setup(repo => repo.ListAsync(1, 10, search, "LastUpdateAt", "desc", It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResponse);

            var handler = new BusinessCardListQueryHandler(_businessCardRepositoryMock.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Data.Should().OnlyContain(c => c.Name.Contains(search) || c.Address.Contains(search));
        }

        [Theory]
        [InlineData("LastUpdateAt", "desc")]
        [InlineData("Name", "asc")]
        public async Task Handle_Should_ReturnSuccess_WithSorting(string orderBy, string orderDirection)
        {
            // Arrange
            var query = new BusinessCardListQuery(false, 1, 10, null, orderBy, orderDirection);

            var lastUpdateDate1 = DateTime.UtcNow;
            var lastUpdateDate2 = lastUpdateDate1.AddMinutes(-10);

            var businessCards = new List<BusinessCard>
    {
        new BusinessCard(Guid.NewGuid(), "Omran Alksour", 1, "omranalksour@gmail.com", "Amman, Jordan", "+962789079890", lastUpdateDate2, "photo1"),
        new BusinessCard(Guid.NewGuid(), "John Doe", 1, "john@example.com", "New York, USA", "+15551234567", lastUpdateDate1, "photo2")
    };

            var pagedResponse = new PagedResponse<BusinessCard>(businessCards, 1, 10, businessCards.Count);

            _businessCardRepositoryMock
                .Setup(repo => repo.ListAsync(1, 10, null, orderBy, orderDirection, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResponse);

            var handler = new BusinessCardListQueryHandler(_businessCardRepositoryMock.Object);

            // Act
            var result = await handler.Handle(query, default);

            var expectedResponse = new PagedResponse<BusinessCardResponse>(
                businessCards.Select(bc => new BusinessCardResponse(
                    bc.Name,
                    bc.Gender,
                    bc.Email,
                    bc.PhoneNumber,
                    bc.DateOfBirth,
                    bc.Address,
                    "withBase64 parameter is false")) 
                .ToList(),
                1,
                10,
                businessCards.Count
            );

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Data.Should().HaveCount(expectedResponse.Data.Count);

            for (int i = 0; i < result.Value.Data.Count; i++)
            {
                var actual = result.Value.Data[i];
                var expected = expectedResponse.Data[i];

                actual.Name.Should().Be(expected.Name);
                actual.Gender.Should().Be(expected.Gender);
                actual.Email.Should().Be(expected.Email);
                actual.PhoneNumber.Should().Be(expected.PhoneNumber);
                actual.Address.Should().Be(expected.Address);
                actual.Photo.Should().Be(expected.Photo);
            }
        }

        [Theory]
        [InlineData("InvalidField", "asc")]
        [InlineData("LastUpdateAt", "wrongDirection")]
        public async Task Handle_Should_ReturnEmptyList_WithInvalidSortParameters(string orderBy, string orderDirection)
        {
            // Arrange
            var query = new BusinessCardListQuery(false, 1, 10, null, orderBy, orderDirection);
            var emptyResponse = new PagedResponse<BusinessCard>(new List<BusinessCard>(), 1, 10, 0);

            _businessCardRepositoryMock
                .Setup(repo => repo.ListAsync(1, 10, null, orderBy, orderDirection, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyResponse);

            var handler = new BusinessCardListQueryHandler(_businessCardRepositoryMock.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Data.Should().BeEmpty();
            result.Value.TotalRecords.Should().Be(0);
        }


        private List<BusinessCard> GetSampleBusinessCards(int count)
        {
            var businessCards = new List<BusinessCard>();
            for (var i = 0; i < count; i++)
            {
                businessCards.Add(new BusinessCard(Guid.NewGuid(), $"Omran_{i}", 1, $"omran_{i}@mail.com", $"Amman_{i}", "+962789079890", new DateTime(2024, 1, 1), null));
            }
            return businessCards;
        }
    }
}

