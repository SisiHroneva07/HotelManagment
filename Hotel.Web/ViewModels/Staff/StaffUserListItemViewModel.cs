namespace Hotel.Web.ViewModels.Staff;

public class StaffUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime? DismissalDate { get; set; }

    public IList<string> Roles { get; set; } = Array.Empty<string>();
}
