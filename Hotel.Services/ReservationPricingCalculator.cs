using Hotel.Data.Models;
using Hotel.Services.Options;
using Microsoft.Extensions.Options;

namespace Hotel.Services;

/// <summary>
/// Computes reservation totals from nights, guest mix, room rates, and add-ons.
/// </summary>
public class ReservationPricingCalculator
{
    private readonly ReservationPricingOptions _options;

    public ReservationPricingCalculator(IOptions<ReservationPricingOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Calculates total amount for a stay using adult/child counts and optional packages.
    /// </summary>
    public decimal CalculateTotal(
        Room room,
        int adultCount,
        int childCount,
        DateTime checkIn,
        DateTime checkOut,
        bool hasBreakfast,
        bool isAllInclusive)
    {
        var nights = (checkOut - checkIn).Days;
        if (nights <= 0)
        {
            return 0m;
        }

        var baseRate = nights * (adultCount * room.PriceAdult + childCount * room.PriceChild);
        var guests = adultCount + childCount;
        decimal extras = 0m;

        if (hasBreakfast)
        {
            extras += nights * guests * _options.BreakfastPerPersonPerNight;
        }

        if (isAllInclusive)
        {
            extras += nights * guests * _options.AllInclusivePerPersonPerNight;
        }

        return decimal.Round(baseRate + extras, 2, MidpointRounding.AwayFromZero);
    }
}
