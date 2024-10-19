using Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Domain.Enums;
using Domain.Errors;
using BusinessCardEntity = Domain.Entities.BusinessCard.BusinessCard;
using Domain.Shared;

namespace Application.UseCases.BusinessCard.Services;

public class BusinessCardFileProcessingService : IBusinessCardFileProcessingService
{
    public async Task<Result<List<BusinessCardEntity>>> ParseFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var fileExtension = Path.GetExtension(file.FileName).ToUpper();

        switch (fileExtension)
        {
            case ".XML":
                return await ParseXmlFileAsync(file);

            case ".CSV":
                return await ParseCsvFileAsync(file);

            default:
                return Result.Failure<List<BusinessCardEntity>>(ApplicationErrors.File.UnsupportedFileFormat);
        }
    }


    private static readonly List<string> RequiredFields = new List<string>
{
    "Name", "Gender", "Email", "PhoneNumber", "DateOfBirth", "Address"
};

    private async Task<Result<List<BusinessCardEntity>>> ParseXmlFileAsync(IFormFile file)
    {
        var businessCards = new List<BusinessCardEntity>();

        using (var stream = file.OpenReadStream())
        {
            try
            {
                using (var streamReader = new StreamReader(stream, true)) // Support UTF-16
                {
                    var serializer = new XmlSerializer(typeof(List<BusinessCardEntity>), new XmlRootAttribute("BusinessCards"));
                    businessCards = (List<BusinessCardEntity>)serializer.Deserialize(streamReader);
                }

                foreach (var card in businessCards)
                {
                    var missingFields = RequiredFields.Where(field =>
                        string.IsNullOrWhiteSpace(typeof(BusinessCardEntity).GetProperty(field)?.GetValue(card)?.ToString())
                    ).ToList();

                    if (missingFields.Any())
                    {
                        return Result.Failure<List<BusinessCardEntity>>(ApplicationErrors.File.InvalidFormat);
                    }
                }

                businessCards = CheckPhotoSizeForCards(businessCards);

                if (businessCards == null || businessCards.Any(bc => bc == null))
                {
                    return Result.Failure<List<BusinessCardEntity>>(ApplicationErrors.File.FileSizeExceeded);
                }
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<List<BusinessCardEntity>>(ApplicationErrors.File.ParsingXmlError);
            }
        }

        return Result.Success(businessCards);
    }


    private async Task<List<BusinessCardEntity>> ParseCsvFileAsync(IFormFile file)
    {
        var businessCards = new List<BusinessCardEntity>();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            var firstLine = await reader.ReadLineAsync();
            char delimiter;

            if (firstLine.Contains(';'))
            {
                delimiter = ';';
            }
            else if (firstLine.Contains(','))
            {
                delimiter = ',';
            }
            else
            {
                throw new InvalidOperationException(ApplicationErrors.File.UnsupportedCsvFormat.Message);
            }

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                var values = Regex.Matches(line, "(?:\"([^\"]*)\"|([^" + delimiter + "]+))")
                                .Cast<Match>()
                                .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)
                                .ToArray();

                if (values.Length < 6)
                {
                    throw new InvalidOperationException(ApplicationErrors.File.InvalidCsvFormat.Message);
                }

                var businessCard = new BusinessCardEntity
                {
                    Name = values[0],
                    Gender = (int)Enum.Parse(typeof(Gender), values[1]),
                    Email = values[2],
                    PhoneNumber = values[3],
                    DateOfBirth = DateTime.Parse(values[4]),
                    Address = values[5],  // "Cleaned" Address 
                    Photo = CheckIfPhotoExceeds1MB(values[6]),
                };

                businessCards.Add(businessCard);
            }
        }
        return businessCards;
    }


    private List<BusinessCardEntity> CheckPhotoSizeForCards(List<BusinessCardEntity> businessCards)
    {
        return businessCards.Select(card =>
        {
            card.Photo = CheckIfPhotoExceeds1MB(card.Photo);
            return card;
        }).ToList();
    }
    private string  CheckIfPhotoExceeds1MB(string photoBase64)
    {
        return (!string.IsNullOrEmpty(photoBase64) && photoBase64.Length > 1_398_132) ? null : photoBase64;
        // Rough estimate for 1MB Base64 string (~33% overhead added by Base64 encoding.)
    }

}

