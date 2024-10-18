using Application.UseCases.BusinessCard.Commands.Create;
using Microsoft.AspNetCore.Authorization;
using Presentation.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Domain.ValueObjects;
using Domain.Requests;
using MediatR;


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

      }
}

