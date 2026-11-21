using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Frends.FixedWidthFlatFile.CreateFromXml.Attributes;

/// <summary>
/// Validates that a property is required if another property has a specific value.
/// If a property is null, an empty collection, or an empty or white-space-only string, validation fails.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Untouched version already tested in template solution.")]
[AttributeUsage(AttributeTargets.Property)]
internal class RequiredIfAttribute(string dependentProperty, params object[] targetValues) : ValidationAttribute
{
    /// <summary>
    /// When true, a string of exactly one white-space character is accepted as a value.
    /// </summary>
    /// <example>true</example>
    public bool AllowSingleWhitespace { get; set; }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var field = validationContext.ObjectType.GetProperty(dependentProperty);

        if (field == null)
            return new ValidationResult($"Unknown property: {dependentProperty}");

        var dependentValue = field.GetValue(validationContext.ObjectInstance);

        if (!targetValues.Contains(dependentValue)) return ValidationResult.Success;

        if (value == null ||
            (value is string s && string.IsNullOrWhiteSpace(s) && !(AllowSingleWhitespace && s.Length == 1)) ||
            (value is IEnumerable collection && !collection.Cast<object>().Any()))
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
        }

        return ValidationResult.Success;
    }
}
