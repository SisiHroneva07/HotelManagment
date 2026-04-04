using Hotel.Data.Models;
using Hotel.Services.Models;

namespace Hotel.Services.Interfaces;

/// <summary>
/// Admin-only operations for hotel staff accounts.
/// </summary>
public interface IStaffUserService
{
    Task<PagedResult<ApplicationUser>> GetPagedAsync(
        int page,
        int pageSize,
        string? userNameFilter,
        string? nameFilter,
        string? emailFilter,
        CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors)> CreateAsync(
        ApplicationUser user,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors)> UpdateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors)> DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors)> SetRolesAsync(
        ApplicationUser user,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);
}
