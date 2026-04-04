using Hotel.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.ViewModels.Clients;

public class ClientReservationsViewModel
{
    public int ClientId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public IReadOnlyList<ClientReservationRowViewModel> Reservations { get; init; } =
        Array.Empty<ClientReservationRowViewModel>();

    public PagerViewModel Pager { get; init; } = new();

    public int? RoomFilterId { get; set; }

    public IReadOnlyList<SelectListItem> Rooms { get; init; } = Array.Empty<SelectListItem>();
}
