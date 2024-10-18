using Application.Abstractions.Messaging;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.UseCases.BusinessCard.Commands.Create
{
    public sealed record BusinessCardCreateCommand(
        string Name,
        Gender Gender,
        DateTime DateOfBirth,
        Email Email,
        string PhoneNumber,
        string Address,
        string Photo,
        CancellationToken CancellationToken = default
    ) : ICommand<Guid>;
}
