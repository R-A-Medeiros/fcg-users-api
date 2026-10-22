namespace FCG.Contracts.Events;

// Published by UsersAPI when a new user is registered.
// Consumed by NotificationsAPI to send a welcome e-mail.
// MassTransit matches messages by the full type name (namespace + type), so this
// contract keeps the same namespace/shape used by the other services.
public record UserCreatedEvent
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
