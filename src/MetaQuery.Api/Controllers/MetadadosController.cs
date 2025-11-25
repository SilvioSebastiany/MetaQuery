using MediatR;
using Microsoft.AspNetCore.Mvc;
using MetaQuery.Domain.Commands.CriarMetadado;
using MetaQuery.Domain.Commands.AtualizarMetadado;
using MetaQuery.Domain.Commands.DesativarMetadado;
using MetaQuery.Domain.Interfaces;

namespace MetaQuery.Api.Controllers
{
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
        public async Task<IActionResult> ObterTodos([FromQuery] bool apenasAtivos = true)
        {
            var metadados = await _repository.ObterTodosAsync(apenasAtivos);
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
        public async Task<IActionResult> ObterPorId(int id)
        {
            var metadado = await _repository.ObterPorIdAsync(id);
            return metadado == null ? NotFound() : Ok(metadado);
        }

        /// <summary>
        /// Obtém metadado por nome da tabela
        /// </summary>
        [HttpGet("tabela/{nomeTabela}")]
        public async Task<IActionResult> ObterPorTabela(string nomeTabela)
        {
            var metadado = await _repository.ObterPorNomeTabelaAsync(nomeTabela);
            return metadado == null ? NotFound() : Ok(metadado);
        }

        /// <summary>
        /// Cria um novo metadado
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarMetadadoCommand command)
        {
            var id = await _mediator.Send(command);
            return id > 0
                ? CreatedAtAction(nameof(ObterPorId), new { id }, new { id })
                : BadRequest();
        }

        /// <summary>
        /// Atualiza um metadado existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarMetadadoCommand command)
        {
            // Recria o command com o ID da rota
            var commandComId = command with { Id = id };
            var sucesso = await _mediator.Send(commandComId);
            return sucesso ? Ok() : NotFound();
        }

        /// <summary>
        /// Desativa um metadado (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Desativar(int id)
        {
            var sucesso = await _mediator.Send(new DesativarMetadadoCommand(id));
            return sucesso ? Ok() : NotFound();
        }
    }
}
