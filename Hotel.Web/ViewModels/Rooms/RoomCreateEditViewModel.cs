using System.ComponentModel.DataAnnotations;
using Hotel.Data.Models;

namespace Hotel.Web.ViewModels.Rooms;

public class RoomCreateEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Room number")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 20)]
    public int Capacity { get; set; } = 2;

    [Required]
    public RoomType Type { get; set; } = RoomType.Double;

    [Display(Name = "Available")]
    public bool IsFree { get; set; } = true;

    [Required]
    [Range(typeof(decimal), "0", "999999")]
    [Display(Name = "Adult rate / night")]
    public decimal PriceAdult { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "999999")]
    [Display(Name = "Child rate / night")]
    public decimal PriceChild { get; set; }
}
