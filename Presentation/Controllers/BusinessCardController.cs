﻿using Application.UseCases.BusinessCard.Queries.ExportBusinessCards;
using Application.UseCases.BusinessCard.Commands.GenerateQrCode;
using Application.UseCases.BusinessCard.Commands.DecodeQrCode;
using Application.UseCases.General.Commands.ConvertToBase64;
using Application.UseCases.BusinessCard.Queries.GetByEmail;
using Application.UseCases.BusinessCard.Commands.Create;
using Application.UseCases.BusinessCard.Commands.Delete;
using Application.UseCases.BusinessCard.Commands.Import;
using Application.UseCases.BusinessCard.Queries.List;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Presentation.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Application.Validators;
using Domain.ValueObjects;
using Domain.Requests;
using Domain.Errors;
using MediatR;
using Domain.Entities.BusinessCard;


namespace Presentation.Controllers
{
    [AllowAnonymous]
    [Route("api/BusinessCard")]
    public class BusinessCardController : ApiController
    {
        public BusinessCardController(ISender sender) : base(sender)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] BusinessCardCreateRequest request, CancellationToken cancellationToken = default)
        {
            var emailResult = Email.Create(request.email);
            if (!emailResult.IsSuccess)
            {
                return BadRequest(emailResult.Error);
            }

            var command = new BusinessCardCreateCommand(
              request.name,
              request.gender,
              request.dateOfBirth,
              Email.Create(request.email),
              request.phoneNumber,
              request.address,
             request.photo,
              cancellationToken
            ); ;

            var result = await Sender.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : StatusCode(600, result.Error);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([CustomFileExtensions(".csv,.xml")] IFormFile FileImport , CancellationToken cancellationToken = default)
        {
            var command = new BusinessCardsImportCommand( FileImport,cancellationToken ); ;
            var result = await Sender.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : StatusCode(600, result.Error);
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {

            var query = new BusinessCardGetByIdQuery(id);
            var result = await Sender.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(result) : StatusCode(600, result.Error);
        }

        [HttpGet]
        public async Task<IActionResult> List(bool withBase64 = false, int pageNumber = 1, int pageSize = 10, string? search = null, string? orderBy = nameof(BusinessCard.LastUpdateAt) , string? orderDirection = "desc", CancellationToken cancellationToken = default)
        {
            var query = new BusinessCardListQuery(withBase64, pageNumber, pageSize, search, orderBy, orderDirection, cancellationToken);
            var result = await Sender.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(result) : StatusCode(600, result.Error);
        }


        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<Guid> IDs, bool forceDelete = false, CancellationToken cancellationToken = default)
        {
            if (IDs == null || !IDs.Any())
            {
                return BadRequest(ApplicationErrors.File.NoFileUploaded);
            }

            var command = new BusinessCardDeleteCommand(IDs, forceDelete, cancellationToken);
            var result = await Sender.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : StatusCode(600, result.Error);
        }



        [HttpPost("decodeQrCode")]
        public async Task<IActionResult> DecodeBusinessCardQrCode([CustomFileExtensions(".png,.jpg,.jpeg,.gif")] IFormFile QRCodeFile)
        {
            var command = new DecodeBusinessCardFromQrCodeCommand(QRCodeFile);
            var result = await Sender.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result);
        }


        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] List<Guid>? IDs, [FromQuery] string format = "csv", CancellationToken cancellationToken = default)
        {
            format = format.ToLower();
            var query = new ExportBusinessCardsQuery(IDs, format);
            var result = await Sender.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            var contentType = format.Equals("csv", StringComparison.OrdinalIgnoreCase) ? "text/csv" : "application/xml";
            var fileName = $"BusinessCards.{format}";

            return File(result.Value, contentType, fileName);
        }


        [HttpPost("convertImageToBase64")]
        public async Task<IActionResult> convertImageToBase64(
        [Required]
        [CustomFileSize(1 * 1024 * 1024)]
        [CustomFileExtensions(".png,.jpg,.jpeg,.gif")]
          IFormFile photoFile)
        {
            {
                var command = new ConvertToBase64ImageCommand(photoFile);
                var result = await Sender.Send(command);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error);
                }

                return Ok(new { Base64Image = result });
            }

        }


        [HttpPost("generateQrCode")]
        public async Task<IActionResult> GenerateQrCode([FromBody] BusinessCardCreateRequest businessCard)
        {
            var qrCodeBytes = await Sender.Send(new GenerateQrCodeCommand(businessCard.name, businessCard.gender, businessCard.dateOfBirth, businessCard.email, businessCard.phoneNumber, businessCard.address));
          
            if (qrCodeBytes == null || qrCodeBytes.IsFailure || qrCodeBytes.Value.Length == 0)
            {
                return StatusCode(600, ApplicationErrors.QrCode.QrCodeGenerationFailed);
            }

            var sanitizedCardName = businessCard.name.Replace(" ", "_");
            var fileName = $"QR_Code_BusinessCard_{sanitizedCardName}.png";

            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName,
                Inline = false
            };

            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

            return File(qrCodeBytes, "image/png");
        }


  
    }
}

