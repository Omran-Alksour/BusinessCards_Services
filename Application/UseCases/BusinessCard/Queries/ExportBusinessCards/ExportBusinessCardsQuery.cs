using Application.Abstractions.Messaging;
using Domain.Shared;

namespace Application.UseCases.BusinessCard.Queries.ExportBusinessCards
{
    public sealed record ExportBusinessCardsQuery(
        List<Guid>? BusinessCardIds,
        string Format // "csv" or "xml"
    ) : IQuery<byte[]>;
}



