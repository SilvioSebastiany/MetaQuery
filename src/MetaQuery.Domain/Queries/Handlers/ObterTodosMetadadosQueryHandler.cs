using MediatR;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.DomainServices;
using MetaQuery.Domain.Notifications;
using MetaQuery.Domain.Queries.Metadados;

namespace MetaQuery.Domain.Queries.Handlers;

/// <summary>
/// Handler para ObterTodosMetadadosQuery
/// Responsabilidade: Orquestração - delega para DomainService
/// </summary>
public class ObterTodosMetadadosQueryHandler : IRequestHandler<ObterTodosMetadadosQuery, ObterTodosMetadadosResult?>
{
    private readonly MetadadosDomainService _domainService;
    private readonly INotificationContext _notificationContext;
    private readonly ILogger<ObterTodosMetadadosQueryHandler> _logger;

    public ObterTodosMetadadosQueryHandler(
        MetadadosDomainService domainService,
        INotificationContext notificationContext,
        ILogger<ObterTodosMetadadosQueryHandler> logger)
    {
        _domainService = domainService;
        _notificationContext = notificationContext;
        _logger = logger;
    }

    public async Task<ObterTodosMetadadosResult?> Handle(
        ObterTodosMetadadosQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadados = await _domainService.ObterTodosAsync(request.ApenasAtivos);

            return new ObterTodosMetadadosResult(
                Total: metadados.Count(),
                Metadados: metadados
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os metadados");
            _notificationContext.AddNotification("Erro", $"Erro ao obter metadados: {ex.Message}");
            return null;
        }
    }
}
