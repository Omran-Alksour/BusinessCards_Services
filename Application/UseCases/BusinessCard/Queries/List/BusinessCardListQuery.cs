using Application.Abstractions.Messaging;
using Application.UseCases.BusinessCard.Responses;
using Domain.Shared;

namespace Application.UseCases.BusinessCard.Queries.List
{
    public sealed record BusinessCardListQuery(
        bool withBase64=false,
        int PageNumber = 1,
        int PageSize = 15,
        string? Search = null,
        string? OrderBy = null,
        string? OrderDirection = "asc",
        CancellationToken CancellationToken = default
    ) : IQuery<PagedResponse<BusinessCardResponse>>;

}
