namespace MetaQuery.Domain.Notifications;

/// <summary>
/// Interface para gerenciamento de notificações (padrão Notification Pattern)
/// Substitui exceptions para validações e erros de negócio
/// </summary>
public interface INotificationContext
{
    void AddNotification(string key, string message);
    void AddNotifications(IEnumerable<Notification> notifications);
    bool HasNotifications { get; }
    IReadOnlyCollection<Notification> Notifications { get; }
    void Clear();
}
