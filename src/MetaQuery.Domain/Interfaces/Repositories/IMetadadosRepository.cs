using MetaQuery.Domain.Entities;

namespace MetaQuery.Domain.Interfaces;

/// <summary>
/// Interface do repositório de metadados
/// Constitution 2.6: Todos os métodos async recebem CancellationToken
/// </summary>
public interface IMetadadosRepository
{
    Task<TabelaDinamica?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TabelaDinamica?> ObterPorNomeTabelaAsync(string nomeTabela, CancellationToken cancellationToken = default);
    Task<IEnumerable<TabelaDinamica>> ObterTodosAsync(bool apenasAtivos = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<TabelaDinamica>> ObterVisiveisParaIAAsync(CancellationToken cancellationToken = default);
    Task<int> CriarAsync(TabelaDinamica tabela, CancellationToken cancellationToken = default);
    Task AtualizarAsync(TabelaDinamica tabela, CancellationToken cancellationToken = default);
    Task DeletarAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(string nomeTabela, CancellationToken cancellationToken = default);
    Task<IEnumerable<TabelaDinamica>> ObterPorVinculoAsync(string nomeTabela, CancellationToken cancellationToken = default);
}
