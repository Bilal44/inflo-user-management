using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Services.Domain.Validators;

public class DateOfBirthAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        // Let [Required] handle nulls
        if (value is not DateOnly date)
            return true;

        return date <= DateOnly.FromDateTime(DateTime.Today);
    }
}
