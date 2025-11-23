using MediatR;
using MetaQuery.Domain.Entities;

namespace MetaQuery.Domain.Queries.Metadados;

/// <summary>
/// Query para obter metadado por ID
/// </summary>
public record ObterMetadadoPorIdQuery(
    int Id
) : IRequest<TabelaDinamica?>;
