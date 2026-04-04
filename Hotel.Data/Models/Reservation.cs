namespace Hotel.Data.Models;

/// <summary>
/// A stay booking linking a room, staff author, guests, and pricing options.
/// </summary>
public class Reservation
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    /// <summary>Staff member who registered the reservation.</summary>
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public ICollection<Client> Clients { get; set; } = new List<Client>();

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public bool HasBreakfast { get; set; }

    public bool IsAllInclusive { get; set; }

    public decimal TotalAmount { get; set; }
}
