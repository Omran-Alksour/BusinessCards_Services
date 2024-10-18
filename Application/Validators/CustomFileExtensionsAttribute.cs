using Domain.Errors;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators
{
    public class CustomFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public CustomFileExtensionsAttribute(string extensions)
        {
            _extensions = extensions.Split(',').Select(e => e.ToLowerInvariant()).ToArray();
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file != null)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!_extensions.Contains(fileExtension))
                {
                    var allowedExtensions = string.Join(", ", _extensions);
                    return new ValidationResult(string.Format(ApplicationErrors.File.FileExtensionError, allowedExtensions));
                }
            }

            return ValidationResult.Success;
        }
    }
}
