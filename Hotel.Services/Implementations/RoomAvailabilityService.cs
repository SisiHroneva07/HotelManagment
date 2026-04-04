using Hotel.Data;
using Hotel.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services.Implementations;

/// <summary>
/// Updates <c>Room.IsFree</c> from active reservation coverage for "today".
/// </summary>
public class RoomAvailabilityService : IRoomAvailabilityService
{
    private readonly ApplicationDbContext _db;

    public RoomAvailabilityService(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task RefreshAllRoomsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;

        var rooms = await _db.Rooms.ToListAsync(cancellationToken).ConfigureAwait(false);
        var activeRoomIds = await _db.Reservations
            .AsNoTracking()
            .Where(r => r.CheckInDate.Date <= today && r.CheckOutDate.Date > today)
            .Select(r => r.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var busy = activeRoomIds.ToHashSet();
        foreach (var room in rooms)
        {
            room.IsFree = !busy.Contains(room.Id);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
