using FluentValidation.TestHelper;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Validators;

namespace WarehouseInventory.UnitTests.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator;

    public CreateProductValidatorTests()
    {
        _validator = new CreateProductValidator();
    }

    [Fact]
    public void Should_Have_Error_When_ProductCode_Is_Empty()
    {
        var dto = new CreateProductDto { ProductCode = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductCode);
    }

    [Fact]
    public void Should_Have_Error_When_ProductCode_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { ProductCode = new string('A', 51) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductCode);
    }

    [Fact]
    public void Should_Have_Error_When_ProductCode_Contains_Invalid_Characters()
    {
        var dto = new CreateProductDto { ProductCode = "ABC@123" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductCode);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ProductCode_Is_Valid()
    {
        var dto = new CreateProductDto { ProductCode = "ABC-123" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.ProductCode);
    }

    [Fact]
    public void Should_Have_Error_When_ProductName_Is_Empty()
    {
        var dto = new CreateProductDto { ProductName = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void Should_Have_Error_When_ProductName_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { ProductName = new string('A', 201) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void Should_Have_Error_When_Group_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { Group = new string('A', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Group);
    }

    [Fact]
    public void Should_Have_Error_When_Barcode_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { Barcode = new string('A', 51) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Barcode);
    }

    [Fact]
    public void Should_Have_Error_When_Warehouse_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { Warehouse = new string('A', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Warehouse);
    }

    [Fact]
    public void Should_Have_Error_When_Catalogue_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { Catalogue = new string('A', 51) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Catalogue);
    }

    [Fact]
    public void Should_Have_Error_When_Type_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { Type = new string('A', 51) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Should_Have_Error_When_Condition_Exceeds_MaxLength()
    {
        var dto = new CreateProductDto { Condition = new string('A', 51) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Condition);
    }

    [Fact]
    public void Should_Have_Error_When_ExpiryDate_Is_In_The_Past()
    {
        var dto = new CreateProductDto { ExpiryDate = DateTime.UtcNow.AddDays(-1) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryDate);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ExpiryDate_Is_In_The_Future()
    {
        var dto = new CreateProductDto { ExpiryDate = DateTime.UtcNow.AddDays(1) };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ExpiryDate_Is_Null()
    {
        var dto = new CreateProductDto { ExpiryDate = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
    }
}
