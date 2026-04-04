namespace Hotel.Data.Models;

/// <summary>
/// Guest profile used for reservations and CRM-style lookups.
/// </summary>
public class Client
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsAdult { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
