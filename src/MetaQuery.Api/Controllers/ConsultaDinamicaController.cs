using Microsoft.AspNetCore.Mvc;
using MetaQuery.Domain.DomainServices;
using MetaQuery.Domain.Interfaces;

namespace MetaQuery.Api.Controllers;

/// <summary>
/// Controller para consultas dinâmicas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConsultaDinamicaController : ControllerBase
{
    private readonly ConsultaDinamicaDomainService _consultaService;
    private readonly IMetadadosRepository _metadadosRepository;

    public ConsultaDinamicaController(
        ConsultaDinamicaDomainService consultaService,
        IMetadadosRepository metadadosRepository)
    {
        _consultaService = consultaService;
        _metadadosRepository = metadadosRepository;
    }

    /// <summary>
    /// Consulta dados de uma tabela dinamicamente
    /// </summary>
    [HttpGet("{tabela}")]
    public async Task<IActionResult> Consultar(
        string tabela,
        [FromQuery] bool incluirJoins = false,
        [FromQuery] int profundidade = 2,
        [FromQuery] bool formatoHierarquico = false)
    {
        try
        {
            var resultado = await _consultaService.ConsultarTabelaAsync(
                tabela,
                incluirJoins,
                profundidade,
                formatoHierarquico);

            return Ok(new
            {
                Tabela = tabela,
                Formato = formatoHierarquico ? "hierarchical" : "flat",
                IncluiJoins = incluirJoins,
                Profundidade = profundidade,
                Total = resultado.TotalRegistros,
                Dados = resultado.Dados,
                Debug = new { SqlGerado = resultado.SqlGerado }
            });
        }
        catch (ArgumentException ex)
        {
            // Tabela não existe no banco (ORA-00942)
            return BadRequest(new
            {
                Erro = "Tabela não encontrada",
                Mensagem = ex.Message,
                Tabela = tabela,
                Tipo = "TableNotFound"
            });
        }
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
