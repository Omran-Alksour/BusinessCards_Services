using Application.Abstractions.Caching;
using Application.UseCases.BusinessCard.Queries.GetByEmail;
using Domain.Entities.BusinessCard;
using Domain.Errors;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.BusinessCards.Queries
{
    public class BusinessCardGetByIdQueryHandlerTests
    {
        private readonly Mock<IBusinessCardRepository> _businessCardRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;

        public BusinessCardGetByIdQueryHandlerTests()
        {
            _businessCardRepositoryMock = new Mock<IBusinessCardRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
        }

        [Fact]
        public async Task Handle_Should_ReturnFailureResult_WhenIdDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

                 _businessCardRepositoryMock
                .Setup(repo => repo.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessCard?)null);

            var handler = new BusinessCardGetByIdQueryHandler(_businessCardRepositoryMock.Object, _cacheServiceMock.Object);

            // Act
            var result = await handler.Handle(new BusinessCardGetByIdQuery(nonExistentId), default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ApplicationErrors.BusinessCards.Queries.BusinessCardNotFound);
        }
    }
}
