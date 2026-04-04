using Hotel.Data.Models;
using Hotel.Services.Models;

namespace Hotel.Services.Interfaces;

/// <summary>
/// Guest CRUD and reservation history queries.
/// </summary>
public interface IClientService
{
    Task<PagedResult<Client>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

    Task<Client?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Tracked load for edit forms.</summary>
    Task<Client?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    Task<Client> CreateAsync(Client client, CancellationToken cancellationToken = default);

    Task UpdateAsync(Client client, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prior reservations for a client with optional room filter and paging.
    /// </summary>
    Task<PagedResult<Reservation>> GetReservationsForClientAsync(
        int clientId,
        int page,
        int pageSize,
        int? roomId,
        CancellationToken cancellationToken = default);
}
