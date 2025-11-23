namespace MetaQuery.Domain.Interfaces;

/// <summary>
/// Interface do serviço de catálogo para IA
/// </summary>
public interface IIADataCatalogService
{
    Task<string> GerarPromptParaIAAsync();
    Task<string> GerarContextoTabelaAsync(string nomeTabela);
    Task<Dictionary<string, string>> ObterDicionarioTabelasAsync();
    Task<bool> ValidarQueryIAAsync(string query);
}
