using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Requests;


public sealed class BusinessCardCreateRequest
{
    [Required(ErrorMessage = "The name field is required.")]
    public string name { get; set; } = string.Empty;

    [Required(ErrorMessage = "The gender field is required.")]
    public Gender gender { get; set; }

    [Required(ErrorMessage = "The Date of Birth field is required.")]
    public DateTime dateOfBirth { get; set; }

    [Required(ErrorMessage = "The email field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string email { get; set; } = string.Empty;


    [Required(ErrorMessage = "The phoneNumber Number field is required.")]
    public string phoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "The address field is required.")]
    public string address { get; set; } = string.Empty;

    [MaxLength(1000000)] // Max length of 1MB
    public string? photo { get; set; }
}



public sealed class BusinessCardByEmailRequest
{
    [Required(ErrorMessage = "The email field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? email { get; set; }
}
