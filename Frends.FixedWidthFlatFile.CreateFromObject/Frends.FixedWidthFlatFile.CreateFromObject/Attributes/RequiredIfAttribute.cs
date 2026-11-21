using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.CreateFromObject.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class RequiredIfAttribute(string dependentProperty, object targetValue) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(dependentProperty);
        if (property == null)
            return new ValidationResult($"Unknown property: {dependentProperty}");

        if (!Equals(property.GetValue(validationContext.ObjectInstance), targetValue))
            return ValidationResult.Success;

        return value is string text && !string.IsNullOrWhiteSpace(text)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
    }
}
