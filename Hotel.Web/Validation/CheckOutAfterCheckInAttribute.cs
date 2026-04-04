using System.ComponentModel.DataAnnotations;

namespace Hotel.Web.Validation;

/// <summary>
/// Ensures the check-out date is strictly after the check-in date.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public sealed class CheckOutAfterCheckInAttribute : ValidationAttribute
{
    public string CheckInProperty { get; set; } = "CheckInDate";

    public string CheckOutProperty { get; set; } = "CheckOutDate";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = value ?? validationContext.ObjectInstance;
        var type = instance.GetType();
        var checkInProp = type.GetProperty(CheckInProperty);
        var checkOutProp = type.GetProperty(CheckOutProperty);
        if (checkInProp == null || checkOutProp == null)
        {
            return ValidationResult.Success;
        }

        var checkInObj = checkInProp.GetValue(instance);
        var checkOutObj = checkOutProp.GetValue(instance);
        if (checkInObj is not DateTime checkIn || checkOutObj is not DateTime checkOut)
        {
            return ValidationResult.Success;
        }

        if (checkOut <= checkIn)
        {
            return new ValidationResult("Check-out must be after check-in.");
        }

        return ValidationResult.Success;
    }
}
