using MediatR;
using Microsoft.AspNetCore.Mvc;
using MetaQuery.Domain.Commands.CriarMetadado;
using MetaQuery.Domain.Commands.AtualizarMetadado;
using MetaQuery.Domain.Commands.DesativarMetadado;
using MetaQuery.Domain.Interfaces;

namespace MetaQuery.Api.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de metadados
    /// Constitution 2.6: Todos os métodos async recebem CancellationToken
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MetadadosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMetadadosRepository _repository;

        public MetadadosController(
            IMediator mediator,
            IMetadadosRepository repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

        /// <summary>
        /// Lista todos os metadados cadastrados
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObterTodos([FromQuery] bool apenasAtivos = true, CancellationToken cancellationToken = default)
        {
            var metadados = await _repository.ObterTodosAsync(apenasAtivos, cancellationToken);
            return Ok(new
            {
                Total = metadados.Count(),
                Metadados = metadados
            });
        }

        /// <summary>
        /// Obtém metadado por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken = default)
        {
            var metadado = await _repository.ObterPorIdAsync(id, cancellationToken);
            return metadado == null ? NotFound() : Ok(metadado);
        }

        /// <summary>
        /// Obtém metadado por nome da tabela
        /// </summary>
        [HttpGet("tabela/{nomeTabela}")]
        public async Task<IActionResult> ObterPorTabela(string nomeTabela, CancellationToken cancellationToken = default)
        {
            var metadado = await _repository.ObterPorNomeTabelaAsync(nomeTabela, cancellationToken);
            return metadado == null ? NotFound() : Ok(metadado);
        }

        /// <summary>
        /// Cria um novo metadado
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarMetadadoCommand command, CancellationToken cancellationToken = default)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return id > 0
                ? CreatedAtAction(nameof(ObterPorId), new { id }, new { id })
                : BadRequest();
        }

        /// <summary>
        /// Atualiza um metadado existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarMetadadoCommand command, CancellationToken cancellationToken = default)
        {
            // Recria o command com o ID da rota
            var commandComId = command with { Id = id };
            var sucesso = await _mediator.Send(commandComId, cancellationToken);
            return sucesso ? Ok() : NotFound();
        }

        /// <summary>
        /// Desativa um metadado (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Desativar(int id, CancellationToken cancellationToken = default)
        {
            var sucesso = await _mediator.Send(new DesativarMetadadoCommand(id), cancellationToken);
            return sucesso ? Ok() : NotFound();
        }
    }
}
