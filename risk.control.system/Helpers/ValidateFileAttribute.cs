using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Helpers
{
    public class ValidateFileAttribute : ValidationAttribute
    {
        private int _maxFileSize = 1024 * 1024 * 5;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            var size = _maxFileSize / 1024;
            return $"Maximum allowed file size is {size.ToString()} MB.";
        }
    }
}