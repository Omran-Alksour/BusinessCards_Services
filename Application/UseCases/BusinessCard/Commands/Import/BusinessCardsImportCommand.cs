using Application.Abstractions.Messaging;
using Application.Validators;
using Microsoft.AspNetCore.Http;

namespace Application.UseCases.BusinessCard.Commands.Import
{
    public sealed record BusinessCardsImportCommand(
        [CustomFileExtensions(".csv,.xml")] 
        IFormFile File,
        CancellationToken CancellationToken = default
    ) : ICommand<Dictionary<string, List<object>>>;
}
