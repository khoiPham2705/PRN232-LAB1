using FluentValidation;
using PRN232.LMS.Services.RequestModels;

namespace PRN232.LMS.Services.Validation;

public class StudentRequestValidator : AbstractValidator<StudentRequest>
{
    public StudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\d]*$").WithMessage("Full Name must not contain special characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of Birth is required.")
            .LessThan(DateTime.UtcNow).WithMessage("Date of Birth must be in the past.");
    }
}
