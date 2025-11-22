using MediatR;
using Microsoft.Extensions.Logging;
using QueryBuilder.Domain.Commands.Metadados;
using QueryBuilder.Domain.Interfaces;
using QueryBuilder.Domain.Notifications;

namespace QueryBuilder.Domain.Commands.Handlers
{
    /// <summary>
    /// Handler para desativação (soft delete) de metadados
    /// </summary>
    public class DesativarMetadadoCommandHandler : IRequestHandler<DesativarMetadadoCommand, bool>
    {
        private readonly IMetadadosRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationContext _notificationContext;
        private readonly ILogger<DesativarMetadadoCommandHandler> _logger;

        public DesativarMetadadoCommandHandler(
            IMetadadosRepository repository,
            IUnitOfWork unitOfWork,
            INotificationContext notificationContext,
            ILogger<DesativarMetadadoCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _notificationContext = notificationContext;
            _logger = logger;
        }

        public async Task<bool> Handle(DesativarMetadadoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Desativando metadado ID: {Id}", request.Id);

            // Buscar metadado existente
            var metadado = await _repository.ObterPorIdAsync(request.Id);
            if (metadado == null)
            {
                _notificationContext.AddNotification(
                    "NotFound",
                    $"Metadado com ID {request.Id} não encontrado");

                _logger.LogWarning("Tentativa de desativar metadado inexistente - ID: {Id}", request.Id);
                return false;
            }

            // Verificar se já está inativo
            if (!metadado.Ativo)
            {
                _notificationContext.AddNotification(
                    "JaDesativado",
                    $"Metadado com ID {request.Id} já está desativado");

                _logger.LogWarning("Tentativa de desativar metadado já inativo - ID: {Id}", request.Id);
                return false;
            }

            try
            {
                // Iniciar transação
                _unitOfWork.BeginTransaction();

                // Desativar entidade de domínio
                metadado.Desativar();

                // Persistir no banco
                await _repository.AtualizarAsync(metadado);

                // Commit da transação
                _unitOfWork.Commit();

                _logger.LogInformation(
                    "Metadado desativado com sucesso - ID: {Id}, Tabela: {Tabela}",
                    metadado.Id, metadado.Tabela);

                return true;
            }
            catch (Exception ex)
            {
                // Rollback em caso de erro
                _unitOfWork.Rollback();

                _notificationContext.AddNotification("Erro", "Erro ao desativar metadado no banco de dados");
                _logger.LogError(ex, "Erro ao desativar metadado ID: {Id}", request.Id);
                return false;
            }
        }
    }
}
