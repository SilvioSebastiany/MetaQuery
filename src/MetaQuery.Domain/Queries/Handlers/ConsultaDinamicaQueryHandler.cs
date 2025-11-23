using MediatR;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.DomainServices;
using MetaQuery.Domain.Notifications;

namespace MetaQuery.Domain.Queries.Handlers;

/// <summary>
/// Handler para processamento de consulta dinâmica (CQRS Pattern)
/// Responsabilidade: Orquestração - delega lógica de negócio para DomainService
/// </summary>
public class ConsultaDinamicaQueryHandler : IRequestHandler<ConsultaDinamicaQuery, ConsultaDinamicaResult?>
{
    private readonly ConsultaDinamicaDomainService _domainService;
    private readonly INotificationContext _notificationContext;
    private readonly ILogger<ConsultaDinamicaQueryHandler> _logger;

    public ConsultaDinamicaQueryHandler(
        ConsultaDinamicaDomainService domainService,
        INotificationContext notificationContext,
        ILogger<ConsultaDinamicaQueryHandler> logger)
    {
        _domainService = domainService;
        _notificationContext = notificationContext;
        _logger = logger;
    }

    public async Task<ConsultaDinamicaResult?> Handle(
        ConsultaDinamicaQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Delegar lógica de negócio para DomainService
            var resultado = await _domainService.ConsultarTabelaAsync(
                request.Tabela,
                request.IncluirJoins,
                request.Profundidade);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar consulta dinâmica para tabela {Tabela}", request.Tabela);
            _notificationContext.AddNotification("Erro", $"Erro ao executar consulta: {ex.Message}");
            return null;
        }
    }
}
