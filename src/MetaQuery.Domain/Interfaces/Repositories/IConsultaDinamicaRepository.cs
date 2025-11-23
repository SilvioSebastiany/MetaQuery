using SqlKata;

namespace MetaQuery.Domain.Interfaces;

/// <summary>
/// Interface do repositório de consultas dinâmicas
/// </summary>
public interface IConsultaDinamicaRepository
{
    Task<IEnumerable<dynamic>> ExecutarQueryAsync(Query query);
    Task<int> ExecutarQueryCountAsync(Query query);
    Task<T?> ExecutarQuerySingleAsync<T>(Query query);
    Task<IEnumerable<T>> ExecutarQueryAsync<T>(Query query);
}
