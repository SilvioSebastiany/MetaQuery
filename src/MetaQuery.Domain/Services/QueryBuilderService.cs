using System.Text;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.Entities;
using MetaQuery.Domain.Interfaces;
using SqlKata;
using SqlKata.Compilers;

namespace MetaQuery.Domain.Services;

/// <summary>
/// Serviço para construção dinâmica de queries SQL baseadas em metadados
/// </summary>
public class QueryBuilderService : IQueryBuilderService
{
    private readonly IMetadadosRepository _metadadosRepository;
    private readonly OracleCompiler _compiler;
    private readonly ILogger<QueryBuilderService> _logger;
    private List<TabelaDinamica>? _metadadosCache;

    public QueryBuilderService(
        IMetadadosRepository metadadosRepository,
        ILogger<QueryBuilderService> logger)
    {
        _metadadosRepository = metadadosRepository;
        _compiler = new OracleCompiler();
        _logger = logger;
    }

    /// <summary>
    /// Carrega metadados em cache
    /// </summary>
    public async Task CarregarMetadadosAsync()
    {
        _logger.LogInformation("Carregando metadados em cache...");
        _metadadosCache = (await _metadadosRepository.ObterTodosAsync()).ToList();
        _logger.LogInformation("{Total} metadados carregados", _metadadosCache.Count);
    }

