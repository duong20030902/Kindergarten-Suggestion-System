using System.ComponentModel.DataAnnotations;

namespace Group03_Kindergarten_Suggestion_System_Project.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName)?.ToLower();
                string[] extensions = { ".png", ".jpg", ".jpeg" };

                if (string.IsNullOrEmpty(extension) || !extensions.Contains(extension))
                {
                    return new ValidationResult("Only png, jpg, jpeg formats are accepted.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
