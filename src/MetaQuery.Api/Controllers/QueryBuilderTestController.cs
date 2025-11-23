using Microsoft.AspNetCore.Mvc;
using MetaQuery.Domain.Interfaces;
using SqlKata.Compilers;

namespace MetaQuery.Api.Controllers;

/// <summary>
/// Controller para testes do QueryBuilder
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QueryBuilderTestController : ControllerBase
{
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly OracleCompiler _compiler;

    public QueryBuilderTestController(
        IQueryBuilderService queryBuilderService,
        OracleCompiler compiler)
    {
        _queryBuilderService = queryBuilderService;
        _compiler = compiler;
    }

    /// <summary>
    /// Testa geração de query simples (sem JOINs)
    /// </summary>
    [HttpGet("simples/{tabela}")]
    public IActionResult GerarQuerySimples(string tabela)
    {
        var query = _queryBuilderService.MontarQuery(tabela, incluirJoins: false);
        var compiled = _compiler.Compile(query);
        return Ok(new { Tabela = tabela, SQL = compiled.Sql, Parametros = compiled.NamedBindings });
    }

    /// <summary>
    /// Testa geração de query com JOINs
    /// </summary>
    [HttpGet("com-joins/{tabela}")]
    public IActionResult GerarQueryComJoins(string tabela, [FromQuery] int profundidade = 2)
    {
        var query = _queryBuilderService.MontarQuery(tabela, incluirJoins: true, profundidadeMaxima: profundidade);
        var compiled = _compiler.Compile(query);
        return Ok(new { Tabela = tabela, Profundidade = profundidade, SQL = compiled.Sql, Parametros = compiled.NamedBindings });
    }

    /// <summary>
    /// Testa geração de query com filtros
    /// </summary>
    [HttpPost("com-filtros/{tabela}")]
    public IActionResult GerarQueryComFiltros(string tabela, [FromBody] Dictionary<string, object> filtros)
    {
        var query = _queryBuilderService.MontarQueryComFiltros(tabela, filtros, incluirJoins: false);
        var compiled = _compiler.Compile(query);
        return Ok(new { Tabela = tabela, TotalFiltros = filtros.Count, SQL = compiled.Sql, Parametros = compiled.NamedBindings });
    }
}
