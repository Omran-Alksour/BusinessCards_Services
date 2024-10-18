using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Domain.Errors; 

namespace Application.Validators
{
    public class CustomFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;

        public CustomFileSizeAttribute(long maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file != null && file.Length > _maxFileSize)
            {
                var maxSizeInMb = _maxFileSize / (1024 * 1024);
                var errorMessage = string.Format(ApplicationErrors.File.FileSizeExceeded.Message, maxSizeInMb);

                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
