using MediatR;
using Microsoft.AspNetCore.Mvc;
using MetaQuery.Domain.Interfaces;
using MetaQuery.Domain.Queries;

namespace MetaQuery.Api.Controllers;

/// <summary>
/// Controller para consultas dinâmicas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConsultaDinamicaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMetadadosRepository _metadadosRepository;

    public ConsultaDinamicaController(IMediator mediator, IMetadadosRepository metadadosRepository)
    {
        _mediator = mediator;
        _metadadosRepository = metadadosRepository;
    }

    /// <summary>
    /// Consulta dados de uma tabela dinamicamente
    /// </summary>
    [HttpGet("{tabela}")]
    public async Task<IActionResult> Consultar(
        string tabela,
        [FromQuery] bool incluirJoins = false,
        [FromQuery] int profundidade = 2)
    {
        var resultado = await _mediator.Send(new ConsultaDinamicaQuery(tabela, incluirJoins, profundidade));
        return Ok(resultado);
    }

    /// <summary>
    /// Lista todas as tabelas disponíveis para consulta
    /// </summary>
    [HttpGet("tabelas-disponiveis")]
    public async Task<IActionResult> ListarTabelasDisponiveis()
    {
        var metadados = await _metadadosRepository.ObterTodosAsync(apenasAtivos: true);
        var tabelas = metadados.Select(m => m.Tabela).OrderBy(t => t);
        return Ok(new { Total = tabelas.Count(), Tabelas = tabelas });
    }
}
