using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Web.ViewModels.Clients;
using Hotel.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

[Authorize]
public class ClientsController : Controller
{
    private readonly IClientService _clientService;
    private readonly IRoomService _roomService;

    public ClientsController(IClientService clientService, IRoomService roomService)
    {
        _clientService = clientService;
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        const int pageSize = 15;
        var result = await _clientService.GetPagedAsync(page, pageSize, search).ConfigureAwait(false);
        var vm = new ClientIndexViewModel
        {
            Search = search,
            Clients = result.Items.Select(c => new ClientListItemViewModel
            {
                Id = c.Id,
                FullName = $"{c.FirstName} {c.LastName}",
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                IsAdult = c.IsAdult
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
    public IActionResult Create()
    {
        return View(new ClientCreateEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = new Client
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            IsAdult = model.IsAdult
        };
        await _clientService.CreateAsync(client).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _clientService.GetForEditAsync(id).ConfigureAwait(false);
        if (client == null)
        {
            return NotFound();
        }

        var vm = new ClientCreateEditViewModel
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            PhoneNumber = client.PhoneNumber,
            Email = client.Email,
            IsAdult = client.IsAdult
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClientCreateEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = await _clientService.GetForEditAsync(model.Id.Value).ConfigureAwait(false);
        if (client == null)
        {
            return NotFound();
        }

        client.FirstName = model.FirstName;
        client.LastName = model.LastName;
        client.PhoneNumber = model.PhoneNumber;
        client.Email = model.Email;
        client.IsAdult = model.IsAdult;
        await _clientService.UpdateAsync(client).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _clientService.GetByIdAsync(id).ConfigureAwait(false);
        if (client == null)
        {
            return NotFound();
        }

        return View(new ClientListItemViewModel
        {
            Id = client.Id,
            FullName = $"{client.FirstName} {client.LastName}",
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            IsAdult = client.IsAdult
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _clientService.DeleteAsync(id).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Reservations(int id, int page = 1, int? roomId = null)
    {
        const int pageSize = 10;
        var client = await _clientService.GetByIdAsync(id).ConfigureAwait(false);
        if (client == null)
        {
            return NotFound();
        }

        var roomsPage = await _roomService.GetPagedAsync(1, 500, null, null, null, null).ConfigureAwait(false);
        var roomItems = new List<SelectListItem> { new("All rooms", "", !roomId.HasValue) };
        roomItems.AddRange(roomsPage.Items.Select(r =>
            new SelectListItem(r.RoomNumber, r.Id.ToString(), roomId == r.Id)));

        var result = await _clientService
            .GetReservationsForClientAsync(id, page, pageSize, roomId)
            .ConfigureAwait(false);

        var vm = new ClientReservationsViewModel
        {
            ClientId = client.Id,
            ClientName = $"{client.FirstName} {client.LastName}",
            RoomFilterId = roomId,
            Rooms = roomItems,
            Reservations = result.Items.Select(r => new ClientReservationRowViewModel
            {
                Id = r.Id,
                RoomNumber = r.Room.RoomNumber,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                TotalAmount = r.TotalAmount,
                StaffName = $"{r.User.FirstName} {r.User.LastName}"
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
}
