namespace Hotel.Services.Interfaces;

/// <summary>
/// Keeps <c>Room.IsFree</c> aligned with checkout dates and active bookings.
/// </summary>
public interface IRoomAvailabilityService
{
    /// <summary>
    /// Recomputes availability flags for all rooms (e.g. after checkout day passes).
    /// </summary>
    Task RefreshAllRoomsAsync(CancellationToken cancellationToken = default);
}
