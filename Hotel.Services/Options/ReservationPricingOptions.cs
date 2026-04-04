namespace Hotel.Services.Options;

/// <summary>
/// Configurable nightly add-ons applied on top of base room rates.
/// </summary>
public class ReservationPricingOptions
{
    public const string SectionName = "ReservationPricing";

    /// <summary>Flat add-on per guest per night when breakfast is selected.</summary>
    public decimal BreakfastPerPersonPerNight { get; set; } = 15m;

    /// <summary>Flat add-on per guest per night for all-inclusive.</summary>
    public decimal AllInclusivePerPersonPerNight { get; set; } = 45m;
}
