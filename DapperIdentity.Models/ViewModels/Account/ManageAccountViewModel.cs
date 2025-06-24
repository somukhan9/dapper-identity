using System.ComponentModel.DataAnnotations;

namespace DapperIdentity.Models.ViewModels.Account;

public class ProfileViewModel
{
    [Required]
    [Display(Name = "User Name")]
    public string? Username { get; set; }

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Phone]
    [Display(Name = "Phone number")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
}

public class EmailViewModel
{
    [Required]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Required]
    [Display(Name = "New Email Address")]
    [DataType(DataType.EmailAddress)]
    public string? NewEmail { get; set; }

    [Display(Name = "Email Confirmation")]
    public bool IsEmailConfirmed { get; set; }
}

public class ChangePasswordViewModel
{
    [Required]
    [Display(Name = "Current password")]
    [DataType(DataType.Password)]
    public string? OldPassword { get; set; }

    [Required]
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [Required]
    [Display(Name = "Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
}