using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Attributes;

/// <summary>
/// Validates that the items of a collection have unique values of the given property (case-sensitive).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class UniqueByAttribute(string propertyName) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not IEnumerable items) return ValidationResult.Success;

        var seen = new HashSet<object>();
        foreach (var item in items)
        {
            var key = item?.GetType().GetProperty(propertyName)?.GetValue(item);
            if (key != null && !seen.Add(key))
            {
                return new ValidationResult(
                    ErrorMessage ?? $"{validationContext.DisplayName} contains duplicate {propertyName} '{key}'.");
            }
        }

        return ValidationResult.Success;
    }
}
