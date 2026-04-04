using System.ComponentModel.DataAnnotations;
using Hotel.Web.Authorization;

namespace Hotel.Web.ViewModels.Staff;

public class StaffUserCreateViewModel
{
    [Required]
    [StringLength(256)]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Middle name")]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 10)]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must be exactly 10 digits.")]
    public string EGN { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    [Display(Name = "Phone")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Hire date")]
    public DateTime HireDate { get; set; } = DateTime.Today;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [DataType(DataType.Date)]
    [Display(Name = "Dismissal date")]
    public DateTime? DismissalDate { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Select at least one role.")]
    [Display(Name = "Roles")]
    public List<string> SelectedRoles { get; set; } = new() { HotelRoles.Staff };
}
