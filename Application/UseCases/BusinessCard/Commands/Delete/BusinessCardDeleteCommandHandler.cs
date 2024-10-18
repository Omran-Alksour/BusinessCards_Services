using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Entities = Domain.Entities.BusinessCard;

namespace Application.UseCases.BusinessCard.Commands.Delete
{
    public sealed class BusinessCardDeleteCommandHandler : ICommandHandler<BusinessCardDeleteCommand, List<Guid>>
    {
        private readonly IBusinessCardRepository _BusinessCardRepository;
        private readonly ICacheService _cacheService;

        public BusinessCardDeleteCommandHandler(IBusinessCardRepository BusinessCardRepository, ICacheService cacheService)
        {
            _BusinessCardRepository = BusinessCardRepository;
            _cacheService = cacheService;
        }

        public async Task<Result<List<Guid>>> Handle(BusinessCardDeleteCommand request, CancellationToken cancellationToken)
        {
            if (request is null || request?.IDs is null)
            {
                return Result.Failure<List<Guid>>(ApplicationErrors.BusinessCards.Commands.AtLeastOneIdRequired);
            }

            foreach (var id in request.IDs)
            {
                await _cacheService.RemoveAsync($"{nameof(Entities.BusinessCard)}_{id}");
            }

            return await _BusinessCardRepository.DeleteAsync(request.IDs, request.ForceDelete, cancellationToken);
        }
    }
}
