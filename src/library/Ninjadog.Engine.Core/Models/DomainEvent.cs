namespace Ninjadog.Engine.Core.Models;

/// <summary>
/// Abstract base class for domain events, providing common properties and functionality.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets or sets the date and time when the event occurred.
    /// </summary>
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}
