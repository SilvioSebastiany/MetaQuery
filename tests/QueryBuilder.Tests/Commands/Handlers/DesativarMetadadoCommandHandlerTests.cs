using Moq;
using Xunit;
using FluentAssertions;
using QueryBuilder.Domain.Commands.Handlers;
using QueryBuilder.Domain.Commands.Metadados;
using QueryBuilder.Domain.Interfaces;
using QueryBuilder.Infra.Data.Repositories;
using QueryBuilder.Domain.Notifications;
using Microsoft.Extensions.Logging;
using QueryBuilder.Domain.Entities;

namespace QueryBuilder.Tests.Commands.Handlers
{
    public class DesativarMetadadoCommandHandlerTests
    {
        private readonly Mock<IMetadadosRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<INotificationContext> _notificationContextMock;
        private readonly Mock<ILogger<DesativarMetadadoCommandHandler>> _loggerMock;
        private readonly DesativarMetadadoCommandHandler _handler;

        public DesativarMetadadoCommandHandlerTests()
        {
            _repositoryMock = new Mock<IMetadadosRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _notificationContextMock = new Mock<INotificationContext>();
            _loggerMock = new Mock<ILogger<DesativarMetadadoCommandHandler>>();

            _handler = new DesativarMetadadoCommandHandler(
                _repositoryMock.Object,
                _unitOfWorkMock.Object,
                _notificationContextMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Handle_ComDadosValidos_DeveChamarBeginTransaction()
        {
            // Arrange
            var metadado = TabelaDinamica.Criar("CLIENTES", "ID,NOME", "ID", visivelParaIA: true);
            var command = new DesativarMetadadoCommand(1);

            _repositoryMock.Setup(x => x.ObterPorIdAsync(1))
                .ReturnsAsync(metadado);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComDadosValidos_DeveChamarCommit()
        {
            // Arrange
            var metadado = TabelaDinamica.Criar("CLIENTES", "ID,NOME", "ID", visivelParaIA: true);
            var command = new DesativarMetadadoCommand(1);

            _repositoryMock.Setup(x => x.ObterPorIdAsync(1))
                .ReturnsAsync(metadado);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComErroNoDesativar_DeveChamarRollback()
        {
            // Arrange
            var metadado = TabelaDinamica.Criar("CLIENTES", "ID,NOME", "ID", visivelParaIA: true);
            var command = new DesativarMetadadoCommand(1);

            _repositoryMock.Setup(x => x.ObterPorIdAsync(1))
                .ReturnsAsync(metadado);
            _repositoryMock.Setup(x => x.AtualizarAsync(It.IsAny<TabelaDinamica>()))
                .ThrowsAsync(new Exception("Erro ao desativar"));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComMetadadoInexistente_NaoDeveChamarBeginTransaction()
        {
            // Arrange
            var command = new DesativarMetadadoCommand(999);

            _repositoryMock.Setup(x => x.ObterPorIdAsync(999))
                .ReturnsAsync((TabelaDinamica?)null);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Never);
        }

        [Fact]
        public async Task Handle_ComMetadadoInexistente_DeveAdicionarNotificacao()
        {
            // Arrange
            var command = new DesativarMetadadoCommand(999);

            _repositoryMock.Setup(x => x.ObterPorIdAsync(999))
                .ReturnsAsync((TabelaDinamica?)null);

            // Act
            var resultado = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _notificationContextMock.Verify(
                x => x.AddNotification(
                    It.IsAny<string>(),
                    It.Is<string>(msg => msg.Contains("não encontrado"))),
                Times.Once
            );
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ComMetadadoJaInativo_NaoDeveChamarBeginTransaction()
        {
            // Arrange
            var metadado = TabelaDinamica.Criar("CLIENTES", "ID,NOME", "ID", visivelParaIA: true);
            metadado.Desativar(); // Já desativa

            var command = new DesativarMetadadoCommand(1);

            _repositoryMock.Setup(x => x.ObterPorIdAsync(1))
                .ReturnsAsync(metadado);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Never);
        }

        [Fact]
        public async Task Handle_ComDadosValidos_DeveRetornarTrue()
        {
            // Arrange
            var metadado = TabelaDinamica.Criar("CLIENTES", "ID,NOME", "ID", visivelParaIA: true);
            var command = new DesativarMetadadoCommand(1);

            _repositoryMock.Setup(x => x.ObterPorIdAsync(1))
                .ReturnsAsync(metadado);

            // Act
            var resultado = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultado.Should().BeTrue();
        }
    }
}
