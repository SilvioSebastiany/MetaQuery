using MediatR;

namespace MetaQuery.Domain.Queries;

/// <summary>
/// Query para consulta dinâmica de dados
/// </summary>
public record ConsultaDinamicaQuery(
    string Tabela,
    bool IncluirJoins = false,
    int Profundidade = 1
) : IRequest<ConsultaDinamicaResult>;

/// <summary>
/// Resultado da consulta dinâmica
/// </summary>
public record ConsultaDinamicaResult(
    string Tabela,
    int TotalRegistros,
    IEnumerable<dynamic> Dados,
    string SqlGerado
);
