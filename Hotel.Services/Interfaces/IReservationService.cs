using Hotel.Data.Models;
using Hotel.Services.Models;

namespace Hotel.Services.Interfaces;

/// <summary>
/// Booking workflow: overlap checks, totals, and persistence.
/// </summary>
public interface IReservationService
{
    Task<PagedResult<Reservation>> GetPagedAsync(
        int page,
        int pageSize,
        int? roomId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Tracked load with guests for edit forms.</summary>
    Task<Reservation?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a reservation; throws <see cref="InvalidOperationException"/> on overlap or business rule violation.
    /// </summary>
    Task<Reservation> CreateAsync(
        Reservation reservation,
        IEnumerable<int> clientIds,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Reservation reservation,
        IEnumerable<int> clientIds,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if another reservation for the same room overlaps the given window.
    /// </summary>
    Task<bool> HasOverlappingReservationAsync(
        int roomId,
        DateTime checkIn,
        DateTime checkOut,
        int? excludeReservationId,
        CancellationToken cancellationToken = default);
}
