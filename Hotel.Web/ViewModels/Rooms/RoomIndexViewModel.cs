using Hotel.Data.Models;
using Hotel.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.ViewModels.Rooms;

public class RoomIndexViewModel
{
    public IReadOnlyList<RoomListItemViewModel> Rooms { get; init; } = Array.Empty<RoomListItemViewModel>();

    public PagerViewModel Pager { get; init; } = new();

    public int? MinCapacity { get; set; }

    public RoomType? Type { get; set; }

    public bool? IsFree { get; set; }

    public string? RoomNumberFilter { get; set; }

    public IEnumerable<SelectListItem> TypeOptions { get; set; } = Array.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> AvailabilityOptions { get; set; } = Array.Empty<SelectListItem>();
}
