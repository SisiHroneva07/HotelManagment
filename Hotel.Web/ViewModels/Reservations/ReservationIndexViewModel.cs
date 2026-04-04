using Hotel.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.ViewModels.Reservations;

public class ReservationIndexViewModel
{
    public IReadOnlyList<ReservationListItemViewModel> Reservations { get; init; } =
        Array.Empty<ReservationListItemViewModel>();

    public PagerViewModel Pager { get; init; } = new();

    public int? RoomId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public IReadOnlyList<SelectListItem> Rooms { get; init; } = Array.Empty<SelectListItem>();
}
