using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Application.UseCases.General.Commands.ConvertToBase64
{
    public sealed record ConvertToBase64ImageCommand(
        IFormFile photoFile 
    ) : ICommand<string>; 
}
