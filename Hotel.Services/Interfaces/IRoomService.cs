using Hotel.Data.Models;
using Hotel.Services.Models;

namespace Hotel.Services.Interfaces;

/// <summary>
/// Room inventory operations; admin may mutate, staff typically read.
/// </summary>
public interface IRoomService
{
    Task<PagedResult<Room>> GetPagedAsync(
        int page,
        int pageSize,
        int? minCapacity,
        RoomType? type,
        bool? isFree,
        string? roomNumberFilter,
        CancellationToken cancellationToken = default);

    Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Tracked load for edit forms.</summary>
    Task<Room?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    Task<Room> CreateAsync(Room room, CancellationToken cancellationToken = default);

    Task UpdateAsync(Room room, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
