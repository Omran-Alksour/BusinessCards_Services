using Application.Abstractions.Messaging;

namespace Application.UseCases.BusinessCard.Commands.Delete
{
    public sealed record BusinessCardDeleteCommand(
         List<Guid> IDs,
        bool ForceDelete = false,
        CancellationToken CancellationToken = default
    ) : ICommand<List<Guid>>;
}
