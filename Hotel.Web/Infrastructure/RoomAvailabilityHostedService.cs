using Hotel.Services.Interfaces;

namespace Hotel.Web.Infrastructure;

/// <summary>
/// Periodically refreshes room <c>IsFree</c> flags after check-outs elapse.
/// </summary>
public class RoomAvailabilityHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RoomAvailabilityHostedService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public RoomAvailabilityHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<RoomAvailabilityHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IRoomAvailabilityService>();
                await svc.RefreshAllRoomsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Room availability refresh failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
