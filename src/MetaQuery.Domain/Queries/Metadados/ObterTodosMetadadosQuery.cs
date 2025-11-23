using MediatR;
using MetaQuery.Domain.Entities;

namespace MetaQuery.Domain.Queries.Metadados;

/// <summary>
/// Query para obter todos os metadados
/// </summary>
public record ObterTodosMetadadosQuery(
    bool ApenasAtivos = true
) : IRequest<ObterTodosMetadadosResult>;

/// <summary>
/// Resultado da query de obter todos os metadados
/// </summary>
public record ObterTodosMetadadosResult(
    int Total,
    IEnumerable<TabelaDinamica> Metadados
);
