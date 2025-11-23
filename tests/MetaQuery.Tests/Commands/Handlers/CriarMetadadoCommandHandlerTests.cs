using Moq;
using Xunit;
using FluentAssertions;
using MetaQuery.Domain.Commands.CriarMetadado;
using MetaQuery.Domain.Interfaces;
using MetaQuery.Infra.Data.Repositories;
using MetaQuery.Domain.Notifications;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.Entities;

namespace MetaQuery.Tests.Commands.Handlers
{
    public class CriarMetadadoCommandHandlerTests
    {
        private readonly Mock<IMetadadosRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<INotificationContext> _notificationContextMock;
        private readonly Mock<ILogger<CriarMetadadoCommandHandler>> _loggerMock;
        private readonly CriarMetadadoCommandHandler _handler;

        public CriarMetadadoCommandHandlerTests()
        {
            _repositoryMock = new Mock<IMetadadosRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _notificationContextMock = new Mock<INotificationContext>();
            _loggerMock = new Mock<ILogger<CriarMetadadoCommandHandler>>();

            _handler = new CriarMetadadoCommandHandler(
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
            var command = new CriarMetadadoCommand
            {
                Tabela = "TESTE",
                CamposDisponiveis = "ID,NOME",
                ChavePk = "ID",
                VisivelParaIA = true
            };

            _repositoryMock.Setup(x => x.ExisteAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(x => x.CriarAsync(It.IsAny<TabelaDinamica>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComDadosValidos_DeveChamarCommit()
        {
            // Arrange
            var command = new CriarMetadadoCommand
            {
                Tabela = "TESTE",
                CamposDisponiveis = "ID,NOME",
                ChavePk = "ID",
                VisivelParaIA = true
            };

            _repositoryMock.Setup(x => x.ExisteAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(x => x.CriarAsync(It.IsAny<TabelaDinamica>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComErroNoCriar_DeveChamarRollback()
        {
            // Arrange
            var command = new CriarMetadadoCommand
            {
                Tabela = "TESTE",
                CamposDisponiveis = "ID,NOME",
                ChavePk = "ID",
                VisivelParaIA = true
            };

            _repositoryMock.Setup(x => x.ExisteAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(x => x.CriarAsync(It.IsAny<TabelaDinamica>()))
                .ThrowsAsync(new Exception("Erro ao criar"));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Once);
        }

        [Fact]
        public async Task Handle_ComErroNoCriar_NaoDeveChamarCommit()
        {
            // Arrange
            var command = new CriarMetadadoCommand
            {
                Tabela = "TESTE",
                CamposDisponiveis = "ID,NOME",
                ChavePk = "ID",
                VisivelParaIA = true
            };

            _repositoryMock.Setup(x => x.ExisteAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(x => x.CriarAsync(It.IsAny<TabelaDinamica>()))
                .ThrowsAsync(new Exception("Erro ao criar"));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public async Task Handle_ComDadosValidos_DeveRetornarIdMaiorQueZero()
        {
            // Arrange
            var command = new CriarMetadadoCommand
            {
                Tabela = "TESTE",
                CamposDisponiveis = "ID,NOME",
                ChavePk = "ID",
                VisivelParaIA = true
            };

            _repositoryMock.Setup(x => x.ExisteAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(x => x.CriarAsync(It.IsAny<TabelaDinamica>()))
                .ReturnsAsync(61);

            // Act
            var resultado = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultado.Should().Be(61);
        }

        [Fact]
        public async Task Handle_ComTabelaDuplicada_DeveAdicionarNotificacao()
        {
            // Arrange
            var command = new CriarMetadadoCommand
            {
                Tabela = "CLIENTES",
                CamposDisponiveis = "ID,NOME",
                ChavePk = "ID",
                VisivelParaIA = true
            };

            _repositoryMock.Setup(x => x.ExisteAsync("CLIENTES"))
                .ReturnsAsync(true);

            // Act
            var resultado = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _notificationContextMock.Verify(
                x => x.AddNotification(
                    It.IsAny<string>(),
                    It.Is<string>(msg => msg.Contains("Já existe"))),
                Times.Once
            );
            resultado.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ComTabelaDuplicada_NaoDeveChamarBeginTransaction()
        {
            // Arrange
            var command = new CriarMetadadoCommand
            {
                Tabela = "CLIENTES",
                CamposDisponiveis = "ID,NOME",
                ChavePk = "ID",
                VisivelParaIA = true
            };

            _repositoryMock.Setup(x => x.ExisteAsync("CLIENTES"))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Never);
        }
    }
}
