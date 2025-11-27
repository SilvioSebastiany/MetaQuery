using SqlKata;

namespace MetaQuery.Domain.Interfaces;

/// <summary>
/// Interface do repositório de consultas dinâmicas
/// Constitution 2.6: Todos os métodos async recebem CancellationToken
/// </summary>
public interface IConsultaDinamicaRepository
{
    Task<IEnumerable<dynamic>> ExecutarQueryAsync(Query query, CancellationToken cancellationToken = default);
    Task<int> ExecutarQueryCountAsync(Query query, CancellationToken cancellationToken = default);
    Task<T?> ExecutarQuerySingleAsync<T>(Query query, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> ExecutarQueryAsync<T>(Query query, CancellationToken cancellationToken = default);
}
