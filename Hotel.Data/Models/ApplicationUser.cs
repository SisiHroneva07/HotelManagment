using Microsoft.AspNetCore.Identity;

namespace Hotel.Data.Models;

/// <summary>
/// Hotel staff account with extended profile and employment metadata.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = string.Empty;

    /// <summary>Unified civil number (Bulgarian EGN).</summary>
    public string EGN { get; set; } = string.Empty;

    public DateTime HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? DismissalDate { get; set; }

    public ICollection<Reservation> ReservationsCreated { get; set; } = new List<Reservation>();
}
