using Microsoft.Extensions.Logging;
using MetaQuery.Domain.Interfaces;
using MetaQuery.Domain.Notifications;
using MetaQuery.Domain.Services;

namespace MetaQuery.Domain.DomainServices;

/// <summary>
/// Domain Service responsável pela lógica de negócio de consultas dinâmicas
/// </summary>
public class ConsultaDinamicaDomainService
{
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly IConsultaDinamicaRepository _consultaDinamicaRepository;
    private readonly ILogger<ConsultaDinamicaDomainService> _logger;
    private readonly HierarchicalGrouper _hierarchicalGrouper;

    public ConsultaDinamicaDomainService(
        IQueryBuilderService queryBuilderService,
        IConsultaDinamicaRepository consultaDinamicaRepository,
        ILogger<ConsultaDinamicaDomainService> logger,
        HierarchicalGrouper hierarchicalGrouper)
    {
        _queryBuilderService = queryBuilderService;
        _consultaDinamicaRepository = consultaDinamicaRepository;
        _logger = logger;
        _hierarchicalGrouper = hierarchicalGrouper;
    }

    /// <summary>
    /// Executa consulta dinâmica em uma tabela com lógica de negócio aplicada
    /// </summary>
    public async Task<ConsultaDinamicaResult> ConsultarTabelaAsync(
        string tabela,
        bool incluirJoins,
        int profundidade,
        bool formatoHierarquico = false)
    {
        _logger.LogInformation(
            "Consultando tabela {Tabela} com joins={IncluirJoins}, profundidade={Profundidade}, hierarquico={Hierarquico}",
            tabela, incluirJoins, profundidade, formatoHierarquico);

        var sqlQuery = _queryBuilderService.MontarQuery(tabela, incluirJoins, profundidade);

        var compiledQuery = _queryBuilderService.CompilarQuery(sqlQuery);
        _logger.LogDebug("SQL gerado: {Sql}", compiledQuery.Sql);

        var dados = await _consultaDinamicaRepository.ExecutarQueryAsync(sqlQuery);

        // Agrupar hierarquicamente se solicitado
        if (formatoHierarquico && incluirJoins && dados.Any())
        {
            _logger.LogDebug("Agrupando resultados hierarquicamente para tabela {Tabela}", tabela);
            dados = await _hierarchicalGrouper.AgruparHierarquicamenteAsync(dados, tabela);
        }

        var totalRegistros = dados.Count();

        if (totalRegistros > 5000)
        {
            _logger.LogWarning(
                "Consulta à tabela {Tabela} retornou {Total} registros (acima do recomendado)",
                tabela, totalRegistros);
        }

        return new ConsultaDinamicaResult(tabela, totalRegistros, dados, compiledQuery.Sql);
    }
}
