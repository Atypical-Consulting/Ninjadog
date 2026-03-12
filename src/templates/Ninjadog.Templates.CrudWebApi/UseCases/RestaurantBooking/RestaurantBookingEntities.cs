namespace Ninjadog.Templates.CrudWebAPI.UseCases.RestaurantBooking;

/// <summary>
/// Defines the entities specific to the RestaurantBooking use case.
/// </summary>
public sealed class RestaurantBookingEntities : NinjadogEntities
{
    // https://hevodata.com/learn/schema-example/#Schema_Example_Restaurant_Booking
    private const string Customer = "Customer";
    private const string Booking = "Booking";
    private const string Table = "Table";
    private const string Order = "Order";
    private const string OrderMenuItem = "OrderMenuItem";
    private const string Menu = "Menu";
    private const string MenuItem = "MenuItem";
    private const string MenuItemIngredient = "MenuItemIngredient";
    private const string Ingredient = "Ingredient";
    private const string IngredientType = "IngredientType";
    private const string Staff = "Staff";
    private const string StaffRole = "StaffRole";

    /// <summary>
    /// Initializes a new instance of the <see cref="RestaurantBookingEntities"/> class.
    /// </summary>
    public RestaurantBookingEntities()
    {
        AddCustomerEntity();
        AddBookingEntity();
        AddTableEntity();
        AddOrderEntity();
        AddOrderMenuItemEntity();
        AddMenuEntity();
        AddMenuItemEntity();
        AddMenuItemIngredientEntity();
        AddIngredientEntity();
        AddIngredientTypeEntity();
        AddStaffEntity();
        AddStaffRoleEntity();
    }

    private void AddCustomerEntity()
    {
        NinjadogEntityProperties properties = new()
        {
            { "CustomerId", new NinjadogEntityId() },
            { "CustomerFirstName", new NinjadogEntityProperty<string>() },
            { "CustomerSurname", new NinjadogEntityProperty<string>() },
            { "PhoneNumber", new NinjadogEntityProperty<string>() },
            { "CellphoneNumber", new NinjadogEntityProperty<string>() },
            { "EmailAddress", new NinjadogEntityProperty<string>() },
            { "OtherCustomerDetails", new NinjadogEntityProperty<string>() },
        };

        NinjadogEntityRelationships relationships = new()
        {
            { "Bookings", new NinjadogEntityRelationship(Booking, NinjadogEntityRelationshipType.OneToMany) }
        };

        Add(Customer, new NinjadogEntity(properties, relationships));
    }

    private void AddBookingEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "BookingId", new NinjadogEntityId() },
            { "DateOfBooking", new NinjadogEntityProperty<DateTime>() },
            { "NumberInParty", new NinjadogEntityProperty<int>() },
        };

        Add(Booking, new NinjadogEntity(properties));
    }

    private void AddTableEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "TableNumber", new NinjadogEntityProperty<int>(true) },
            { "TableDetails", new NinjadogEntityProperty<string>() },
        };

        Add(Table, new NinjadogEntity(properties));
    }

    private void AddOrderEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "OrderId", new NinjadogEntityId() }, { "OrderDateTime", new NinjadogEntityProperty<DateTime>() },
        };

        Add(Order, new NinjadogEntity(properties));
    }

    private void AddOrderMenuItemEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "OrderMenuItemId", new NinjadogEntityId() },
            { "OrderMenuItemQuantity", new NinjadogEntityProperty<int>() },
            { "OrderMenuItemComments", new NinjadogEntityProperty<string>() },
        };

        Add(OrderMenuItem, new NinjadogEntity(properties));
    }

    private void AddMenuEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "MenuId", new NinjadogEntityId() }, { "MenuDate", new NinjadogEntityProperty<DateTime>() },
        };

        Add(Menu, new NinjadogEntity(properties));
    }

    private void AddMenuItemEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "MenuItemId", new NinjadogEntityId() },
            { "MenuItemDescription", new NinjadogEntityProperty<string>() },
            { "MenuItemPrice", new NinjadogEntityProperty<decimal>() },
        };

        Add(MenuItem, new NinjadogEntity(properties));
    }

    private void AddMenuItemIngredientEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "MenuItemIngredientId", new NinjadogEntityId() },
            { "ItemQuantity", new NinjadogEntityProperty<int>() },
        };

        Add(MenuItemIngredient, new NinjadogEntity(properties));
    }

    private void AddIngredientEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "IngredientId", new NinjadogEntityId() }, { "IngredientName", new NinjadogEntityProperty<string>() },
        };

        Add(Ingredient, new NinjadogEntity(properties));
    }

    private void AddIngredientTypeEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "IngredientTypeCode", new NinjadogEntityId() },
            { "IngredientTypeDescription", new NinjadogEntityProperty<string>() },
        };

        Add(IngredientType, new NinjadogEntity(properties));
    }

    private void AddStaffEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "StaffId", new NinjadogEntityId() },
            { "StaffFirstName", new NinjadogEntityProperty<string>() },
            { "StaffLastName", new NinjadogEntityProperty<string>() },
        };

        Add(Staff, new NinjadogEntity(properties));
    }

    private void AddStaffRoleEntity()
    {
        var properties = new NinjadogEntityProperties
        {
            { "StaffRoleCode", new NinjadogEntityId() },
            { "StaffRoleDescription", new NinjadogEntityProperty<string>() },
        };

        Add(StaffRole, new NinjadogEntity(properties));
    }
}
