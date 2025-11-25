using System.Collections.Generic;

namespace MetaQuery.Domain.Notifications;

/// <summary>
/// Resultado de uma consulta dinâmica
/// </summary>
public record ConsultaDinamicaResult(string Tabela, int TotalRegistros, IEnumerable<dynamic> Dados, string SqlGerado);
