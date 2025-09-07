using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Services.Domain.Validators;

namespace UserManagement.Services.Domain.Models;

public class UserModel
{
    /// <summary>
    /// Unique user Id.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Forename of the user.
    /// </summary>
    [Required]
    [StringLength(32, ErrorMessage = "Forename cannot exceed 32 characters.")]
    public string Forename { get; set; } = string.Empty;

    /// <summary>
    /// Surname of the user.
    /// </summary>
    [Required]
    [StringLength(32, ErrorMessage = "Surname cannot exceed 32 characters.")]
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Unique email address associated with a user account.
    /// </summary>
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(128, ErrorMessage = "Email cannot exceed 128 characters.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Date of birth of the user.
    /// </summary>
    [DataType(DataType.Date)]
    [DateOfBirth(ErrorMessage = "Date of birth cannot be in the future.")]
    [Display(Name = "Date of Birth")]
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// A flag indicating whether the user is currently active.
    /// </summary>
    [Display(Name = "Account Active")]
    public bool IsActive { get; set; }
}
