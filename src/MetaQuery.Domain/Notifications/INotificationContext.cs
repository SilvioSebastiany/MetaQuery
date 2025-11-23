namespace MetaQuery.Domain.Notifications;

/// <summary>
/// Interface para gerenciamento de notificações (padrão Notification Pattern)
/// Substitui exceptions para validações e erros de negócio
/// </summary>
public interface INotificationContext
{
    /// <summary>
    /// Adiciona uma notificação simples
    /// </summary>
    void AddNotification(string key, string message);

    /// <summary>
    /// Adiciona múltiplas notificações de uma vez
    /// </summary>
    void AddNotifications(IEnumerable<Notification> notifications);

    /// <summary>
    /// Verifica se existem notificações
    /// </summary>
    bool HasNotifications { get; }

    /// <summary>
    /// Obtém todas as notificações registradas
    /// </summary>
    IReadOnlyCollection<Notification> Notifications { get; }

    /// <summary>
    /// Limpa todas as notificações
    /// </summary>
    void Clear();
}
