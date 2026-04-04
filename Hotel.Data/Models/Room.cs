namespace Hotel.Data.Models;

/// <summary>
/// Physical room inventory with nightly pricing for adults and children.
/// </summary>
public class Room
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public RoomType Type { get; set; }

    /// <summary>Whether the room is currently available for new bookings.</summary>
    public bool IsFree { get; set; } = true;

    public decimal PriceAdult { get; set; }

    public decimal PriceChild { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
