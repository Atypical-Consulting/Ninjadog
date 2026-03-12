namespace Ninjadog.Templates.CrudWebAPI.UseCases.RestaurantBooking;

/// <summary>
/// Represents the settings for the RestaurantBooking use case.
/// </summary>
public record RestaurantBookingSettings()
    : NinjadogSettings(
        new RestaurantBookingConfiguration(),
        new RestaurantBookingEntities());
