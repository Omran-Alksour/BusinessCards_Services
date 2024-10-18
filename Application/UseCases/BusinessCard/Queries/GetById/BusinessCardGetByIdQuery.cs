using Application.Abstractions.Messaging;
using Application.UseCases.BusinessCard.Responses;

namespace Application.UseCases.BusinessCard.Queries.GetByEmail
{
    public sealed record BusinessCardGetByIdQuery(
        Guid Id
    ) : IQuery<BusinessCardResponse>;
}
