using Hotel.Data.Models;

namespace Hotel.Web.ViewModels.Rooms;

public class RoomListItemViewModel
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public RoomType Type { get; set; }

    public bool IsFree { get; set; }

    public decimal PriceAdult { get; set; }

    public decimal PriceChild { get; set; }
}
