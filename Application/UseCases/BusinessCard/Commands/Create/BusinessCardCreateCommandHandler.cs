using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Entities = Domain.Entities.BusinessCard;

namespace Application.UseCases.BusinessCard.Commands.Create;

public sealed class BusinessCardCreateCommandHandler : ICommandHandler<BusinessCardCreateCommand, Guid>
{
    private readonly IBusinessCardRepository _BusinessCardRepository;
    private readonly IBusinessCardFileProcessingService _fileProcessingService;

    private readonly ICacheService _cacheService;

    public BusinessCardCreateCommandHandler(IBusinessCardRepository BusinessCardRepository, IBusinessCardFileProcessingService fileProcessingService, ICacheService cacheService)
    {
        _BusinessCardRepository = BusinessCardRepository;
        _fileProcessingService = fileProcessingService;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid>> Handle(BusinessCardCreateCommand command, CancellationToken cancellationToken)
    {

        Result<Entities.BusinessCard?> BusinessCard = await _BusinessCardRepository.CreateAsync(command.Name,command.Gender,
            command.Email,command.Address,command.PhoneNumber,command.DateOfBirth,command.Photo,cancellationToken);


        if (BusinessCard is null)
        {
            return Result.Failure<Guid>(ApplicationErrors.BusinessCards.Commands.BusinessCardCreationFailed);
        }
        if (string.IsNullOrEmpty(BusinessCard.Value?.Email))
        {
            return Result.Failure<Guid>(ApplicationErrors.BusinessCards.Email.InvalidFormat);

        }
        await  _cacheService.SetAsync($"{nameof(Entities.BusinessCard)}_{command.Email.Value.Trim().ToLower()}", BusinessCard.Value);


        return Result.Success<Guid>(Guid.Parse(BusinessCard?.Value?.Id));
    }

}

