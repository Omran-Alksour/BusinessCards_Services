using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Enums;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.UseCases.BusinessCard.Commands.Import;

public sealed class BusinessCardsImportCommandHandler : ICommandHandler<BusinessCardsImportCommand, Dictionary<string, List<object>>>
{
    private readonly IBusinessCardRepository _businessCardRepository;
    private readonly IBusinessCardFileProcessingService _fileProcessingService;

    public BusinessCardsImportCommandHandler(IBusinessCardRepository businessCardRepository, IBusinessCardFileProcessingService fileProcessingService)
    {
        _businessCardRepository = businessCardRepository;
        _fileProcessingService = fileProcessingService;
    }

    public async Task<Result<Dictionary<string, List<object>>>> Handle(BusinessCardsImportCommand command, CancellationToken cancellationToken)
    {
        if (command.File is null || command.File.Length == 0)
        {
            return Result.Failure<Dictionary<string, List<object>>>(ApplicationErrors.File.EmptyFile);
        }

        var businessCards = await _fileProcessingService.ParseFileAsync(command.File, cancellationToken);
        if (businessCards is null || businessCards.IsFailure)
        {
            return Result.Failure<Dictionary<string, List<object>>>(ApplicationErrors.File.UnsupportedFileFormat);
        }
        var result = new Dictionary<string, List<object>>()
        {
            { "success", new List<object>() },
            { "failed", new List<object>() }
        };

        foreach (var card in businessCards.Value)
        {
            var businessCardResult = await _businessCardRepository.CreateAsync(
                card.Name,
                (Gender)card.Gender,
                Email.Create(card.Email),
                card.Address,
                card.PhoneNumber,
                card.DateOfBirth,
                card.Photo, 
                cancellationToken);

            if (businessCardResult.IsSuccess)
            {
                result["success"].Add(businessCardResult.Value.Id);
            }
            else
            {
                result["failed"].Add(new
                {
                    card.Name,
                    card.Email,
                    card.Address,
                    card.PhoneNumber,
                    card.Gender,
                    card.DateOfBirth,
                    Error = businessCardResult.Error.Message
                });
            }
        }

        return Result.Success(result);
    }
}
