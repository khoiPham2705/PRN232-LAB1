using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PRN232.LMS.Services.Helpers;

public class FptuCodeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        var code = value.ToString();
        // 2 letters (case-insensitive) followed by exactly 6 digits
        if (Regex.IsMatch(code ?? "", @"^[a-zA-Z]{2}\d{6}$"))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("Invalid FPTU code format (e.g., se231231, ss123456).");
    }
}
