using Microsoft.AspNetCore.Mvc;
using QueryBuilder.Domain.Interfaces;
using SqlKata.Compilers;

namespace QueryBuilder.Api.Controllers;

/// <summary>
/// Controller para consultas dinâmicas - Endpoint público do MVP
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConsultaDinamicaController : ControllerBase
{
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly IConsultaDinamicaRepository _consultaRepository;
    private readonly OracleCompiler _compiler;
    private readonly ILogger<ConsultaDinamicaController> _logger;

    // Lista de tabelas permitidas (whitelist de segurança)
    private static readonly HashSet<string> TabelasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "CLIENTES",
        "PEDIDOS",
        "PRODUTOS",
        "CATEGORIAS",
        "ITENS_PEDIDO",
        "ENDERECOS"
    };

    public ConsultaDinamicaController(
        IQueryBuilderService queryBuilderService,
        IConsultaDinamicaRepository consultaRepository,
        OracleCompiler compiler,
        ILogger<ConsultaDinamicaController> logger)
    {
        _queryBuilderService = queryBuilderService;
        _consultaRepository = consultaRepository;
        _compiler = compiler;
        _logger = logger;
    }

    /// <summary>
    /// Consulta dados de uma tabela dinamicamente
    /// </summary>
    /// <param name="tabela">Nome da tabela a consultar</param>
    /// <param name="incluirJoins">Se true, inclui JOINs com tabelas relacionadas</param>
    /// <param name="profundidade">Profundidade máxima de JOINs (1-3, padrão: 2)</param>
    /// <response code="200">Dados retornados com sucesso</response>
    /// <response code="400">Tabela não autorizada ou parâmetros inválidos</response>
    /// <response code="404">Tabela não encontrada nos metadados</response>
    /// <response code="500">Erro ao executar consulta</response>
    [HttpGet("{tabela}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Consultar( string tabela,
        [FromQuery] bool incluirJoins = false,
        [FromQuery] int profundidade = 2)
    {
        try
        {
            // Validação: tabela permitida
            if (!TabelasPermitidas.Contains(tabela))
            {
                _logger.LogWarning("Tentativa de acesso à tabela não autorizada: {Tabela}", tabela);
                return BadRequest(new
                {
                    Erro = "Tabela não autorizada",
                    TabelasDisponiveis = TabelasPermitidas.OrderBy(t => t).ToList()
                });
            }

            // Validação: profundidade
            if (profundidade < 1 || profundidade > 3)
            {
                return BadRequest(new { Erro = "Profundidade deve estar entre 1 e 3" });
            }

            _logger.LogInformation(
                "Consultando tabela: {Tabela}, JOINs: {IncluirJoins}, Profundidade: {Profundidade}",
                tabela, incluirJoins, profundidade);

            // 1. Gerar query dinâmica
            var query = _queryBuilderService.MontarQuery(tabela, incluirJoins, profundidade);

            // 2. Executar no banco
            var dados = await _consultaRepository.ExecutarQueryAsync(query);
            var lista = dados.ToList();

            // 3. Obter total (útil para paginação futura)
            var total = lista.Count;

            // 4. Compilar SQL para debug (opcional)
            var sqlGerado = _compiler.Compile(query).Sql;

            return Ok(new
            {
                Tabela = tabela.ToUpper(),
                IncluiJoins = incluirJoins,
                Profundidade = profundidade,
                Total = total,
                Dados = lista,
                Debug = new
                {
                    SqlGerado = sqlGerado
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Tabela não encontrada: {Tabela}", tabela);
            return NotFound(new { Erro = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro ao executar consulta para tabela: {Tabela}", tabela);
            return StatusCode(500, new { Erro = "Erro ao executar consulta", Detalhes = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao consultar tabela: {Tabela}", tabela);
            return StatusCode(500, new { Erro = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Consulta com filtros dinâmicos (WHERE)
    /// </summary>
    /// <param name="tabela">Nome da tabela</param>
    /// <param name="filtros">Dicionário de filtros (campo: valor)</param>
    /// <param name="incluirJoins">Se true, inclui JOINs</param>
    /// <response code="200">Dados filtrados retornados com sucesso</response>
    [HttpPost("{tabela}/filtrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarComFiltros(
        string tabela,
        [FromBody] Dictionary<string, object> filtros,
        [FromQuery] bool incluirJoins = false)
    {
        try
        {
            // Validação: tabela permitida
            if (!TabelasPermitidas.Contains(tabela))
            {
                return BadRequest(new { Erro = "Tabela não autorizada" });
            }

            _logger.LogInformation(
                "Consultando tabela {Tabela} com {TotalFiltros} filtros",
                tabela, filtros.Count);

            // Converter JsonElement para tipos nativos
            var filtrosConvertidos = ConverterFiltros(filtros);

            // 1. Gerar query com filtros
            var query = _queryBuilderService.MontarQueryComFiltros(tabela, filtrosConvertidos, incluirJoins);

            // 2. Executar no banco
            var dados = await _consultaRepository.ExecutarQueryAsync(query);
            var lista = dados.ToList();

            return Ok(new
            {
                Tabela = tabela.ToUpper(),
                Filtros = filtros,
                Total = lista.Count,
                Dados = lista
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Tabela não encontrada: {Tabela}", tabela);
            return NotFound(new { Erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar consulta com filtros");
            return StatusCode(500, new { Erro = "Erro ao executar consulta", Detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Consulta com paginação
    /// </summary>
    /// <param name="tabela">Nome da tabela</param>
    /// <param name="pagina">Número da página (começa em 1)</param>
    /// <param name="itensPorPagina">Quantidade de itens por página</param>
    /// <param name="incluirJoins">Se true, inclui JOINs</param>
    [HttpGet("{tabela}/paginado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarPaginado(
        string tabela,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] bool incluirJoins = false)
    {
        try
        {
            // Validações
            if (!TabelasPermitidas.Contains(tabela))
            {
                return BadRequest(new { Erro = "Tabela não autorizada" });
            }

            if (pagina < 1)
            {
                return BadRequest(new { Erro = "Página deve ser maior que 0" });
            }

            if (itensPorPagina < 1 || itensPorPagina > 100)
            {
                return BadRequest(new { Erro = "Itens por página deve estar entre 1 e 100" });
            }

            _logger.LogInformation(
                "Consultando tabela {Tabela} - Página {Pagina}, Itens: {ItensPorPagina}",
                tabela, pagina, itensPorPagina);

            // 1. Obter total de registros (sem paginação)
            var queryCount = _queryBuilderService.MontarQuery(tabela, false);
            var total = await _consultaRepository.ExecutarQueryCountAsync(queryCount);

            // 2. Gerar query com paginação
            var query = _queryBuilderService.MontarQueryComPaginacao(
                tabela,
                pagina,
                itensPorPagina,
                incluirJoins);

            // 3. Executar no banco
            var dados = await _consultaRepository.ExecutarQueryAsync(query);

            var totalPaginas = (int)Math.Ceiling((double)total / itensPorPagina);

            return Ok(new
            {
                Tabela = tabela.ToUpper(),
                Paginacao = new
                {
                    PaginaAtual = pagina,
                    ItensPorPagina = itensPorPagina,
                    TotalItens = total,
                    TotalPaginas = totalPaginas,
                    TemProxima = pagina < totalPaginas,
                    TemAnterior = pagina > 1
                },
                Dados = dados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar consulta paginada");
            return StatusCode(500, new { Erro = "Erro ao executar consulta", Detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Lista todas as tabelas disponíveis para consulta
    /// </summary>
    [HttpGet("tabelas-disponiveis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ListarTabelasDisponiveis()
    {
        var tabelas = TabelasPermitidas.OrderBy(t => t).ToList();

        return Ok(new
        {
            Total = tabelas.Count,
            Tabelas = tabelas,
            Observacao = "Use GET /api/ConsultaDinamica/{tabela} para consultar"
        });
    }

    /// <summary>
    /// Converte valores do dicionário de filtros de JsonElement para tipos nativos
    /// </summary>
    private Dictionary<string, object> ConverterFiltros(Dictionary<string, object> filtros)
    {
        var resultado = new Dictionary<string, object>();

        foreach (var (chave, valor) in filtros)
        {
            if (valor is System.Text.Json.JsonElement jsonElement)
            {
                resultado[chave] = ConverterJsonElement(jsonElement);
            }
            else
            {
                resultado[chave] = valor;
            }
        }

        return resultado;
    }

    /// <summary>
    /// Converte JsonElement para tipo nativo apropriado
    /// </summary>
    private object ConverterJsonElement(System.Text.Json.JsonElement element)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => element.GetString()!,
            System.Text.Json.JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            System.Text.Json.JsonValueKind.True => true,
            System.Text.Json.JsonValueKind.False => false,
            System.Text.Json.JsonValueKind.Null => null!,
            _ => element.ToString()!
        };
    }
}
