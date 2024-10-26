using Application.Abstractions.Caching;
using Application.UseCases.BusinessCard.Commands.Delete;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.UnitTests.BusinessCards.Commands
{
    public class BusinessCardDeleteCommandHandlerTests
    {
        private readonly Mock<IBusinessCardRepository> _businessCardRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;

        public BusinessCardDeleteCommandHandlerTests()
        {
            _businessCardRepositoryMock = new Mock<IBusinessCardRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessResult_WhenIDsAreValid()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var command = new BusinessCardDeleteCommand(ids, false);
            var deletionResult = Result.Success(ids);

            _businessCardRepositoryMock
                .Setup(repo => repo.DeleteAsync(ids, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletionResult);

            var handler = new BusinessCardDeleteCommandHandler(
                _businessCardRepositoryMock.Object,
                _cacheServiceMock.Object
            );

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(ids);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailureResult_WhenIDsDoNotExist()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };
            var command = new BusinessCardDeleteCommand(ids, false);
            var result = Result.Failure<List<Guid>>(ApplicationErrors.BusinessCards.Commands.BusinessCardNotFound);

            _businessCardRepositoryMock
                .Setup(repo => repo.DeleteAsync(ids, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var handler = new BusinessCardDeleteCommandHandler(
                _businessCardRepositoryMock.Object,
                _cacheServiceMock.Object
            );

            // Act
            var handlerResult = await handler.Handle(command, default);

            // Assert
            handlerResult.IsFailure.Should().BeTrue();
            handlerResult.Error.Should().Be(ApplicationErrors.BusinessCards.Commands.BusinessCardNotFound);
        }

        [Fact]
        public async Task Handle_Should_ReturnPartialSuccess_WhenMixedValidInvalidIDs()
        {
            // Arrange
            var validId = Guid.NewGuid();
            var invalidId = Guid.NewGuid();
            var ids = new List<Guid> { validId, invalidId };
            var result = Result.Failure<List<Guid>>(ApplicationErrors.BusinessCards.Commands.BusinessCardNotFound);

            _businessCardRepositoryMock
                .Setup(repo => repo.DeleteAsync(ids, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var command = new BusinessCardDeleteCommand(ids);

            var handler = new BusinessCardDeleteCommandHandler(
                _businessCardRepositoryMock.Object,
                _cacheServiceMock.Object
            );

            // Act
            var handlerResult = await handler.Handle(command, default);

            // Assert
            handlerResult.IsFailure.Should().BeTrue();
            handlerResult.Error.Should().Be(ApplicationErrors.BusinessCards.Commands.BusinessCardNotFound);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_Should_RespectForceDeleteFlag(bool forceDelete)
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };
            var command = new BusinessCardDeleteCommand(ids, forceDelete);
            var result = Result.Success(ids);

            _businessCardRepositoryMock
                .Setup(repo => repo.DeleteAsync(ids, forceDelete, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var handler = new BusinessCardDeleteCommandHandler(
                _businessCardRepositoryMock.Object,
                _cacheServiceMock.Object
            );

            // Act
            var handlerResult = await handler.Handle(command, default);

            // Assert
            handlerResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenIDsAreMissing()
        {
            // Arrange
            var command = new BusinessCardDeleteCommand(null, false);

            var handler = new BusinessCardDeleteCommandHandler(
                _businessCardRepositoryMock.Object,
                _cacheServiceMock.Object
            );

            // Act
            var handlerResult = await handler.Handle(command, default);

            // Assert
            handlerResult.IsFailure.Should().BeTrue();
            handlerResult.Error.Should().Be(ApplicationErrors.BusinessCards.Commands.AtLeastOneIdRequired);
        }

    }
}
