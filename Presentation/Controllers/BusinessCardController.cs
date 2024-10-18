using Application.UseCases.BusinessCard.Commands.Create;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Presentation.Abstractions;
using Microsoft.AspNetCore.Mvc;
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



  
    }
}

