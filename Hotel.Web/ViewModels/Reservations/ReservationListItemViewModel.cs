namespace Hotel.Web.ViewModels.Reservations;

public class ReservationListItemViewModel
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string StaffName { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal TotalAmount { get; set; }

    public int GuestCount { get; set; }
}
