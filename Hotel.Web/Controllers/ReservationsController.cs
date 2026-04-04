using System.Security.Claims;
using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Web.ViewModels.Reservations;
using Hotel.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly IRoomService _roomService;
    private readonly IClientService _clientService;

    public ReservationsController(
        IReservationService reservationService,
        IRoomService roomService,
        IClientService clientService)
    {
        _reservationService = reservationService;
        _roomService = roomService;
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        int? roomId = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        const int pageSize = 15;
        var result = await _reservationService
            .GetPagedAsync(page, pageSize, roomId, from, to)
            .ConfigureAwait(false);

        var rooms = await _roomService.GetPagedAsync(1, 500, null, null, null, null).ConfigureAwait(false);
        var roomSelect = new List<SelectListItem> { new("All rooms", "", !roomId.HasValue) };
        roomSelect.AddRange(rooms.Items.Select(r =>
            new SelectListItem(r.RoomNumber, r.Id.ToString(), roomId == r.Id)));

        var vm = new ReservationIndexViewModel
        {
            RoomId = roomId,
            From = from,
            To = to,
            Rooms = roomSelect,
            Reservations = result.Items.Select(r => new ReservationListItemViewModel
            {
                Id = r.Id,
                RoomNumber = r.Room.RoomNumber,
                StaffName = $"{r.User.FirstName} {r.User.LastName}",
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                TotalAmount = r.TotalAmount,
                GuestCount = r.Clients.Count
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
    public async Task<IActionResult> Create()
    {
        var vm = await BuildEditorViewModelAsync(new ReservationCreateEditViewModel()).ConfigureAwait(false);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await HydrateSelectListsAsync(model).ConfigureAwait(false);
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var entity = new Reservation
        {
            RoomId = model.RoomId,
            UserId = userId,
            CheckInDate = model.CheckInDate,
            CheckOutDate = model.CheckOutDate,
            HasBreakfast = model.HasBreakfast,
            IsAllInclusive = model.IsAllInclusive
        };

        try
        {
            await _reservationService.CreateAsync(entity, model.SelectedClientIds).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await HydrateSelectListsAsync(model).ConfigureAwait(false);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var res = await _reservationService.GetForEditAsync(id).ConfigureAwait(false);
        if (res == null)
        {
            return NotFound();
        }

        var vm = new ReservationCreateEditViewModel
        {
            Id = res.Id,
            RoomId = res.RoomId,
            CheckInDate = res.CheckInDate,
            CheckOutDate = res.CheckOutDate,
            HasBreakfast = res.HasBreakfast,
            IsAllInclusive = res.IsAllInclusive,
            SelectedClientIds = res.Clients.Select(c => c.Id).ToList()
        };
        await HydrateSelectListsAsync(vm).ConfigureAwait(false);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReservationCreateEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await HydrateSelectListsAsync(model).ConfigureAwait(false);
            return View(model);
        }

        var existing = await _reservationService.GetForEditAsync(model.Id.Value).ConfigureAwait(false);
        if (existing == null)
        {
            return NotFound();
        }

        existing.RoomId = model.RoomId;
        existing.CheckInDate = model.CheckInDate;
        existing.CheckOutDate = model.CheckOutDate;
        existing.HasBreakfast = model.HasBreakfast;
        existing.IsAllInclusive = model.IsAllInclusive;

        try
        {
            await _reservationService.UpdateAsync(existing, model.SelectedClientIds).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await HydrateSelectListsAsync(model).ConfigureAwait(false);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _reservationService.GetByIdAsync(id).ConfigureAwait(false);
        if (res == null)
        {
            return NotFound();
        }

        return View(new ReservationListItemViewModel
        {
            Id = res.Id,
            RoomNumber = res.Room.RoomNumber,
            StaffName = $"{res.User.FirstName} {res.User.LastName}",
            CheckInDate = res.CheckInDate,
            CheckOutDate = res.CheckOutDate,
            TotalAmount = res.TotalAmount,
            GuestCount = res.Clients.Count
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _reservationService.DeleteAsync(id).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    private async Task<ReservationCreateEditViewModel> BuildEditorViewModelAsync(ReservationCreateEditViewModel vm)
    {
        await HydrateSelectListsAsync(vm).ConfigureAwait(false);
        return vm;
    }

    private async Task HydrateSelectListsAsync(ReservationCreateEditViewModel vm)
    {
        var rooms = await _roomService.GetPagedAsync(1, 500, null, null, null, null).ConfigureAwait(false);
        vm.RoomOptions = rooms.Items
            .Select(r => new SelectListItem($"{r.RoomNumber} ({r.Type})", r.Id.ToString(), r.Id == vm.RoomId))
            .ToList();

        var clients = await _clientService.GetPagedAsync(1, 2000, null).ConfigureAwait(false);
        var selected = vm.SelectedClientIds.ToHashSet();
        vm.ClientOptions = clients.Items
            .Select(c => new SelectListItem(
                $"{c.FirstName} {c.LastName} ({(c.IsAdult ? "adult" : "child")})",
                c.Id.ToString(),
                selected.Contains(c.Id)))
            .ToList();
    }
}
