using System.ComponentModel.DataAnnotations;

namespace DapperIdentity.Models.ViewModels.Account;

public class ManageAccountViewModel
{
    [Required]
    [Display(Name = "User Name")]
    public string Username;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    public string? StatusMessage { get; set; }
}