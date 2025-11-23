using MetaQuery.Domain.Entities;

namespace MetaQuery.Domain.Interfaces;

/// <summary>
/// Interface do repositório de metadados
/// </summary>
public interface IMetadadosRepository
{
    Task<TabelaDinamica?> ObterPorIdAsync(int id);
    Task<TabelaDinamica?> ObterPorNomeTabelaAsync(string nomeTabela);
    Task<IEnumerable<TabelaDinamica>> ObterTodosAsync(bool apenasAtivos = true);
    Task<IEnumerable<TabelaDinamica>> ObterVisiveisParaIAAsync();
    Task<int> CriarAsync(TabelaDinamica tabela);
    Task AtualizarAsync(TabelaDinamica tabela);
    Task DeletarAsync(int id);
    Task<bool> ExisteAsync(string nomeTabela);
    Task<IEnumerable<TabelaDinamica>> ObterPorVinculoAsync(string nomeTabela);
}
