using Domain.Entities.BusinessCard;
using Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Services;

public interface IBusinessCardFileProcessingService
{
 //befor testing   Task<List<BusinessCard>> ParseFileAsync(IFormFile file, CancellationToken cancellationToken);
    Task<Result<List<BusinessCard>>> ParseFileAsync(IFormFile file, CancellationToken cancellationToken);
}

