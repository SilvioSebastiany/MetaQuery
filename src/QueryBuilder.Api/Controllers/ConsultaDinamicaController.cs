using MediatR;
using Microsoft.AspNetCore.Mvc;
using QueryBuilder.Domain.Notifications;
using QueryBuilder.Domain.Queries;

namespace QueryBuilder.Api.Controllers;

/// <summary>
/// Controller para consultas dinâmicas - CQRS + MediatR Pattern
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConsultaDinamicaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notificationContext;
    private readonly ILogger<ConsultaDinamicaController> _logger;

    public ConsultaDinamicaController(
        IMediator mediator,
        INotificationContext notificationContext,
        ILogger<ConsultaDinamicaController> logger)
    {
        _mediator = mediator;
        _notificationContext = notificationContext;
        _logger = logger;
    }

    /// <summary>
    /// Consulta dados de uma tabela dinamicamente (CQRS Pattern)
    /// </summary>
    [HttpGet("{tabela}")]
    public async Task<IActionResult> Consultar(
        string tabela,
        [FromQuery] bool incluirJoins = false,
        [FromQuery] int profundidade = 2)
    {
        // Criar query (validações são feitas no pipeline automaticamente)
        var query = new ConsultaDinamicaQuery(tabela, incluirJoins, profundidade);

        // Enviar para MediatR
        var resultado = await _mediator.Send(query);

        // Se tem notificações (validações falharam), retorna 400
        if (_notificationContext.HasNotifications)
        {
            return BadRequest(new
            {
                Erros = _notificationContext.Notifications.Select(n => new
                {
                    Campo = n.Key,
                    Mensagem = n.Message
                })
            });
        }

        // Se resultado é null (erro no handler), retorna 500
        if (resultado == null)
        {
            return StatusCode(500, new { Erro = "Erro ao processar consulta" });
        }

        // Sucesso
        return Ok(new
        {
            Tabela = resultado.Tabela.ToUpper(),
            IncluiJoins = incluirJoins,
            Profundidade = profundidade,
            Total = resultado.TotalRegistros,
            Dados = resultado.Dados,
            Debug = new
            {
                SqlGerado = resultado.SqlGerado
            }
        });
    }

    /// <summary>
    /// Lista todas as tabelas disponíveis para consulta
    /// </summary>
    [HttpGet("tabelas-disponiveis")]
    public IActionResult ListarTabelasDisponiveis()
    {
        var tabelas = new[] { "CLIENTES", "PEDIDOS", "PRODUTOS", "CATEGORIAS", "ITENS_PEDIDO", "ENDERECOS" };

        return Ok(new
        {
            Total = tabelas.Length,
            Tabelas = tabelas.OrderBy(t => t),
            Observacao = "Use GET /api/ConsultaDinamica/{tabela} para consultar"
        });
    }
}
