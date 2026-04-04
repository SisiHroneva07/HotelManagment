using Hotel.Data;
using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services.Implementations;

/// <summary>
/// Guest persistence and reservation history queries.
/// </summary>
public class ClientService : IClientService
{
    private readonly ApplicationDbContext _db;

    public ClientService(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Client>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Clients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(c =>
                c.FirstName.Contains(s)
                || c.LastName.Contains(s)
                || c.Email.Contains(s)
                || c.PhoneNumber.Contains(s));
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Client> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <inheritdoc />
    public Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Client?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Client> CreateAsync(Client client, CancellationToken cancellationToken = default)
    {
        _db.Clients.Add(client);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return client;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        _db.Clients.Update(client);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Clients.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (entity != null)
        {
            _db.Clients.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<PagedResult<Reservation>> GetReservationsForClientAsync(
        int clientId,
        int page,
        int pageSize,
        int? roomId,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .Where(r => r.Clients.Any(c => c.Id == clientId));

        if (roomId.HasValue)
        {
            query = query.Where(r => r.RoomId == roomId.Value);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(r => r.CheckInDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Reservation> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
