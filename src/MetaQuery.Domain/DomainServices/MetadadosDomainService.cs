using Microsoft.Extensions.Logging;
using MetaQuery.Domain.Entities;
using MetaQuery.Domain.Interfaces;

namespace MetaQuery.Domain.DomainServices;

/// <summary>
/// Domain Service responsável pela lógica de negócio de metadados
/// </summary>
public class MetadadosDomainService
{
    private readonly IMetadadosRepository _metadadosRepository;
    private readonly ILogger<MetadadosDomainService> _logger;

    public MetadadosDomainService(
        IMetadadosRepository metadadosRepository,
        ILogger<MetadadosDomainService> logger)
    {
        _metadadosRepository = metadadosRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os metadados com lógica de negócio aplicada
    /// </summary>
    public async Task<IEnumerable<TabelaDinamica>> ObterTodosAsync(bool apenasAtivos = true)
    {
        _logger.LogInformation("Obtendo metadados (apenasAtivos={ApenasAtivos})", apenasAtivos);

        var metadados = await _metadadosRepository.ObterTodosAsync(apenasAtivos);

        // Regra de negócio: Alertar se nenhum metadado encontrado
        if (!metadados.Any())
        {
            _logger.LogWarning("Nenhum metadado encontrado no sistema");
        }

        return metadados;
    }

    /// <summary>
    /// Obtém metadado por ID com validações de negócio
    /// </summary>
    public async Task<TabelaDinamica?> ObterPorIdAsync(int id)
    {
        _logger.LogInformation("Obtendo metadado ID={Id}", id);

        // Regra de negócio: ID deve ser positivo
        if (id <= 0)
        {
            _logger.LogWarning("ID inválido fornecido: {Id}", id);
            return null;
        }

        var metadado = await _metadadosRepository.ObterPorIdAsync(id);

        if (metadado == null)
        {
            _logger.LogWarning("Metadado ID={Id} não encontrado", id);
        }

        return metadado;
    }

    /// <summary>
    /// Obtém metadado por nome da tabela
    /// </summary>
    public async Task<TabelaDinamica?> ObterPorTabelaAsync(string nomeTabela)
    {
        _logger.LogInformation("Obtendo metadado da tabela {Tabela}", nomeTabela);

        // Regra de negócio: Nome da tabela não pode ser vazio
        if (string.IsNullOrWhiteSpace(nomeTabela))
        {
            _logger.LogWarning("Nome de tabela inválido fornecido");
            return null;
        }

        var metadado = await _metadadosRepository.ObterPorNomeTabelaAsync(nomeTabela);

        if (metadado == null)
        {
            _logger.LogWarning("Metadado da tabela {Tabela} não encontrado", nomeTabela);
        }

        return metadado;
    }

    /// <summary>
    /// Obtém metadados visíveis para IA
    /// </summary>
    public async Task<IEnumerable<TabelaDinamica>> ObterVisiveisParaIAAsync()
    {
        _logger.LogInformation("Obtendo metadados visíveis para IA");

        var metadados = await _metadadosRepository.ObterVisiveisParaIAAsync();

        // Regra de negócio: Log quantidade para monitoramento
        _logger.LogInformation(
            "{Total} metadados estão visíveis para IA",
            metadados.Count());

        return metadados;
    }

    /// <summary>
    /// Cria novo metadado com validações de negócio (usado por CommandHandler)
    /// </summary>
    public async Task<int> CriarAsync(TabelaDinamica metadado)
    {
        _logger.LogInformation("Criando metadado para tabela {Tabela}", metadado.Tabela);

        // Regra de negócio: Verificar se tabela já existe
        var existe = await _metadadosRepository.ExisteAsync(metadado.Tabela);
        if (existe)
        {
            _logger.LogWarning("Tabela {Tabela} já possui metadados cadastrados", metadado.Tabela);
            throw new InvalidOperationException($"Metadado para tabela '{metadado.Tabela}' já existe");
        }

        // Persistir no banco
        var novoId = await _metadadosRepository.CriarAsync(metadado);

        _logger.LogInformation(
            "Metadado criado com sucesso: ID={Id}, Tabela={Tabela}",
            novoId, metadado.Tabela);

        return novoId;
    }

    /// <summary>
    /// Atualiza metadado existente (usado por CommandHandler)
    /// </summary>
    public async Task AtualizarAsync(TabelaDinamica metadado)
    {
        _logger.LogInformation("Atualizando metadado ID={Id}", metadado.Id);

        // Regra de negócio: Verificar se existe antes de atualizar
        var existe = await _metadadosRepository.ObterPorIdAsync(metadado.Id);
        if (existe == null)
        {
            _logger.LogWarning("Tentativa de atualizar metadado inexistente: ID={Id}", metadado.Id);
            throw new InvalidOperationException($"Metadado com ID '{metadado.Id}' não encontrado");
        }

        await _metadadosRepository.AtualizarAsync(metadado);

        _logger.LogInformation("Metadado ID={Id} atualizado com sucesso", metadado.Id);
    }

    /// <summary>
    /// Desativa metadado (soft delete) (usado por CommandHandler)
    /// </summary>
    public async Task DesativarAsync(int id)
    {
        _logger.LogInformation("Desativando metadado ID={Id}", id);

        var metadado = await _metadadosRepository.ObterPorIdAsync(id);
        if (metadado == null)
        {
            _logger.LogWarning("Tentativa de desativar metadado inexistente: ID={Id}", id);
            throw new InvalidOperationException($"Metadado com ID '{id}' não encontrado");
        }

        // Regra de negócio: Usar soft delete ao invés de deletar fisicamente
        metadado.Desativar();
        await _metadadosRepository.AtualizarAsync(metadado);

        _logger.LogInformation("Metadado ID={Id} desativado com sucesso", id);
    }
}
