# 🤖 Plano de Implementação IA - Parte 2

> **Continuação do arquivo 09_IMPLEMENTACAO_IA.md**

---

## 🔧 Implementação Passo a Passo (Continuação)

### **Fase 3: Services Layer (Continuação)**

#### 3.2 IAQueryGeneratorService

**Arquivo:** `MetaQuery.Infra.Externals/OpenAI/OpenAIService.cs`

```csharp
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MetaQuery.Infra.Externals.OpenAI;

/// <summary>
/// Serviço de integração com OpenAI para geração de SQL
/// </summary>
public class IAQueryGeneratorService
{
    private readonly OpenAIClient _client;
    private readonly OpenAISettings _settings;
    private readonly ILogger<IAQueryGeneratorService> _logger;

    public IAQueryGeneratorService(
        IOptions<OpenAISettings> settings,
        ILogger<IAQueryGeneratorService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new OpenAIClient(new Uri(_settings.BaseUrl), new AzureKeyCredential(_settings.ApiKey));
    }

    /// <summary>
    /// Gera SQL a partir de pergunta em linguagem natural
    /// </summary>
    public async Task<(string sql, int tokens)> GerarSQLAsync(string pergunta, string contexto)
    {
        _logger.LogInformation("Gerando SQL para pergunta: {Pergunta}", pergunta);

        var prompt = MontarPrompt(pergunta, contexto);

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, ObterSystemPrompt()),
                new ChatMessage(ChatRole.User, prompt)
            },
            Temperature = (float)_settings.Temperature,
            MaxTokens = _settings.MaxTokens,
            NucleusSamplingFactor = 1,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        try
        {
            var response = await _client.GetChatCompletionsAsync(
                _settings.Model,
                chatCompletionsOptions
            );

            var sqlGerado = ExtractirSQL(response.Value.Choices[0].Message.Content);
            var tokens = response.Value.Usage.TotalTokens;

            _logger.LogInformation("SQL gerado com sucesso. Tokens usados: {Tokens}", tokens);

            return (sqlGerado, tokens);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Erro ao chamar OpenAI API");
            throw new InvalidOperationException("Erro ao gerar SQL via IA", ex);
        }
    }

    private string ObterSystemPrompt()
    {
        return @"
Você é um especialista em Oracle SQL. Sua tarefa é converter perguntas em linguagem natural para queries SQL válidas.

REGRAS IMPORTANTES:
1. Retorne APENAS o SQL, sem explicações
2. Use apenas SELECT (nunca UPDATE, DELETE, DROP, TRUNCATE, ALTER)
3. Use Oracle SQL syntax
4. Use UPPER() para comparações de texto (case-insensitive)
5. Use JOINs quando precisar combinar tabelas
6. Use WHERE para filtros
7. Use ORDER BY quando apropriado
8. Limite resultados com ROWNUM se necessário
9. Retorne SQL formatado em uma única linha ou com quebras de linha claras

FORMATO DA RESPOSTA:
```sql
[SEU SQL AQUI]
```

Se não conseguir gerar um SQL válido, retorne:
ERRO: [motivo]
";
    }

    private string MontarPrompt(string pergunta, string contexto)
    {
        return $@"
{contexto}

## PERGUNTA DO USUÁRIO:
{pergunta}

## SUA TAREFA:
Converta a pergunta acima em uma query SQL Oracle válida usando apenas as tabelas e campos do catálogo.
";
    }

    private string ExtractirSQL(string resposta)
    {
        // Remove marcadores de código se houver
        var sql = resposta.Trim();

        // Extrai SQL de code block se presente
        if (sql.Contains("```sql"))
        {
            var startIndex = sql.IndexOf("```sql") + 6;
            var endIndex = sql.LastIndexOf("```");
            if (endIndex > startIndex)
            {
                sql = sql.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
        else if (sql.Contains("```"))
        {
            var startIndex = sql.IndexOf("```") + 3;
            var endIndex = sql.LastIndexOf("```");
            if (endIndex > startIndex)
            {
                sql = sql.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }

        // Remove quebras de linha extras
        sql = string.Join(" ", sql.Split('\n').Select(line => line.Trim()));

        // Verifica erro
        if (sql.StartsWith("ERRO:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"IA não conseguiu gerar SQL: {sql}");
        }

        return sql;
    }
}
```

**Arquivo:** `MetaQuery.Infra.Externals/OpenAI/OpenAISettings.cs`

```csharp
namespace MetaQuery.Infra.Externals.OpenAI;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.1;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}
```

#### 3.3 SQLValidatorService

**Arquivo:** `MetaQuery.Domain/Services/SQLValidatorService.cs`

```csharp
using Microsoft.Extensions.Logging;

namespace MetaQuery.Domain.Services;

/// <summary>
/// Valida SQL gerado pela IA antes de executar
/// </summary>
public class SQLValidatorService
{
    private readonly ILogger<SQLValidatorService> _logger;

    // Comandos perigosos BLOQUEADOS
    private static readonly HashSet<string> ComandosProibidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "DELETE", "DROP", "TRUNCATE", "ALTER", "CREATE",
        "UPDATE", "INSERT", "GRANT", "REVOKE", "EXEC",
        "EXECUTE", "CALL", "MERGE", "RENAME"
    };

    // Tabelas permitidas (whitelist)
    private static readonly HashSet<string> TabelasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "CLIENTES", "PEDIDOS", "PRODUTOS", "CATEGORIAS",
        "ITENS_PEDIDO", "ENDERECOS", "PAGAMENTOS"
    };

    public SQLValidatorService(ILogger<SQLValidatorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Valida se SQL é seguro para executar
    /// </summary>
    public (bool valido, string? erro) ValidarSQL(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return (false, "SQL vazio");

        sql = sql.Trim().ToUpper();

        // 1. Verificar comandos proibidos
        foreach (var comando in ComandosProibidos)
        {
            if (sql.Contains(comando))
            {
                _logger.LogWarning("SQL rejeitado: contém comando proibido '{Comando}'", comando);
                return (false, $"Comando '{comando}' não permitido");
            }
        }

        // 2. Deve começar com SELECT
        if (!sql.StartsWith("SELECT"))
        {
            _logger.LogWarning("SQL rejeitado: não inicia com SELECT");
            return (false, "Apenas queries SELECT são permitidas");
        }

        // 3. Verificar whitelist de tabelas
        var erroTabelas = ValidarTabelasUsadas(sql);
        if (erroTabelas != null)
        {
            return (false, erroTabelas);
        }

        // 4. Verificar sintaxe básica
        if (!ValidarSintaxeBasica(sql))
        {
            return (false, "SQL com sintaxe inválida");
        }

        _logger.LogInformation("SQL validado com sucesso");
        return (true, null);
    }

    private string? ValidarTabelasUsadas(string sql)
    {
        // Extrai nomes de tabelas do FROM e JOIN
        var tabelasUsadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Procura padrões: FROM tabela, JOIN tabela
        var palavrasChave = new[] { "FROM", "JOIN" };
        foreach (var palavra in palavrasChave)
        {
            var index = 0;
            while ((index = sql.IndexOf(palavra, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                index += palavra.Length;

                // Pula espaços
                while (index < sql.Length && char.IsWhiteSpace(sql[index]))
                    index++;

                // Captura nome da tabela (até espaço, vírgula ou parêntese)
                var startIndex = index;
                while (index < sql.Length &&
                       !char.IsWhiteSpace(sql[index]) &&
                       sql[index] != ',' &&
                       sql[index] != '(' &&
                       sql[index] != ')')
                {
                    index++;
                }

                if (index > startIndex)
                {
                    var tabela = sql.Substring(startIndex, index - startIndex);
                    tabelasUsadas.Add(tabela);
                }
            }
        }

        // Verifica whitelist
        foreach (var tabela in tabelasUsadas)
        {
            if (!TabelasPermitidas.Contains(tabela))
            {
                _logger.LogWarning("Tabela '{Tabela}' não está na whitelist", tabela);
                return $"Tabela '{tabela}' não autorizada";
            }
        }

        return null;
    }

    private bool ValidarSintaxeBasica(string sql)
    {
        // Parênteses balanceados
        var countAbre = sql.Count(c => c == '(');
        var countFecha = sql.Count(c => c == ')');
        if (countAbre != countFecha)
        {
            _logger.LogWarning("SQL com parênteses desbalanceados");
            return false;
        }

        // Não contém ponto-e-vírgula múltiplo (evita multiple statements)
        if (sql.Split(';').Length > 2)
        {
            _logger.LogWarning("SQL com múltiplos statements (;)");
            return false;
        }

        return true;
    }
}
```

---

### **Fase 4: Handler (Dia 8-9)**

#### 4.1 NaturalQueryCommandHandler

**Arquivo:** `MetaQuery.Domain/Commands/Handlers/NaturalQueryCommandHandler.cs`

```csharp
using MediatR;
using MetaQuery.Domain.Commands.NaturalQuery;
using MetaQuery.Domain.DomainServices;
using MetaQuery.Domain.Services;
using MetaQuery.Domain.Interfaces;
using MetaQuery.Domain.Entities;
using MetaQuery.Infra.Externals.OpenAI;
using MetaQuery.Infra.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MetaQuery.Domain.Commands.Handlers;

/// <summary>
/// Handler para processar consultas em linguagem natural
/// </summary>
public class NaturalQueryCommandHandler : IRequestHandler<NaturalQueryCommand, NaturalQueryResult>
{
    private readonly IADataCatalogService _catalogService;
    private readonly IAQueryGeneratorService _iaService;
    private readonly SQLValidatorService _validatorService;
    private readonly IConsultaDinamicaRepository _consultaRepository;
    private readonly IHistoricoConsultasRepository _historicoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NaturalQueryCommandHandler> _logger;

    public NaturalQueryCommandHandler(
        IADataCatalogService catalogService,
        IAQueryGeneratorService iaService,
        SQLValidatorService validatorService,
        IConsultaDinamicaRepository consultaRepository,
        IHistoricoConsultasRepository historicoRepository,
        IUnitOfWork unitOfWork,
        ILogger<NaturalQueryCommandHandler> logger)
    {
        _catalogService = catalogService;
        _iaService = iaService;
        _validatorService = validatorService;
        _consultaRepository = consultaRepository;
        _historicoRepository = historicoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<NaturalQueryResult> Handle(NaturalQueryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Processando consulta natural: {Pergunta}", request.Pergunta);

        HistoricoConsulta? historico = null;
        string sqlGerado = string.Empty;
        int tokensUsados = 0;

        try
        {
            // 1. Gerar contexto do catálogo
            var contexto = await _catalogService.GerarContextoCatalogoAsync();
            var exemplos = _catalogService.GerarExemplosQueries();
            var contextoCompleto = contexto + "\n" + exemplos;

            // 2. Chamar IA para gerar SQL
            (sqlGerado, tokensUsados) = await _iaService.GerarSQLAsync(
                request.Pergunta,
                contextoCompleto
            );

            _logger.LogInformation("SQL gerado: {SQL}", sqlGerado);

            // 3. Validar SQL
            var (valido, erroValidacao) = _validatorService.ValidarSQL(sqlGerado);
            if (!valido)
            {
                throw new InvalidOperationException($"SQL inválido: {erroValidacao}");
            }

            // 4. Executar SQL
            var query = new SqlKata.Query().FromRaw($"({sqlGerado})").As("resultado");
            var dados = await _consultaRepository.ExecutarQueryAsync(query);
            var totalLinhas = dados.Count();

            stopwatch.Stop();

            // 5. Salvar histórico (se habilitado)
            if (request.SalvarHistorico)
            {
                _unitOfWork.BeginTransaction();

                historico = HistoricoConsulta.Criar(
                    request.Pergunta,
                    sqlGerado,
                    request.ModeloIA,
                    request.Usuario
                );

                historico.RegistrarSucesso(
                    totalLinhas,
                    (int)stopwatch.ElapsedMilliseconds,
                    tokensUsados
                );

                await _historicoRepository.CriarAsync(historico);
                _unitOfWork.Commit();
            }

            // 6. Retornar resultado
            return new NaturalQueryResult
            {
                PerguntaOriginal = request.Pergunta,
                SqlGerado = sqlGerado,
                Dados = dados,
                TotalLinhas = totalLinhas,
                TempoExecucao = $"{stopwatch.ElapsedMilliseconds}ms",
                Metadados = new MetadadosResposta
                {
                    ModeloIA = request.ModeloIA ?? "gpt-4",
                    Confianca = 0.95, // TODO: calcular baseado na resposta da IA
                    TokensUsados = tokensUsados
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar consulta natural");

            // Salvar erro no histórico
            if (request.SalvarHistorico && historico != null)
            {
                try
                {
                    _unitOfWork.BeginTransaction();
                    historico ??= HistoricoConsulta.Criar(request.Pergunta, sqlGerado);
                    historico.RegistrarErro(ex.Message);
                    await _historicoRepository.CriarAsync(historico);
                    _unitOfWork.Commit();
                }
                catch (Exception exHistorico)
                {
                    _logger.LogError(exHistorico, "Erro ao salvar histórico de erro");
                    _unitOfWork.Rollback();
                }
            }

            throw;
        }
    }
}
```

---

### **Fase 5: API Layer (Dia 10)**

#### 5.1 Controller

**Arquivo:** `MetaQuery.Api/Controllers/NaturalQueryController.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MetaQuery.Domain.Commands.NaturalQuery;

namespace MetaQuery.Api.Controllers;

/// <summary>
/// Controller para consultas em linguagem natural
/// </summary>
[ApiController]
[Route("api/consulta/natural")]
public class NaturalQueryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NaturalQueryController> _logger;

    public NaturalQueryController(IMediator mediator, ILogger<NaturalQueryController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Processa consulta em linguagem natural e retorna resultados
    /// </summary>
    /// <param name="command">Command com a pergunta do usuário</param>
    /// <returns>Resultados da consulta + SQL gerado</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessarConsulta([FromBody] NaturalQueryCommand command)
    {
        var resultado = await _mediator.Send(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém histórico de consultas
    /// </summary>
    [HttpGet("historico")]
    public async Task<IActionResult> ObterHistorico(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        // TODO: Implementar ObterHistoricoQuery
        return Ok(new { mensagem = "Em desenvolvimento" });
    }
}
```

---

### **Fase 6: Repository (Dia 11)**

#### 6.1 HistoricoConsultasRepository

**Arquivo:** `MetaQuery.Infra.Data/Repositories/HistoricoConsultasRepository.cs`

```csharp
using Dapper;
using MetaQuery.Domain.Entities;
using System.Data;

namespace MetaQuery.Infra.Data.Repositories;

public interface IHistoricoConsultasRepository
{
    Task<int> CriarAsync(HistoricoConsulta historico);
    Task<HistoricoConsulta?> ObterPorIdAsync(int id);
    Task<IEnumerable<HistoricoConsulta>> ObterUltimosAsync(int quantidade = 50);
}

public class HistoricoConsultasRepository : IHistoricoConsultasRepository
{
    private readonly IDbConnection _connection;

    public HistoricoConsultasRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<int> CriarAsync(HistoricoConsulta historico)
    {
        var sql = @"
            INSERT INTO HISTORICO_CONSULTAS (
                PERGUNTA_ORIGINAL, SQL_GERADO, SUCESSO, ERRO,
                TOTAL_LINHAS, TEMPO_EXECUCAO_MS, MODELO_IA,
                TOKENS_USADOS, USUARIO
            ) VALUES (
                :PerguntaOriginal, :SqlGerado, :Sucesso, :Erro,
                :TotalLinhas, :TempoExecucaoMs, :ModeloIA,
                :TokensUsados, :Usuario
            ) RETURNING ID INTO :Id";

        var parameters = new
        {
            historico.PerguntaOriginal,
            historico.SqlGerado,
            Sucesso = historico.Sucesso ? 1 : 0,
            historico.Erro,
            historico.TotalLinhas,
            historico.TempoExecucaoMs,
            historico.ModeloIA,
            historico.TokensUsados,
            historico.Usuario
        };

        var id = await _connection.QuerySingleAsync<int>(sql, parameters);
        return id;
    }

    public async Task<HistoricoConsulta?> ObterPorIdAsync(int id)
    {
        var sql = "SELECT * FROM HISTORICO_CONSULTAS WHERE ID = :Id";
        return await _connection.QuerySingleOrDefaultAsync<HistoricoConsulta>(sql, new { Id = id });
    }

    public async Task<IEnumerable<HistoricoConsulta>> ObterUltimosAsync(int quantidade = 50)
    {
        var sql = @"
            SELECT * FROM (
                SELECT * FROM HISTORICO_CONSULTAS
                ORDER BY DATA_CRIACAO DESC
            ) WHERE ROWNUM <= :Quantidade";

        return await _connection.QueryAsync<HistoricoConsulta>(sql, new { Quantidade = quantidade });
    }
}
```

---

## 🔒 Segurança e Validações

### Camadas de Segurança

#### 1. Validação de Input
```csharp
public class NaturalQueryCommandValidator : AbstractValidator<NaturalQueryCommand>
{
    public NaturalQueryCommandValidator()
    {
        RuleFor(x => x.Pergunta)
            .NotEmpty().WithMessage("Pergunta não pode ser vazia")
            .MaximumLength(1000).WithMessage("Pergunta muito longa (máximo 1000 caracteres)");

        RuleFor(x => x.Usuario)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Usuario));
    }
}
```

#### 2. Whitelist de Tabelas Rígida
- Apenas

 tabelas explicitamente permitidas
- Bloqueio de tabelas de sistema
- Bloqueio de views sensíveis

#### 3. Bloqueio de Comandos Perigosos
- DELETE, DROP, TRUNCATE, ALTER bloqueados
- UPDATE, INSERT bloqueados
- EXEC, CALL bloqueados

#### 4. Rate Limiting
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("natural-query", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10; // 10 consultas por minuto
    });
});

// Controller
[EnableRateLimiting("natural-query")]
public class NaturalQueryController : ControllerBase { }
```

#### 5. Sanitização de SQL
- Remove comentários SQL (-- e /* */)
- Remove múltiplos statements (;)
- Valida parênteses balanceados

---

## 🧪 Testes

### Testes Unitários

**Arquivo:** `MetaQuery.Tests/Commands/Handlers/NaturalQueryCommandHandlerTests.cs`

```csharp
[Fact]
public async Task Handle_PerguntaValida_DeveRetornarResultado()
{
    // Arrange
    var command = new NaturalQueryCommand
    {
        Pergunta = "Mostre os clientes",
        SalvarHistorico = false
    };

    _catalogServiceMock.Setup(x => x.GerarContextoCatalogoAsync())
        .ReturnsAsync("contexto...");

    _iaServiceMock.Setup(x => x.GerarSQLAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(("SELECT * FROM CLIENTES", 100));

    _validatorMock.Setup(x => x.ValidarSQL(It.IsAny<string>()))
        .Returns((true, null));

    _consultaRepositoryMock.Setup(x => x.ExecutarQueryAsync(It.IsAny<Query>()))
        .ReturnsAsync(new List<dynamic> { new { Id = 1, Nome = "João" } });

    // Act
    var resultado = await _handler.Handle(command, CancellationToken.None);

    // Assert
    resultado.SqlGerado.Should().NotBeNullOrEmpty();
    resultado.TotalLinhas.Should().BeGreaterThan(0);
}

[Fact]
public async Task Handle_SQLInvalido_DeveLancarExcecao()
{
    // Arrange
    var command = new NaturalQueryCommand { Pergunta = "DELETE FROM CLIENTES" };

    _catalogServiceMock.Setup(x => x.GerarContextoCatalogoAsync())
        .ReturnsAsync("contexto...");

    _iaServiceMock.Setup(x => x.GerarSQLAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(("DELETE FROM CLIENTES", 50));

    _validatorMock.Setup(x => x.ValidarSQL(It.IsAny<string>()))
        .Returns((false, "DELETE não permitido"));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

---

## 📚 Documentação para o Usuário

### README para Usuários

```markdown
# 🤖 Consultas em Linguagem Natural

## Como Usar

### 1. Fazer uma Pergunta
```bash
POST /api/consulta/natural
{
  "pergunta": "Mostre os pedidos do último mês",
  "salvarHistorico": true
}
```

### 2. Exemplos de Perguntas

✅ **BOM:**
- "Mostre todos os clientes ativos"
- "Pedidos do cliente João dos últimos 30 dias"
- "Produtos mais caros que R$ 100"
- "Quantos pedidos foram feitos em novembro?"

❌ **RUIM:**
- Perguntas muito vagas: "Mostre dados"
- Perguntas sobre tabelas não mapeadas
- Comandos de modificação: "Delete cliente 5"

### 3. Ver Histórico
```bash
GET /api/consulta/natural/historico?pagina=1&tamanhoPagina=20
```

## Limitações

- Máximo 10 consultas por minuto
- Pergunta máxima: 1000 caracteres
- Apenas queries SELECT (leitura)
- Apenas tabelas mapeadas nos metadados
```

---

## 📊 Estimativa de Tempo

| Fase | Atividade | Tempo Estimado |
|------|-----------|----------------|
| 1 | Infraestrutura (tabela, configs) | 4 horas |
| 2 | Domain Layer (entities, commands) | 6 horas |
| 3 | Services (catalog, IA, validator) | 12 horas |
| 4 | CommandHandler | 6 horas |
| 5 | API Controller | 2 horas |
| 6 | Repository | 3 horas |
| 7 | Testes Unitários | 8 horas |
| 8 | Integração e Ajustes | 6 horas |
| 9 | Documentação | 3 horas |
| **TOTAL** | **~50 horas** (6-7 dias úteis) |

---

## ✅ Checklist de Implementação

### Setup
- [ ] Criar tabela HISTORICO_CONSULTAS
- [ ] Instalar Azure.AI.OpenAI
- [ ] Configurar OpenAI API Key (user secrets)
- [ ] Adicionar settings no appsettings.json

### Domain Layer
- [ ] HistoricoConsulta entity
- [ ] NaturalQueryCommand
- [ ] NaturalQueryResult
- [ ] NaturalQueryCommandValidator

### Services
- [ ] IADataCatalogService
- [ ] IAQueryGeneratorService (OpenAI)
- [ ] SQLValidatorService

### Handler
- [ ] NaturalQueryCommandHandler
- [ ] Integração com UnitOfWork
- [ ] Tratamento de erros

### Infrastructure
- [ ] HistoricoConsultasRepository
- [ ] Registrar serviços no DI
- [ ] Configurar OpenAISettings

### API
- [ ] NaturalQueryController
- [ ] Rate limiting
- [ ] Documentação Swagger

### Testes
- [ ] NaturalQueryCommandHandlerTests
- [ ] SQLValidatorServiceTests
- [ ] Integration tests

### Verificação
- [ ] Teste com pergunta simples
- [ ] Teste com pergunta complexa (JOINs)
- [ ] Teste com SQL inválido
- [ ] Teste de rate limiting
- [ ] Teste de histórico

---

**Próximo passo:** Começar pela Fase 1 (Infraestrutura)! 🚀
