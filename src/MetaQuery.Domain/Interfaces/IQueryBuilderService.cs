using SqlKata;

namespace MetaQuery.Domain.Interfaces;

/// <summary>
/// Interface do serviço de montagem de queries dinâmicas
/// </summary>
public interface IQueryBuilderService
{
    Task CarregarMetadadosAsync();
    Query MontarQuery(string tabelaBase, bool incluirJoins = true, int profundidadeMaxima = 3);
    Query MontarQueryComFiltros(string tabelaBase, Dictionary<string, object> filtros, bool incluirJoins = true);
    Query MontarQueryComOrdenacao(string tabelaBase, string campoOrdenacao, bool descendente = false, bool incluirJoins = true);
    Query MontarQueryComPaginacao(string tabelaBase, int pagina, int itensPorPagina, bool incluirJoins = true);
    SqlResult CompilarQuery(Query query);
    List<string> ListarTabelas();
    bool TabelaExiste(string tabela);
    string ObterGrafoRelacionamentos(string tabelaBase, int profundidade = 3);
}
