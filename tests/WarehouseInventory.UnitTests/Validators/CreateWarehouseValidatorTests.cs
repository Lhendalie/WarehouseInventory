using FluentValidation.TestHelper;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Validators;

namespace WarehouseInventory.UnitTests.Validators;

public class CreateWarehouseValidatorTests
{
    private readonly CreateWarehouseValidator _validator;

    public CreateWarehouseValidatorTests()
    {
        _validator = new CreateWarehouseValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new CreateWarehouseDto { Name = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var dto = new CreateWarehouseDto { Name = new string('A', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var dto = new CreateWarehouseDto { Name = "Main Warehouse" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Location_Exceeds_MaxLength()
    {
        var dto = new CreateWarehouseDto { Location = new string('A', 501) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Location_Is_Within_MaxLength()
    {
        var dto = new CreateWarehouseDto { Location = new string('A', 500) };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Location);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Location_Is_Null()
    {
        var dto = new CreateWarehouseDto { Location = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Location);
    }
}
