namespace QueryBuilder.Domain.Notifications;

/// <summary>
/// Implementação do contexto de notificações
/// Mantém lista de erros/validações durante o processamento de uma request
/// </summary>
public class NotificationContext : INotificationContext
{
    private readonly List<Notification> _notifications = new();

    public void AddNotification(string key, string message)
    {
        _notifications.Add(new Notification(key, message));
    }

    public void AddNotifications(IEnumerable<Notification> notifications)
    {
        _notifications.AddRange(notifications);
    }

    public bool HasNotifications => _notifications.Any();

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    public void Clear()
    {
        _notifications.Clear();
    }
}
