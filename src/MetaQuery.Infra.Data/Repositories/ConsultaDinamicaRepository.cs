using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using MetaQuery.Domain.Interfaces;
using SqlKata;
using SqlKata.Compilers;

namespace MetaQuery.Infra.Data.Repositories;

/// <summary>
/// Repositório para execução de consultas dinâmicas geradas pelo QueryBuilderService
/// </summary>
public class ConsultaDinamicaRepository : IConsultaDinamicaRepository
{
    private readonly IDbConnection _connection;
    private readonly OracleCompiler _compiler;
    private readonly ILogger<ConsultaDinamicaRepository> _logger;

    public ConsultaDinamicaRepository(
        IDbConnection connection,
        OracleCompiler compiler,
        ILogger<ConsultaDinamicaRepository> logger)
    {
        _connection = connection;
        _compiler = compiler;
        _logger = logger;
    }

    /// <summary>
    /// Executa uma query dinâmica e retorna os resultados como objetos dinâmicos
    /// </summary>
    public async Task<IEnumerable<dynamic>> ExecutarQueryAsync(Query query)
    {
        var compiled = _compiler.Compile(query);

        _logger.LogInformation("Executando query dinâmica: {Sql}", compiled.Sql);
        _logger.LogDebug("Parâmetros: {@Parametros}", compiled.NamedBindings);

        try
        {
            var resultados = await _connection.QueryAsync<dynamic>(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: 30
            );

            var total = resultados.Count();
            _logger.LogInformation("Query executada com sucesso. {Total} registros retornados", total);

            return resultados;
        }
        catch (Oracle.ManagedDataAccess.Client.OracleException oraEx) when (oraEx.Number == 942)
        {
            // ORA-00942: tabela não existe
            _logger.LogWarning(oraEx, "Tabela não encontrada no banco. SQL: {Sql}", compiled.Sql);

            var msg = "A tabela consultada está cadastrada nos metadados mas não existe no banco de dados. " +
                      "Verifique se a tabela foi criada ou se o nome está correto.";

            throw new ArgumentException(msg, nameof(query), oraEx);
        }
        catch (Oracle.ManagedDataAccess.Client.OracleException oraEx) when (oraEx.Number == 904)
        {
            // ORA-00904: coluna não existe
            var columnName = ExtractColumnName(oraEx.Message);
            _logger.LogWarning(oraEx, "Coluna '{ColumnName}' não encontrada. SQL: {Sql}", columnName, compiled.Sql);

            var msg = $"A coluna '{columnName}' está cadastrada nos metadados mas não existe na tabela. " +
                      $"Verifique se o campo foi criado ou se o nome está correto na TABELA_DINAMICA.";

            throw new ArgumentException(msg, nameof(query), oraEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar query dinâmica. SQL: {Sql}", compiled.Sql);
            throw new InvalidOperationException($"Erro ao executar consulta: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executa uma query COUNT para obter o total de registros
    /// </summary>
    public async Task<int> ExecutarQueryCountAsync(Query query)
    {
        // Criar uma cópia da query original apenas com COUNT
        var countQuery = query.Clone().AsCount();
        var compiled = _compiler.Compile(countQuery);

        _logger.LogInformation("Executando COUNT: {Sql}", compiled.Sql);

        try
        {
            var count = await _connection.ExecuteScalarAsync<int>(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: 30
            );

            _logger.LogInformation("COUNT executado com sucesso: {Count}", count);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar COUNT. SQL: {Sql}", compiled.Sql);
            throw new InvalidOperationException($"Erro ao contar registros: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executa uma query e retorna um único resultado tipado
    /// </summary>
    public async Task<T?> ExecutarQuerySingleAsync<T>(Query query)
    {
        var compiled = _compiler.Compile(query);

        _logger.LogInformation("Executando query single: {Sql}", compiled.Sql);

        try
        {
            var resultado = await _connection.QueryFirstOrDefaultAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: 30
            );

            if (resultado == null)
            {
                _logger.LogWarning("Nenhum registro encontrado");
            }
            else
            {
                _logger.LogInformation("Registro retornado com sucesso");
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar query single. SQL: {Sql}", compiled.Sql);
            throw new InvalidOperationException($"Erro ao buscar registro: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executa uma query e retorna resultados tipados
    /// </summary>
    public async Task<IEnumerable<T>> ExecutarQueryAsync<T>(Query query)
    {
        var compiled = _compiler.Compile(query);

        _logger.LogInformation("Executando query tipada: {Sql}", compiled.Sql);

        try
        {
            var resultados = await _connection.QueryAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: 30
            );

            var total = resultados.Count();
            _logger.LogInformation("Query tipada executada com sucesso. {Total} registros retornados", total);

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar query tipada. SQL: {Sql}", compiled.Sql);
            throw new InvalidOperationException($"Erro ao executar consulta tipada: {ex.Message}", ex);
        }
    }

    private string ExtractColumnName(string errorMessage)
    {
        try
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                errorMessage,
                "\"([^\"]+)\"\\.\"([^\"]+)\"");

            if (match.Success && match.Groups.Count >= 3)
                return $"{match.Groups[1].Value}.{match.Groups[2].Value}";

            var simpleMatch = System.Text.RegularExpressions.Regex.Match(
                errorMessage,
                "\"([^\"]+)\"");

            return simpleMatch.Success ? simpleMatch.Groups[1].Value : "desconhecida";
        }
        catch
        {
            return "desconhecida";
        }
    }
}
