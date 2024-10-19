using Application.Abstractions.Messaging;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using System.Text;
using System.Xml.Serialization;
using BusinessCardEntity = Domain.Entities.BusinessCard.BusinessCard;


namespace Application.UseCases.BusinessCard.Queries.ExportBusinessCards
{
    public class ExportBusinessCardsQueryHandler : IQueryHandler<ExportBusinessCardsQuery, byte[]>
    {
        private readonly IBusinessCardRepository _businessCardRepository;

        public ExportBusinessCardsQueryHandler(IBusinessCardRepository businessCardRepository)
        {
            _businessCardRepository = businessCardRepository;
        }

        public async Task<Result<byte[]>> Handle(ExportBusinessCardsQuery query, CancellationToken cancellationToken)
        {
            var businessCards = await _businessCardRepository.GetByIdsAsync(query.BusinessCardIds);

            if (!businessCards.Any())
            {
                return Result.Failure<byte[]>(ApplicationErrors.BusinessCards.Queries.NoBusinessCardsFound);
            }

            // Handle CSV export
            if (query.Format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                var csvBytes = GenerateCsv(businessCards);
                return Result.Success(csvBytes);
            }

            // Handle XML export
            else if (query.Format.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                var xmlBytes = GenerateXml(businessCards);
                return Result.Success(xmlBytes);
            }

            return Result.Failure<byte[]>(ApplicationErrors.File.UnsupportedExportFormat);
        }


        private byte[] GenerateXml(List<BusinessCardEntity>  businessCards)
        {
            List<Responses.BusinessCard> businessCardsResponse = businessCards.Select(b=> new Responses.BusinessCard(b.Name,b.Gender,b.Email,b.PhoneNumber,b.DateOfBirth,b.Address, b.Photo)).ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<Responses.BusinessCard>));

            using var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, businessCardsResponse);

            var xml = stringWriter.ToString();
            xml = xml.Replace("ArrayOfBusinessCard", "BusinessCards");

            return Encoding.UTF8.GetBytes(xml);
        }



        private byte[] GenerateCsv(List<BusinessCardEntity> businessCards)
        {
            var csvBuilder = new StringBuilder();

            csvBuilder.AppendLine("Name;Gender;Email;PhoneNumber;DateOfBirth;Address;Photo");

            foreach (var card in businessCards)
            {
                var csvLine = string.Join(";",
                    EscapeCsvValue(card.Name),
                    EscapeCsvValue(card.Gender.ToString()),
                    EscapeCsvValue(card.Email),
                    EscapeCsvValue(card.PhoneNumber),
                    EscapeCsvValue(card.DateOfBirth.ToString("yyyy-MM-dd")),
                    EscapeCsvValue(card.Address),
                    EscapeCsvValue(card.Photo) 
                );

                csvBuilder.AppendLine(csvLine);
            }

            return Encoding.UTF8.GetBytes(csvBuilder.ToString());
        }

        private string EscapeCsvValue(string value)
        {
            if (!string.IsNullOrEmpty(value) && (value.Contains(";") || value.Contains(",") || value.Contains("\"")))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }

    }

}
