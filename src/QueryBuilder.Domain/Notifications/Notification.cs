namespace QueryBuilder.Domain.Notifications;

/// <summary>
/// Representa uma notificação de validação ou erro de negócio
/// </summary>
public record Notification(string Key, string Message);
