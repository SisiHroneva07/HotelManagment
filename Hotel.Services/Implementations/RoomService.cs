using Hotel.Data;
using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services.Implementations;

/// <summary>
/// Room CRUD and filtered listings for staff dashboards.
/// </summary>
public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _db;

    public RoomService(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Room>> GetPagedAsync(
        int page,
        int pageSize,
        int? minCapacity,
        RoomType? type,
        bool? isFree,
        string? roomNumberFilter,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Rooms.AsNoTracking().AsQueryable();

        if (minCapacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(r => r.Type == type.Value);
        }

        if (isFree.HasValue)
        {
            query = query.Where(r => r.IsFree == isFree.Value);
        }

        if (!string.IsNullOrWhiteSpace(roomNumberFilter))
        {
            var f = roomNumberFilter.Trim();
            query = query.Where(r => r.RoomNumber.Contains(f));
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderBy(r => r.RoomNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Room> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <inheritdoc />
    public Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Room?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Room> CreateAsync(Room room, CancellationToken cancellationToken = default)
    {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return room;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        _db.Rooms.Update(room);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Rooms.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (entity != null)
        {
            _db.Rooms.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
