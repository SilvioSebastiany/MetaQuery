using MetaQuery.Domain.Entities;

namespace MetaQuery.Domain.Interfaces;

/// <summary>
/// Interface do serviço de validação de metadados
/// </summary>
public interface IValidacaoMetadadosService
{
    Task<List<string>> ValidarMetadadoAsync(TabelaDinamica metadado);
    Task<bool> CampoExisteNoBancoAsync(string tabela, string campo);
    Task<bool> TabelaExisteNoBancoAsync(string tabela);
    Task<List<string>> ValidarVinculosAsync(TabelaDinamica metadado);
}
