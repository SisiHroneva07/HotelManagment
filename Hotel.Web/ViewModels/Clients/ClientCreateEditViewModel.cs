using System.ComponentModel.DataAnnotations;

namespace Hotel.Web.ViewModels.Clients;

public class ClientCreateEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    [Display(Name = "Phone")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Adult guest")]
    public bool IsAdult { get; set; } = true;
}
