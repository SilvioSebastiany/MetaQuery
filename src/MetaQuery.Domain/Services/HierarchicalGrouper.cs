using System.Dynamic;
using MetaQuery.Domain.Interfaces;

namespace MetaQuery.Domain.Services;

/// <summary>
/// Serviço para agrupar resultados flat de JOINs em estrutura hierárquica
/// </summary>
public class HierarchicalGrouper
{
    private readonly IMetadadosRepository _metadadosRepository;

    public HierarchicalGrouper(IMetadadosRepository metadadosRepository)
    {
        _metadadosRepository = metadadosRepository;
    }

    /// <summary>
    /// Agrupa dados flat em estrutura hierárquica baseando-se nos metadados
    /// </summary>
    public async Task<IEnumerable<dynamic>> AgruparHierarquicamenteAsync(
        IEnumerable<dynamic> dadosFlat,
        string tabelaPrincipal)
    {
        if (!dadosFlat.Any())
            return Enumerable.Empty<dynamic>();

        // 1. Buscar metadados da tabela principal
        var metadado = await _metadadosRepository.ObterPorNomeTabelaAsync(tabelaPrincipal);
        if (metadado == null)
            return dadosFlat; // Retorna flat se não encontrar metadados

        // 2. Identificar relacionamentos
        var vinculos = ParseVinculos(metadado.VinculoEntreTabela);
        if (!vinculos.Any())
            return dadosFlat; // Sem relacionamentos, retorna flat

        // 3. Separar relacionamentos por tipo
        var relacionamentos1toN = vinculos.Where(v => v.Tipo == TipoRelacionamento.UmParaVarios).ToList();
        var relacionamentosNto1 = vinculos.Where(v => v.Tipo == TipoRelacionamento.VariosParaUm).ToList();

        // 4. Agrupar por PK da tabela principal
        var pkField = metadado.ChavePk;
        var camposPrincipais = metadado.ObterListaCampos();

        var agrupados = dadosFlat
            .GroupBy(d => GetPropertyValue(d, pkField))
            .Select(grupo =>
            {
                var primeiro = grupo.First();
                var resultado = new ExpandoObject() as IDictionary<string, object>;

                // 4.1. Campos da tabela principal (sem prefixo)
                foreach (var campo in camposPrincipais)
                {
                    var valor = GetPropertyValue(primeiro, campo);
                    if (valor != null)
                    {
                        resultado[ToCamelCase(campo)] = valor;
                    }
                }

                // 4.2. Relacionamentos N:1 (objeto único - ex: Cliente do Pedido)
                foreach (var vinculo in relacionamentosNto1)
                {
                    var objeto = ExtrairObjeto(primeiro, vinculo.TabelaDestino);
                    if (objeto != null && ((IDictionary<string, object>)objeto).Any())
                    {
                        resultado[ToCamelCase(vinculo.TabelaDestino)] = objeto;
                    }
                }

                // 4.3. Relacionamentos 1:N (array - ex: Itens do Pedido)
                foreach (var vinculo in relacionamentos1toN)
                {
                    var itens = grupo
                        .Select(item => ExtrairObjeto(item, vinculo.TabelaDestino))
                        .Where(obj => obj != null && ((IDictionary<string, object>)obj).Any())
                        .DistinctBy(obj => ((IDictionary<string, object>)obj).FirstOrDefault(kv => kv.Key.EndsWith("id", StringComparison.OrdinalIgnoreCase)).Value)
                        .ToList();

                    if (itens.Any())
                    {
                        // Pluraliza nome da tabela (ex: "item" → "itens")
                        var nomeArray = ToCamelCase(vinculo.TabelaDestino);
                        if (!nomeArray.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                            nomeArray += "s";

                        resultado[nomeArray] = itens;
                    }
                }

                return (dynamic)resultado;
            })
            .ToList();

        return agrupados;
    }

    /// <summary>
    /// Parse da string de vínculos (ex: "ITENS_PEDIDO:ID_PEDIDO:ID;CLIENTES:ID:ID_CLIENTE")
    /// </summary>
    private List<Vinculo> ParseVinculos(string? vinculoString)
    {
        if (string.IsNullOrEmpty(vinculoString))
            return new List<Vinculo>();

        return vinculoString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(v =>
            {
                var partes = v.Split(':');
                if (partes.Length != 3)
                    return null;

                var tabelaDestino = partes[0].Trim();
                var campoFk = partes[1].Trim();
                var campoPk = partes[2].Trim();

                // Determinar tipo de relacionamento
                // Se FK contém o nome da tabela destino = 1:N (ex: ITENS_PEDIDO com ID_PEDIDO)
                // Caso contrário = N:1 (ex: PEDIDOS com ID_CLIENTE referenciando CLIENTES)
                var tipo = campoFk.Contains(tabelaDestino, StringComparison.OrdinalIgnoreCase) ||
                           campoFk.StartsWith("ID_", StringComparison.OrdinalIgnoreCase)
                    ? TipoRelacionamento.UmParaVarios
                    : TipoRelacionamento.VariosParaUm;

                return new Vinculo(tabelaDestino, campoFk, campoPk, tipo);
            })
            .Where(v => v != null)
            .Cast<Vinculo>()
            .ToList();
    }

    /// <summary>
    /// Extrai objeto de uma tabela relacionada baseando-se no prefixo dos campos
    /// </summary>
    private dynamic? ExtrairObjeto(dynamic source, string prefixoTabela)
    {
        var resultado = new ExpandoObject() as IDictionary<string, object>;
        var propriedades = (IDictionary<string, object>)source;

        // Buscar campos com prefixo da tabela (ex: CLIENTES_ID, CLIENTES_NOME)
        var camposTabela = propriedades
            .Where(p => p.Key.StartsWith(prefixoTabela + "_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!camposTabela.Any())
            return null;

        foreach (var campo in camposTabela)
        {
            // Remove prefixo: CLIENTES_NOME → nome
            var nomeCampo = campo.Key.Substring(prefixoTabela.Length + 1);
            resultado[ToCamelCase(nomeCampo)] = campo.Value;
        }

        return resultado;
    }

    private object? GetPropertyValue(dynamic obj, string propertyName)
    {
        try
        {
            var dict = (IDictionary<string, object>)obj;
            return dict.ContainsKey(propertyName) ? dict[propertyName] : null;
        }
        catch
        {
            return null;
        }
    }

    private string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        // Remove underscores e converte para camelCase
        var parts = str.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return str.ToLowerInvariant();

        var result = parts[0].ToLowerInvariant();
        for (int i = 1; i < parts.Length; i++)
        {
            result += char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1).ToLowerInvariant();
        }

        return result;
    }
}

/// <summary>
/// Representa um vínculo entre tabelas
/// </summary>
public record Vinculo(string TabelaDestino, string CampoFK, string CampoPK, TipoRelacionamento Tipo);

/// <summary>
/// Tipo de relacionamento entre tabelas
/// </summary>
public enum TipoRelacionamento
{
    /// <summary>1:N - Ex: Pedido → Itens</summary>
    UmParaVarios,

    /// <summary>N:1 - Ex: Pedido → Cliente</summary>
    VariosParaUm
}
