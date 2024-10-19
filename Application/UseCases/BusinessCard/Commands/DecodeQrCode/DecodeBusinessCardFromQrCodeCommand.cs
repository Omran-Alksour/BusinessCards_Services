using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Application.UseCases.BusinessCard.Commands.DecodeQrCode
{
    public sealed record DecodeBusinessCardFromQrCodeCommand(
        IFormFile File,
        CancellationToken CancellationToken = default
    ) : ICommand<Responses.BusinessCard>;
}
