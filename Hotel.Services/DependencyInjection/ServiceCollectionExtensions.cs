using Hotel.Services.Implementations;
using Hotel.Services.Interfaces;
using Hotel.Services.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Services.DependencyInjection;

/// <summary>
/// Registers application services for DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHotelServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ReservationPricingOptions>(configuration.GetSection(ReservationPricingOptions.SectionName));
        services.AddScoped<ReservationPricingCalculator>();
        services.AddScoped<IStaffUserService, StaffUserService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IRoomAvailabilityService, RoomAvailabilityService>();
        services.AddScoped<IReservationService, ReservationService>();
        return services;
    }
}
