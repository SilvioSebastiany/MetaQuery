using MediatR;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.DomainServices;
using MetaQuery.Domain.Entities;
using MetaQuery.Domain.Notifications;
using MetaQuery.Domain.Queries.Metadados;

namespace MetaQuery.Domain.Queries.Handlers;

/// <summary>
/// Handler para ObterMetadadoPorTabelaQuery
/// Responsabilidade: Orquestração - delega para DomainService
/// </summary>
public class ObterMetadadoPorTabelaQueryHandler : IRequestHandler<ObterMetadadoPorTabelaQuery, TabelaDinamica?>
{
    private readonly MetadadosDomainService _domainService;
    private readonly INotificationContext _notificationContext;
    private readonly ILogger<ObterMetadadoPorTabelaQueryHandler> _logger;

    public ObterMetadadoPorTabelaQueryHandler(
        MetadadosDomainService domainService,
        INotificationContext notificationContext,
        ILogger<ObterMetadadoPorTabelaQueryHandler> logger)
    {
        _domainService = domainService;
        _notificationContext = notificationContext;
        _logger = logger;
    }

    public async Task<TabelaDinamica?> Handle(
        ObterMetadadoPorTabelaQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadado = await _domainService.ObterPorTabelaAsync(request.NomeTabela);

            if (metadado == null)
            {
                _notificationContext.AddNotification("NotFound", $"Metadado para tabela '{request.NomeTabela}' não encontrado");
            }

            return metadado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter metadado por tabela {NomeTabela}", request.NomeTabela);
            _notificationContext.AddNotification("Erro", $"Erro ao obter metadado: {ex.Message}");
            return null;
        }
    }
}
