using System.ComponentModel.DataAnnotations;
using Hotel.Web.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.ViewModels.Reservations;

[CheckOutAfterCheckIn]
public class ReservationCreateEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Room")]
    public int RoomId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Check-in")]
    public DateTime CheckInDate { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Check-out")]
    public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

    [Display(Name = "Breakfast")]
    public bool HasBreakfast { get; set; }

    [Display(Name = "All inclusive")]
    public bool IsAllInclusive { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Select at least one guest.")]
    [Display(Name = "Guests")]
    public List<int> SelectedClientIds { get; set; } = new();

    public IReadOnlyList<SelectListItem> RoomOptions { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> ClientOptions { get; set; } = Array.Empty<SelectListItem>();
}
