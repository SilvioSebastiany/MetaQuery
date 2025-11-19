using MediatR;
using Microsoft.Extensions.Logging;
using QueryBuilder.Domain.Interfaces;
using QueryBuilder.Domain.Notifications;

namespace QueryBuilder.Domain.Queries.Handlers;

/// <summary>
/// Handler para processamento de consulta dinâmica
/// </summary>
public class ConsultaDinamicaQueryHandler : IRequestHandler<ConsultaDinamicaQuery, ConsultaDinamicaResult?>
{
    private readonly IQueryBuilderService _queryBuilder;
    private readonly IConsultaDinamicaRepository _repository;
    private readonly INotificationContext _notificationContext;
    private readonly ILogger<ConsultaDinamicaQueryHandler> _logger;

    public ConsultaDinamicaQueryHandler(
        IQueryBuilderService queryBuilder,
        IConsultaDinamicaRepository repository,
        INotificationContext notificationContext,
        ILogger<ConsultaDinamicaQueryHandler> logger)
    {
        _queryBuilder = queryBuilder;
        _repository = repository;
        _notificationContext = notificationContext;
        _logger = logger;
    }

    public async Task<ConsultaDinamicaResult?> Handle(
        ConsultaDinamicaQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando ConsultaDinamicaQuery - Tabela: {Tabela}, JOINs: {IncluirJoins}, Profundidade: {Profundidade}",
            request.Tabela, request.IncluirJoins, request.Profundidade);

        try
        {
            // Gerar query (método síncrono)
            var query = _queryBuilder.MontarQuery(
                request.Tabela,
                request.IncluirJoins,
                request.Profundidade);

            // Compilar para SQL (para retornar no resultado)
            var compiled = _queryBuilder.CompilarQuery(query);

            // Executar
            var dados = await _repository.ExecutarQueryAsync(query);
            var lista = dados.ToList();

            _logger.LogInformation(
                "Consulta executada com sucesso - {Total} registros retornados",
                lista.Count);

            return new ConsultaDinamicaResult(
                request.Tabela,
                lista.Count,
                lista,
                compiled.Sql
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar consulta dinâmica para tabela {Tabela}", request.Tabela);
            _notificationContext.AddNotification("Erro", $"Erro ao executar consulta: {ex.Message}");
            return null;
        }
    }
}
