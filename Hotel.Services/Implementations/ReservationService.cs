using Hotel.Data;
using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services.Implementations;

/// <summary>
/// Reservation persistence with overlap validation and computed totals.
/// </summary>
public class ReservationService : IReservationService
{
    private readonly ApplicationDbContext _db;
    private readonly ReservationPricingCalculator _calculator;
    private readonly IRoomAvailabilityService _roomAvailability;

    public ReservationService(
        ApplicationDbContext db,
        ReservationPricingCalculator calculator,
        IRoomAvailabilityService roomAvailability)
    {
        _db = db;
        _calculator = calculator;
        _roomAvailability = roomAvailability;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Reservation>> GetPagedAsync(
        int page,
        int pageSize,
        int? roomId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .Include(r => r.Clients)
            .AsQueryable();

        if (roomId.HasValue)
        {
            query = query.Where(r => r.RoomId == roomId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(r => r.CheckOutDate >= from.Value.Date);
        }

        if (to.HasValue)
        {
            query = query.Where(r => r.CheckInDate <= to.Value.Date);
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

    /// <inheritdoc />
    public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .Include(r => r.Clients)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Reservation?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Reservations
            .Include(r => r.Room)
            .Include(r => r.User)
            .Include(r => r.Clients)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasOverlappingReservationAsync(
        int roomId,
        DateTime checkIn,
        DateTime checkOut,
        int? excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        var cin = checkIn.Date;
        var cout = checkOut.Date;

        var query = _db.Reservations.AsNoTracking().Where(r => r.RoomId == roomId);
        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        // Half-open interval [checkIn, checkOut): overlap if start < otherEnd && otherStart < end
        return await query.AnyAsync(
                r => cin < r.CheckOutDate.Date && cout > r.CheckInDate.Date,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Reservation> CreateAsync(
        Reservation reservation,
        IEnumerable<int> clientIds,
        CancellationToken cancellationToken = default)
    {
        if (reservation.CheckOutDate.Date <= reservation.CheckInDate.Date)
        {
            throw new InvalidOperationException("Check-out must be after check-in.");
        }

        if (await HasOverlappingReservationAsync(
                reservation.RoomId,
                reservation.CheckInDate,
                reservation.CheckOutDate,
                null,
                cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The room already has a reservation overlapping these dates.");
        }

        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == reservation.RoomId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Room not found.");

        var ids = clientIds.Distinct().ToList();
        var clients = await _db.Clients.Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clients.Count != ids.Count)
        {
            throw new InvalidOperationException("One or more guests were not found.");
        }

        if (clients.Count > room.Capacity)
        {
            throw new InvalidOperationException("Guest count exceeds room capacity.");
        }

        var adults = clients.Count(c => c.IsAdult);
        var children = clients.Count - adults;

        reservation.TotalAmount = _calculator.CalculateTotal(
            room,
            adults,
            children,
            reservation.CheckInDate,
            reservation.CheckOutDate,
            reservation.HasBreakfast,
            reservation.IsAllInclusive);

        foreach (var c in clients)
        {
            reservation.Clients.Add(c);
        }

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _roomAvailability.RefreshAllRoomsAsync(cancellationToken).ConfigureAwait(false);
        return reservation;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        Reservation reservation,
        IEnumerable<int> clientIds,
        CancellationToken cancellationToken = default)
    {
        if (reservation.CheckOutDate.Date <= reservation.CheckInDate.Date)
        {
            throw new InvalidOperationException("Check-out must be after check-in.");
        }

        if (await HasOverlappingReservationAsync(
                reservation.RoomId,
                reservation.CheckInDate,
                reservation.CheckOutDate,
                reservation.Id,
                cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The room already has a reservation overlapping these dates.");
        }

        var entity = await _db.Reservations
            .Include(r => r.Clients)
            .FirstOrDefaultAsync(r => r.Id == reservation.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Reservation not found.");

        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == reservation.RoomId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Room not found.");

        var ids = clientIds.Distinct().ToList();
        var clients = await _db.Clients.Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (clients.Count != ids.Count)
        {
            throw new InvalidOperationException("One or more guests were not found.");
        }

        if (clients.Count > room.Capacity)
        {
            throw new InvalidOperationException("Guest count exceeds room capacity.");
        }

        entity.RoomId = reservation.RoomId;
        entity.UserId = reservation.UserId;
        entity.CheckInDate = reservation.CheckInDate;
        entity.CheckOutDate = reservation.CheckOutDate;
        entity.HasBreakfast = reservation.HasBreakfast;
        entity.IsAllInclusive = reservation.IsAllInclusive;

        entity.Clients.Clear();
        foreach (var c in clients)
        {
            entity.Clients.Add(c);
        }

        var adults = clients.Count(c => c.IsAdult);
        var children = clients.Count - adults;
        entity.TotalAmount = _calculator.CalculateTotal(
            room,
            adults,
            children,
            entity.CheckInDate,
            entity.CheckOutDate,
            entity.HasBreakfast,
            entity.IsAllInclusive);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _roomAvailability.RefreshAllRoomsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Reservations
            .Include(r => r.Clients)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
        {
            return;
        }

        entity.Clients.Clear();
        _db.Reservations.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _roomAvailability.RefreshAllRoomsAsync(cancellationToken).ConfigureAwait(false);
    }
}
