using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Application.UseCases.BusinessCard.Responses;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Entities = Domain.Entities.BusinessCard;


namespace Application.UseCases.BusinessCard.Queries.GetByEmail
{
    public sealed class BusinessCardGetByIdQueryHandler : IQueryHandler<BusinessCardGetByIdQuery, BusinessCardResponse>
    {
        private readonly IBusinessCardRepository _BusinessCardRepository;
        private readonly ICacheService _cacheService;

        public BusinessCardGetByIdQueryHandler(IBusinessCardRepository BusinessCardRepository, ICacheService cacheService)
        {
            _BusinessCardRepository = BusinessCardRepository;
            _cacheService = cacheService;

        }

        public async Task<Result<BusinessCardResponse>> Handle(BusinessCardGetByIdQuery request, CancellationToken cancellationToken)
        {

            Entities.BusinessCard? businessCard = await _cacheService.GetAsync<Entities.BusinessCard>($"{nameof(Entities.BusinessCard)}_{request.Id}",
                            async () =>
                            {
                                var value = await _BusinessCardRepository.GetByIdAsync(request.Id, cancellationToken);
                                return value;
                            }
                            ,
                          cancellationToken
                            );

            if (businessCard is null)
            {
                return Result.Failure<BusinessCardResponse>(ApplicationErrors.BusinessCards.Queries.BusinessCardNotFound);
            }

            BusinessCardResponse BusinessCardDetails = new(
                businessCard.Id,
                businessCard.Name,
                businessCard.Gender,
                businessCard.DateOfBirth,
                businessCard.Email,
                businessCard.PhoneNumber,
                businessCard.Address,
                businessCard.Photo,
                businessCard.LastUpdateAt
            );


            return Result.Success(BusinessCardDetails);
        }
    }
}
