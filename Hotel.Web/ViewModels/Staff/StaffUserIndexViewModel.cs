using Hotel.Web.ViewModels.Shared;

namespace Hotel.Web.ViewModels.Staff;

public class StaffUserIndexViewModel
{
    public IReadOnlyList<StaffUserListItemViewModel> Users { get; init; } = Array.Empty<StaffUserListItemViewModel>();

    public PagerViewModel Pager { get; init; } = new();

    public string? UserNameFilter { get; set; }

    public string? NameFilter { get; set; }

    public string? EmailFilter { get; set; }

    public int PageSize { get; set; } = 10;
}
