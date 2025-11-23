using MediatR;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.Commands.CriarMetadado;
using MetaQuery.Domain.Entities;
using MetaQuery.Domain.Interfaces;
using MetaQuery.Domain.Notifications;

namespace MetaQuery.Domain.Commands.CriarMetadado
{
    /// <summary>
    /// Handler para criação de metadados de tabela dinâmica
    /// </summary>
    public class CriarMetadadoCommandHandler : IRequestHandler<CriarMetadadoCommand, int>
    {
        private readonly IMetadadosRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationContext _notificationContext;
        private readonly ILogger<CriarMetadadoCommandHandler> _logger;

        public CriarMetadadoCommandHandler(
            IMetadadosRepository repository,
            IUnitOfWork unitOfWork,
            INotificationContext notificationContext,
            ILogger<CriarMetadadoCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _notificationContext = notificationContext;
            _logger = logger;
        }

        public async Task<int> Handle(CriarMetadadoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Criando novo metadado para tabela: {Tabela}", request.Tabela);

            // Verificar se tabela já existe
            var existe = await _repository.ExisteAsync(request.Tabela);
            if (existe)
            {
                _notificationContext.AddNotification(
                    nameof(request.Tabela),
                    $"Já existe um metadado cadastrado para a tabela '{request.Tabela}'");

                _logger.LogWarning("Tentativa de criar metadado duplicado para tabela: {Tabela}", request.Tabela);
                return 0;
            }

            try
            {
                // Iniciar transação
                _unitOfWork.BeginTransaction();

                // Criar entidade de domínio usando factory method
                var metadado = TabelaDinamica.Criar(
                    tabela: request.Tabela,
                    camposDisponiveis: request.CamposDisponiveis,
                    chavePk: request.ChavePk,
                    vinculoEntreTabela: request.VinculoEntreTabela,
                    descricaoTabela: request.DescricaoTabela,
                    descricaoCampos: request.DescricaoCampos,
                    visivelParaIA: request.VisivelParaIA
                );

                // Persistir no banco
                var id = await _repository.CriarAsync(metadado);

                // Commit da transação
                _unitOfWork.Commit();

                _logger.LogInformation(
                    "Metadado criado com sucesso - ID: {Id}, Tabela: {Tabela}",
                    id, metadado.Tabela);

                return id;
            }
            catch (ArgumentException ex)
            {
                // Rollback em caso de erro de validação
                _unitOfWork.Rollback();

                // Exceções de validação do domínio viram notificações
                _notificationContext.AddNotification("Validacao", ex.Message);
                _logger.LogWarning(ex, "Erro de validação ao criar metadado para tabela: {Tabela}", request.Tabela);
                return 0;
            }
            catch (Exception ex)
            {
                // Rollback em caso de qualquer erro
                _unitOfWork.Rollback();

                _notificationContext.AddNotification("Erro", "Erro ao criar metadado no banco de dados");
                _logger.LogError(ex, "Erro ao criar metadado para tabela: {Tabela}", request.Tabela);
                return 0;
            }
        }
    }
}
