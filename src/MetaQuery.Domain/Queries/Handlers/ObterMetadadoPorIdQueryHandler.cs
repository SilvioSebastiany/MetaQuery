using MediatR;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.DomainServices;
using MetaQuery.Domain.Entities;
using MetaQuery.Domain.Notifications;
using MetaQuery.Domain.Queries.Metadados;

namespace MetaQuery.Domain.Queries.Handlers;

/// <summary>
/// Handler para ObterMetadadoPorIdQuery
/// Responsabilidade: Orquestração - delega para DomainService
/// </summary>
public class ObterMetadadoPorIdQueryHandler : IRequestHandler<ObterMetadadoPorIdQuery, TabelaDinamica?>
{
    private readonly MetadadosDomainService _domainService;
    private readonly INotificationContext _notificationContext;
    private readonly ILogger<ObterMetadadoPorIdQueryHandler> _logger;

    public ObterMetadadoPorIdQueryHandler(
        MetadadosDomainService domainService,
        INotificationContext notificationContext,
        ILogger<ObterMetadadoPorIdQueryHandler> logger)
    {
        _domainService = domainService;
        _notificationContext = notificationContext;
        _logger = logger;
    }

    public async Task<TabelaDinamica?> Handle(
        ObterMetadadoPorIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadado = await _domainService.ObterPorIdAsync(request.Id);

            if (metadado == null)
            {
                _notificationContext.AddNotification("NotFound", $"Metadado com ID {request.Id} não encontrado");
            }

            return metadado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter metadado por ID {Id}", request.Id);
            _notificationContext.AddNotification("Erro", $"Erro ao obter metadado: {ex.Message}");
            return null;
        }
    }
}
