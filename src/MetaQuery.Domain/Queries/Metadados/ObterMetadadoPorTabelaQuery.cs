using MediatR;
using MetaQuery.Domain.Entities;

namespace MetaQuery.Domain.Queries.Metadados;

/// <summary>
/// Query para obter metadado por nome da tabela
/// </summary>
public record ObterMetadadoPorTabelaQuery(
    string NomeTabela
) : IRequest<TabelaDinamica?>;
