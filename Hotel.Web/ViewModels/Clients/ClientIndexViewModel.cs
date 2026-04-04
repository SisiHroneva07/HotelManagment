using Hotel.Web.ViewModels.Shared;

namespace Hotel.Web.ViewModels.Clients;

public class ClientIndexViewModel
{
    public IReadOnlyList<ClientListItemViewModel> Clients { get; init; } = Array.Empty<ClientListItemViewModel>();

    public PagerViewModel Pager { get; init; } = new();

    public string? Search { get; set; }
}
