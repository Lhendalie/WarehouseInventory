using FluentValidation;
using WarehouseInventory.Application.DTOs;

namespace WarehouseInventory.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50).WithMessage("Product code cannot exceed 50 characters")
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("Product code must contain only letters, numbers, and hyphens");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Group)
            .MaximumLength(100).WithMessage("Group cannot exceed 100 characters");

        RuleFor(x => x.Barcode)
            .MaximumLength(50).WithMessage("Barcode cannot exceed 50 characters");

        RuleFor(x => x.Warehouse)
            .MaximumLength(100).WithMessage("Warehouse cannot exceed 100 characters");

        RuleFor(x => x.Catalogue)
            .MaximumLength(50).WithMessage("Catalogue cannot exceed 50 characters");

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters");

        RuleFor(x => x.Condition)
            .MaximumLength(50).WithMessage("Condition cannot exceed 50 characters");

        RuleFor(x => x.ExpiryDate)
            .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future");
    }
}
