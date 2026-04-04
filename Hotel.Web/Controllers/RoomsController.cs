using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Web.Authorization;
using Hotel.Web.ViewModels.Rooms;
using Hotel.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

[Authorize(Roles = $"{HotelRoles.Admin},{HotelRoles.Staff}")]
public class RoomsController : Controller
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        int? minCapacity = null,
        RoomType? type = null,
        bool? isFree = null,
        string? roomNumber = null)
    {
        pageSize = Math.Clamp(pageSize, 5, 50);
        var result = await _roomService
            .GetPagedAsync(page, pageSize, minCapacity, type, isFree, roomNumber)
            .ConfigureAwait(false);

        var vm = new RoomIndexViewModel
        {
            MinCapacity = minCapacity,
            Type = type,
            IsFree = isFree,
            RoomNumberFilter = roomNumber,
            TypeOptions = GetTypeOptions(type),
            AvailabilityOptions = GetAvailabilityOptions(isFree),
            Rooms = result.Items.Select(r => new RoomListItemViewModel
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Capacity = r.Capacity,
                Type = r.Type,
                IsFree = r.IsFree,
                PriceAdult = r.PriceAdult,
                PriceChild = r.PriceChild
            }).ToList(),
            Pager = new PagerViewModel
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            }
        };
        return View(vm);
    }

    [HttpGet]
    [Authorize(Roles = HotelRoles.Admin)]
    public IActionResult Create()
    {
        return View(new RoomCreateEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = HotelRoles.Admin)]
    public async Task<IActionResult> Create(RoomCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var room = new Room
        {
            RoomNumber = model.RoomNumber,
            Capacity = model.Capacity,
            Type = model.Type,
            IsFree = model.IsFree,
            PriceAdult = model.PriceAdult,
            PriceChild = model.PriceChild
        };
        await _roomService.CreateAsync(room).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = HotelRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetForEditAsync(id).ConfigureAwait(false);
        if (room == null)
        {
            return NotFound();
        }

        var vm = new RoomCreateEditViewModel
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            Type = room.Type,
            IsFree = room.IsFree,
            PriceAdult = room.PriceAdult,
            PriceChild = room.PriceChild
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = HotelRoles.Admin)]
    public async Task<IActionResult> Edit(RoomCreateEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var room = await _roomService.GetForEditAsync(model.Id.Value).ConfigureAwait(false);
        if (room == null)
        {
            return NotFound();
        }

        room.RoomNumber = model.RoomNumber;
        room.Capacity = model.Capacity;
        room.Type = model.Type;
        room.IsFree = model.IsFree;
        room.PriceAdult = model.PriceAdult;
        room.PriceChild = model.PriceChild;
        await _roomService.UpdateAsync(room).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = HotelRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _roomService.GetByIdAsync(id).ConfigureAwait(false);
        if (room == null)
        {
            return NotFound();
        }

        return View(new RoomListItemViewModel
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            Type = room.Type,
            IsFree = room.IsFree,
            PriceAdult = room.PriceAdult,
            PriceChild = room.PriceChild
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = HotelRoles.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _roomService.DeleteAsync(id).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    private static IEnumerable<SelectListItem> GetTypeOptions(RoomType? selected)
    {
        yield return new SelectListItem("Any type", "", !selected.HasValue);
        foreach (RoomType t in Enum.GetValues(typeof(RoomType)))
        {
            yield return new SelectListItem(t.ToString(), ((int)t).ToString(), selected == t);
        }
    }

    private static IEnumerable<SelectListItem> GetAvailabilityOptions(bool? selected)
    {
        yield return new SelectListItem("Any", "", !selected.HasValue);
        yield return new SelectListItem("Available", "true", selected == true);
        yield return new SelectListItem("Occupied / held", "false", selected == false);
    }
}