    /// <summary>
    /// Monta uma query SELECT dinâmica (versão síncrona da interface)
    /// </summary>
    public Query MontarQuery(string tabelaBase, bool incluirJoins = true, int profundidadeMaxima = 3)
    {
        return MontarQueryAsync(tabelaBase, incluirJoins, profundidadeMaxima).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Monta query com filtros dinâmicos
    /// </summary>
    public Query MontarQueryComFiltros(string tabelaBase, Dictionary<string, object> filtros, bool incluirJoins = true)
    {
        return MontarQueryComFiltrosAsync(tabelaBase, filtros, incluirJoins).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Monta query com ordenação
    /// </summary>
    public Query MontarQueryComOrdenacao(string tabelaBase, string campoOrdenacao, bool descendente = false, bool incluirJoins = true)
    {
        var query = MontarQuery(tabelaBase, incluirJoins);

        if (descendente)
            query.OrderByDesc(campoOrdenacao);
        else
            query.OrderBy(campoOrdenacao);

        return query;
    }

    /// <summary>
    /// Monta query com paginação
    /// </summary>
    public Query MontarQueryComPaginacao(string tabelaBase, int pagina, int itensPorPagina, bool incluirJoins = true)
    {
        var query = MontarQuery(tabelaBase, incluirJoins);
        var offset = (pagina - 1) * itensPorPagina;

        query.Offset(offset).Limit(itensPorPagina);

        return query;
    }

    /// <summary>
    /// Compila query para SQL
    /// </summary>
    public SqlResult CompilarQuery(Query query)
    {
        return _compiler.Compile(query);
    }

    /// <summary>
    /// Lista todas as tabelas disponíveis
    /// </summary>
    public List<string> ListarTabelas()
    {
        if (_metadadosCache == null || !_metadadosCache.Any())
        {
            _metadadosCache = _metadadosRepository.ObterTodosAsync().GetAwaiter().GetResult().ToList();
        }

        return _metadadosCache.Select(m => m.Tabela).ToList();
    }

    /// <summary>
    /// Verifica se tabela existe nos metadados
    /// </summary>
    public bool TabelaExiste(string tabela)
    {
        return _metadadosRepository.ExisteAsync(tabela).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Obtém grafo de relacionamentos em formato texto
    /// </summary>
    public string ObterGrafoRelacionamentos(string tabelaBase, int profundidade = 3)
    {
        var metadados = _metadadosRepository.ObterPorNomeTabelaAsync(tabelaBase).GetAwaiter().GetResult();
        if (metadados == null)
            return string.Empty;

        var sb = new StringBuilder();
        var visitadas = new HashSet<string>();

        MontarGrafoRecursivo(sb, metadados, 0, profundidade, visitadas);

        return sb.ToString();
    }

    #region Métodos Async Principais

    private async Task<Query> MontarQueryAsync(
        string nomeTabela,
        bool incluirJoins = false,
        int profundidadeMaxima = 2)
    {
        _logger.LogInformation(
            "Montando query para tabela: {Tabela}, Joins: {IncluirJoins}, Profundidade: {Profundidade}",
            nomeTabela, incluirJoins, profundidadeMaxima);

        var metadados = await _metadadosRepository.ObterPorNomeTabelaAsync(nomeTabela);

        if (metadados == null)
        {
            throw new ArgumentException($"Tabela '{nomeTabela}' não encontrada nos metadados");
        }

        var campos = ParseCampos(metadados.CamposDisponiveis);
        var query = new Query(metadados.Tabela);

        foreach (var campo in campos)
        {
            query.Select($"{metadados.Tabela}.{campo}");
        }

        if (incluirJoins && !string.IsNullOrWhiteSpace(metadados.VinculoEntreTabela))
        {
            var tabelasProcessadas = new HashSet<string> { metadados.Tabela.ToUpper() };
            await AdicionarJoinsRecursivosAsync(query, metadados, tabelasProcessadas, 1, profundidadeMaxima);
        }

        _logger.LogInformation("Query montada com sucesso para tabela: {Tabela}", nomeTabela);
        return query;
    }

    private async Task<Query> MontarQueryComFiltrosAsync(
        string nomeTabela,
        Dictionary<string, object> filtros,
        bool incluirJoins = false)
    {
        _logger.LogInformation(
            "Montando query com filtros para tabela: {Tabela}, Filtros: {Filtros}",
            nomeTabela, filtros.Count);

        var query = await MontarQueryAsync(nomeTabela, incluirJoins);
        var metadados = await _metadadosRepository.ObterPorNomeTabelaAsync(nomeTabela);
        var camposValidos = ParseCampos(metadados!.CamposDisponiveis);

        foreach (var filtro in filtros)
        {
            var nomeCampo = filtro.Key.ToUpper();

            if (!camposValidos.Contains(nomeCampo))
            {
                _logger.LogWarning("Campo '{Campo}' não existe na tabela '{Tabela}'. Filtro ignorado.", nomeCampo, nomeTabela);
                continue;
            }

            query.Where($"{metadados.Tabela}.{nomeCampo}", filtro.Value);
        }

        _logger.LogInformation("Query com filtros montada com sucesso");
        return query;
    }

    #endregion

    #region Métodos Auxiliares Privados

    private static List<string> ParseCampos(string camposDisponiveis)
    {
        if (string.IsNullOrWhiteSpace(camposDisponiveis))
            return new List<string>();

        return camposDisponiveis
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToUpper())
            .ToList();
    }

    private List<VinculoTabela> ParseVinculos(string vinculo)
    {
        var vinculos = new List<VinculoTabela>();

        if (string.IsNullOrWhiteSpace(vinculo))
            return vinculos;

        foreach (var v in vinculo.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var partes = v.Split(':', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 3)
            {
                vinculos.Add(new VinculoTabela
                {
                    TabelaDestino = partes[0].Trim().ToUpper(),
                    CampoFK = partes[1].Trim().ToUpper(),
                    CampoPK = partes[2].Trim().ToUpper()
                });
            }
            else
            {
                _logger.LogWarning("Vínculo malformado ignorado: {Vinculo}", v);
            }
        }

        return vinculos;
    }

    private async Task AdicionarJoinsRecursivosAsync(
        Query query,
        TabelaDinamica metadados,
        HashSet<string> tabelasProcessadas,
        int profundidadeAtual,
        int profundidadeMaxima)
    {
        if (profundidadeAtual > profundidadeMaxima)
        {
            _logger.LogDebug("Limite de profundidade atingido: {Profundidade}", profundidadeMaxima);
            return;
        }

        var vinculos = ParseVinculos(metadados.VinculoEntreTabela ?? "");

        foreach (var vinculo in vinculos)
        {
            if (tabelasProcessadas.Contains(vinculo.TabelaDestino))
            {
                _logger.LogDebug("Tabela {Tabela} já processada. Pulando para evitar loop.", vinculo.TabelaDestino);
                continue;
            }

            var metadadosDestino = await _metadadosRepository.ObterPorNomeTabelaAsync(vinculo.TabelaDestino);

            if (metadadosDestino == null)
            {
                _logger.LogWarning("Metadados não encontrados para tabela: {Tabela}", vinculo.TabelaDestino);
                continue;
            }

            query.LeftJoin(
                vinculo.TabelaDestino,
                $"{metadados.Tabela}.{vinculo.CampoPK}",
                $"{vinculo.TabelaDestino}.{vinculo.CampoFK}"
            );

            _logger.LogDebug(
                "JOIN adicionado: {TabelaOrigem}.{CampoPK} = {TabelaDestino}.{CampoFK} (Profundidade: {Profundidade})",
                metadados.Tabela, vinculo.CampoPK, vinculo.TabelaDestino, vinculo.CampoFK, profundidadeAtual);

            var camposDestino = ParseCampos(metadadosDestino.CamposDisponiveis);
            foreach (var campo in camposDestino)
            {
                query.Select($"{vinculo.TabelaDestino}.{campo} AS {vinculo.TabelaDestino}_{campo}");
            }

            tabelasProcessadas.Add(vinculo.TabelaDestino);

            if (!string.IsNullOrWhiteSpace(metadadosDestino.VinculoEntreTabela))
            {
                await AdicionarJoinsRecursivosAsync(
                    query,
                    metadadosDestino,
                    tabelasProcessadas,
                    profundidadeAtual + 1,
                    profundidadeMaxima);
            }
        }
    }

    private void MontarGrafoRecursivo(
        StringBuilder sb,
        TabelaDinamica metadados,
        int nivel,
        int profundidadeMaxima,
        HashSet<string> visitadas)
    {
        if (nivel > profundidadeMaxima || visitadas.Contains(metadados.Tabela))
            return;

        var indent = new string(' ', nivel * 2);
        sb.AppendLine($"{indent}- {metadados.Tabela}");
        visitadas.Add(metadados.Tabela);

        var vinculos = ParseVinculos(metadados.VinculoEntreTabela ?? "");
        foreach (var vinculo in vinculos)
        {
            var metadadosFilho = _metadadosRepository.ObterPorNomeTabelaAsync(vinculo.TabelaDestino).GetAwaiter().GetResult();
            if (metadadosFilho != null)
            {
                MontarGrafoRecursivo(sb, metadadosFilho, nivel + 1, profundidadeMaxima, visitadas);
            }
        }
    }

    #endregion

    #region Classes Internas

    private sealed class VinculoTabela
    {
        public string TabelaDestino { get; set; } = string.Empty;
        public string CampoFK { get; set; } = string.Empty;
        public string CampoPK { get; set; } = string.Empty;
    }

    #endregion
}
