using Application.Abstractions.Messaging;
using Application.UseCases.BusinessCard.Responses;
using Domain.Repositories;
using Domain.Shared;
using System.Linq;
using Entities = Domain.Entities.BusinessCard ;

namespace Application.UseCases.BusinessCard.Queries.List
{
    public sealed class BusinessCardListQueryHandler : IQueryHandler<BusinessCardListQuery, PagedResponse<BusinessCardResponse>>
    {
        private readonly IBusinessCardRepository _BusinessCardRepository;



        public BusinessCardListQueryHandler(IBusinessCardRepository BusinessCardRepository)
        {
            _BusinessCardRepository = BusinessCardRepository;
        }

        public async Task<Result<PagedResponse<BusinessCardResponse>>> Handle(BusinessCardListQuery request, CancellationToken cancellationToken)
        {
            PagedResponse<Entities.BusinessCard>? BusinessCards = await _BusinessCardRepository.ListAsync(
                                request.PageNumber,
                                request.PageSize,
                                request.Search,
                                request.OrderBy,
                                request.OrderDirection,
                                cancellationToken
                              );


            if (BusinessCards is null || BusinessCards.Data == null || !BusinessCards.Data.Any())
            {
                var emptyPagedResponse = new PagedResponse<BusinessCardResponse>(
                    new List<BusinessCardResponse>(),
                    request.PageNumber,
                    request.PageSize,
                    0 
                );

                return Result.Success(emptyPagedResponse);
            }
           

            var BusinessCardResponses = BusinessCards.Data.Select(businessCard => new BusinessCardResponse(
                    businessCard.Id,
                    businessCard.Name,
                    businessCard.Gender,
                    businessCard.DateOfBirth,
                    businessCard.Email,
                    businessCard.PhoneNumber,
                    businessCard.Address,
                    request.withBase64? businessCard.Photo: "withBase64 parameter is false",
                    businessCard.LastUpdateAt
                )).ToList();

            var pagedResponse = new PagedResponse<BusinessCardResponse>(
                BusinessCardResponses,
                BusinessCards.PageNumber,
                BusinessCards.PageSize,
                BusinessCards.TotalRecords
            );

            return Result.Success(pagedResponse);
        }
    }


}
