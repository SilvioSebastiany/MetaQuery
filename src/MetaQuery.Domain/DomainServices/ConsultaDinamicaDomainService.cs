using Microsoft.Extensions.Logging;
using MetaQuery.Domain.Interfaces;
using MetaQuery.Domain.Notifications;

namespace MetaQuery.Domain.DomainServices;

/// <summary>
/// Domain Service responsável pela lógica de negócio de consultas dinâmicas
/// </summary>
public class ConsultaDinamicaDomainService
{
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly IConsultaDinamicaRepository _consultaDinamicaRepository;
    private readonly ILogger<ConsultaDinamicaDomainService> _logger;

    public ConsultaDinamicaDomainService(
        IQueryBuilderService queryBuilderService,
        IConsultaDinamicaRepository consultaDinamicaRepository,
        ILogger<ConsultaDinamicaDomainService> logger)
    {
        _queryBuilderService = queryBuilderService;
        _consultaDinamicaRepository = consultaDinamicaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executa consulta dinâmica em uma tabela com lógica de negócio aplicada
    /// </summary>
    public async Task<ConsultaDinamicaResult> ConsultarTabelaAsync(string tabela, bool incluirJoins, int profundidade)
    {
        _logger.LogInformation(
            "Consultando tabela {Tabela} com joins={IncluirJoins}, profundidade={Profundidade}",
            tabela, incluirJoins, profundidade);

        // 1. Montar query SQL usando QueryBuilderService
        var sqlQuery = _queryBuilderService.MontarQuery(tabela, incluirJoins, profundidade);

        // 2. Compilar query para obter SQL gerado
        var compiledQuery = _queryBuilderService.CompilarQuery(sqlQuery);
        _logger.LogDebug("SQL gerado: {Sql}", compiledQuery.Sql);

        var dados = await _consultaDinamicaRepository.ExecutarQueryAsync(sqlQuery);

        var totalRegistros = dados.Count();

        if (totalRegistros > 5000)
        {
            _logger.LogWarning(
                "Consulta à tabela {Tabela} retornou {Total} registros (acima do recomendado)",
                tabela, totalRegistros);
        }

        // 5. Retornar resultado estruturado
        return new ConsultaDinamicaResult(tabela, totalRegistros, dados, compiledQuery.Sql);
    }

    /// <summary>
    /// Lista todas as tabelas disponíveis para consulta (whitelist)
    /// </summary>
    public Task<IEnumerable<string>> ListarTabelasDisponiveisAsync()
    {
        // Regra de negócio: Whitelist de tabelas permitidas
        var tabelasPermitidas = new[]
        {
            "CLIENTES",
            "PEDIDOS",
            "PRODUTOS",
            "CATEGORIAS",
            "ITENS_PEDIDO",
            "ENDERECOS"
        };

        _logger.LogInformation("Listando {Total} tabelas disponíveis", tabelasPermitidas.Length);

        return Task.FromResult<IEnumerable<string>>(tabelasPermitidas.OrderBy(t => t));
    }
}
