using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Services.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hotel.Data;

namespace Hotel.Services.Implementations;

/// <summary>
/// Admin CRUD for <see cref="ApplicationUser"/> backed by Identity stores.
/// </summary>
public class StaffUserService : IStaffUserService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public StaffUserService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ApplicationUser>> GetPagedAsync(
        int page,
        int pageSize,
        string? userNameFilter,
        string? nameFilter,
        string? emailFilter,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = pageSize is 10 or 25 or 50 ? pageSize : 10;

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(userNameFilter))
        {
            var f = userNameFilter.Trim();
            query = query.Where(u => u.UserName != null && u.UserName.Contains(f));
        }

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var f = nameFilter.Trim();
            query = query.Where(u =>
                u.FirstName.Contains(f)
                || u.LastName.Contains(f)
                || (u.MiddleName != null && u.MiddleName.Contains(f)));
        }

        if (!string.IsNullOrWhiteSpace(emailFilter))
        {
            var f = emailFilter.Trim();
            query = query.Where(u => u.Email != null && u.Email.Contains(f));
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<ApplicationUser>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _userManager.FindByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<(bool Success, string[] Errors)> CreateAsync(
        ApplicationUser user,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        foreach (var role in roles.Distinct())
        {
            if (!await _roleManager.RoleExistsAsync(role).ConfigureAwait(false))
            {
                await _roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
            }
        }

        var roleResult = await _userManager.AddToRolesAsync(user, roles).ConfigureAwait(false);
        if (!roleResult.Succeeded)
        {
            return (false, roleResult.Errors.Select(e => e.Description).ToArray());
        }

        return (true, Array.Empty<string>());
    }

    /// <inheritdoc />
    public async Task<(bool Success, string[] Errors)> UpdateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
        return result.Succeeded
            ? (true, Array.Empty<string>())
            : (false, result.Errors.Select(e => e.Description).ToArray());
    }

    /// <inheritdoc />
    public async Task<(bool Success, string[] Errors)> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
        if (user == null)
        {
            return (false, new[] { "User not found." });
        }

        var result = await _userManager.DeleteAsync(user).ConfigureAwait(false);
        return result.Succeeded
            ? (true, Array.Empty<string>())
            : (false, result.Errors.Select(e => e.Description).ToArray());
    }

    /// <inheritdoc />
    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return _userManager.GetRolesAsync(user);
    }

    /// <inheritdoc />
    public async Task<(bool Success, string[] Errors)> SetRolesAsync(
        ApplicationUser user,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var current = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        var remove = await _userManager.RemoveFromRolesAsync(user, current).ConfigureAwait(false);
        if (!remove.Succeeded)
        {
            return (false, remove.Errors.Select(e => e.Description).ToArray());
        }

        foreach (var role in roles.Distinct())
        {
            if (!await _roleManager.RoleExistsAsync(role).ConfigureAwait(false))
            {
                await _roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
            }
        }

        var add = await _userManager.AddToRolesAsync(user, roles).ConfigureAwait(false);
        return add.Succeeded
            ? (true, Array.Empty<string>())
            : (false, add.Errors.Select(e => e.Description).ToArray());
    }
}
