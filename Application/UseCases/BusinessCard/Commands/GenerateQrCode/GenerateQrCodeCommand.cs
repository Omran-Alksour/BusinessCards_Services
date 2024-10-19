using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.UseCases.BusinessCard.Commands.GenerateQrCode
{
    public sealed record GenerateQrCodeCommand(
        string Name,
        Gender Gender,
        DateTime DateOfBirth,
        string Email,
        string PhoneNumber,
        string Address
    ) : ICommand<byte[]>;
}
