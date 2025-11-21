# ⏭️ Próximos Passos

## 🏢 MIGRAÇÃO PARA PADRÃO HERVAL (NOVA PRIORIDADE)

**Contexto:** A empresa utiliza CQRS + MediatR como padrão arquitetural. Este projeto precisa ser alinhado para facilitar manutenção e integração com outros sistemas.

**Meta:** Migrar arquitetura atual (Clean Architecture + DDD) para **Clean Architecture + DDD + CQRS + MediatR** seguindo padrões da Herval.

### 🎯 Fase CQRS (Prioridade Máxima - 3 semanas)

---

### 1. 🟡 SEMANA 1: MediatR + CQRS Base (PARCIALMENTE CONCLUÍDO)

**Tempo estimado:** 5 dias
**Complexidade:** ⭐⭐⭐⭐
**Status:** Controllers já simplificados conforme padrão Herval ✅

#### Dia 1: Setup e Pacotes
- [x] Instalar `MediatR` (13.1.0) no QueryBuilder.Domain ✅
- [x] Instalar `MediatR.Extensions.Microsoft.DependencyInjection` no IoC ✅
- [x] Instalar `FluentValidation` (12.1.0) ✅
- [x] Instalar `FluentValidation.DependencyInjectionExtensions` ✅

**Comandos:**
```powershell
dotnet add src/QueryBuilder.Domain/QueryBuilder.Domain.csproj package MediatR
dotnet add src/QueryBuilder.Infra.CrossCutting.IoC/QueryBuilder.Infra.CrossCutting.IoC.csproj package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add src/QueryBuilder.Domain/QueryBuilder.Domain.csproj package FluentValidation.DependencyInjectionExtensions
```

#### Dia 2-3: Estrutura de Queries
- [ ] Criar `src/QueryBuilder.Domain/Queries/`
- [ ] Criar `src/QueryBuilder.Domain/Queries/Handlers/`
- [ ] Criar `src/QueryBuilder.Domain/Queries/ConsultaDinamica/`

**Query Pattern:**
```csharp
// ConsultaDinamicaQuery.cs
public record ConsultaDinamicaQuery(
    string Tabela,
    bool IncluirJoins = false,
    int Profundidade = 1
) : IRequest<ConsultaDinamicaResult>;

// ConsultaDinamicaResult.cs
public record ConsultaDinamicaResult(
    string Tabela,
    int TotalRegistros,
    IEnumerable<dynamic> Dados,
    string SqlGerado
);

// ConsultaDinamicaQueryHandler.cs
public class ConsultaDinamicaQueryHandler
    : IRequestHandler<ConsultaDinamicaQuery, ConsultaDinamicaResult>
{
    private readonly IQueryBuilderService _queryBuilder;
    private readonly IConsultaDinamicaRepository _repository;
    private readonly ILogger<ConsultaDinamicaQueryHandler> _logger;

    public async Task<ConsultaDinamicaResult> Handle(
        ConsultaDinamicaQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Executando consulta dinâmica - Tabela: {Tabela}",
            request.Tabela);

        // Gerar query
        var query = await _queryBuilder.MontarQueryAsync(
            request.Tabela,
            request.IncluirJoins,
            request.Profundidade);

        // Executar
        var dados = await _repository.ExecutarQueryAsync(query);
        var sql = _queryBuilder.CompilarQuery(query);

        return new ConsultaDinamicaResult(
            request.Tabela,
            dados.Count(),
            dados,
            sql.Sql
        );
    }
}
```

#### Dia 4: Queries Adicionais
- [ ] `ObterMetadadosQuery` + Handler
- [ ] `ObterMetadadoPorIdQuery` + Handler
- [ ] `ObterMetadadoPorTabelaQuery` + Handler
- [ ] `ListarTabelasDisponiveisQuery` + Handler

#### Dia 5: Refatorar Controllers ✅ CONCLUÍDO
- [x] Injetar `IMediator` nos controllers (ou IMediator + repository conforme necessário) ✅
- [x] Simplificar controllers seguindo padrão Herval ✅
- [x] Remover INotificationContext, ILogger, try-catch desnecessários ✅
- [x] Controllers reduzidos de 592 para 213 linhas (-64%) ✅
  - MetadadosController: 323 → 101 linhas
  - ConsultaDinamicaController: 93 → 45 linhas
  - QueryBuilderTestController: 176 → 67 linhas

**Controller Refatorado:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ConsultaDinamicaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConsultaDinamicaController> _logger;

    public ConsultaDinamicaController(
        IMediator mediator,
        ILogger<ConsultaDinamicaController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{tabela}")]
    public async Task<IActionResult> ConsultarTabela(
        string tabela,
        [FromQuery] bool incluirJoins = false,
        [FromQuery] int profundidade = 1)
    {
        var query = new ConsultaDinamicaQuery(tabela, incluirJoins, profundidade);
        var resultado = await _mediator.Send(query);

        return Ok(resultado);
    }
}
```

---

### 2. 🔴 SEMANA 2: Notification Pattern + Validations

**Tempo estimado:** 5 dias
**Complexidade:** ⭐⭐⭐⭐

#### Dia 1-2: Notification Context
- [ ] Criar `src/QueryBuilder.Domain/Notifications/`
- [ ] Criar interface `INotificationContext`
- [ ] Implementar `NotificationContext`
- [ ] Criar `Notification` record

**Implementação:**
```csharp
// INotificationContext.cs
public interface INotificationContext
{
    void AddNotification(string key, string message);
    void AddNotifications(IEnumerable<Notification> notifications);
    bool HasNotifications { get; }
    IReadOnlyCollection<Notification> Notifications { get; }
    void Clear();
}

// Notification.cs
public record Notification(string Key, string Message);

// NotificationContext.cs
public class NotificationContext : INotificationContext
{
    private readonly List<Notification> _notifications = new();

    public void AddNotification(string key, string message)
    {
        _notifications.Add(new Notification(key, message));
    }

    public void AddNotifications(IEnumerable<Notification> notifications)
    {
        _notifications.AddRange(notifications);
    }

    public bool HasNotifications => _notifications.Any();

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    public void Clear() => _notifications.Clear();
}
```

#### Dia 3: FluentValidation Validators
- [ ] Criar `ConsultaDinamicaQueryValidator`
- [ ] Criar `CriarMetadadoCommandValidator`
- [ ] Configurar assembly scanning de validadores

**Validator Example:**
```csharp
public class ConsultaDinamicaQueryValidator : AbstractValidator<ConsultaDinamicaQuery>
{
    private static readonly string[] TabelasPermitidas =
    {
        "CLIENTES", "PEDIDOS", "PRODUTOS", "CATEGORIAS", "ITENS_PEDIDO", "ENDERECOS"
    };

    public ConsultaDinamicaQueryValidator()
    {
        RuleFor(x => x.Tabela)
            .NotEmpty().WithMessage("Tabela é obrigatória")
            .Must(tabela => TabelasPermitidas.Contains(tabela.ToUpper()))
            .WithMessage("Tabela não autorizada");

        RuleFor(x => x.Profundidade)
            .InclusiveBetween(1, 3)
            .WithMessage("Profundidade deve estar entre 1 e 3");
    }
}
```

#### Dia 4-5: Pipeline Behaviors
- [ ] Criar `ValidationBehavior<TRequest, TResponse>`
- [ ] Criar `LoggingBehavior<TRequest, TResponse>`
- [ ] Registrar behaviors no DI

**ValidationBehavior:**
```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly INotificationContext _notificationContext;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            foreach (var failure in failures)
            {
                _notificationContext.AddNotification(
                    failure.PropertyName,
                    failure.ErrorMessage);
            }

            return default!; // Retorna default se validação falhar
        }

        return await next();
    }
}
```

---

### 3. 🟡 SEMANA 3: Commands + Unit of Work

**Tempo estimado:** 5 dias
**Complexidade:** ⭐⭐⭐

#### Dia 1-2: Commands Structure
- [ ] Criar `src/QueryBuilder.Domain/Commands/`
- [ ] Criar `src/QueryBuilder.Domain/Commands/Handlers/`
- [ ] Criar `src/QueryBuilder.Domain/Commands/Metadados/`

**Command Pattern:**
```csharp
// CriarMetadadoCommand.cs
public record CriarMetadadoCommand(
    string Tabela,
    string CamposDisponiveis,
    string ChavePk,
    string? VinculoEntreTabela = null,
    string? Descricao = null
) : IRequest<int>; // Retorna ID do metadado criado

// CriarMetadadoCommandHandler.cs
public class CriarMetadadoCommandHandler
    : IRequestHandler<CriarMetadadoCommand, int>
{
    private readonly IMetadadosRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly INotificationContext _notificationContext;
    private readonly ILogger<CriarMetadadoCommandHandler> _logger;

    public async Task<int> Handle(
        CriarMetadadoCommand request,
        CancellationToken ct)
    {
        // Criar entidade de domínio
        var metadado = TabelaDinamica.Criar(
            request.Tabela,
            request.CamposDisponiveis,
            request.ChavePk,
            request.VinculoEntreTabela,
            request.Descricao
        );

        // Validações do domínio já estão na entidade
        // Se tiver erro, exceção é lançada

        // Persistir
        var id = await _repository.CriarAsync(metadado);

        // Commit transação
        await _uow.CommitAsync();

        _logger.LogInformation(
            "Metadado criado - ID: {Id}, Tabela: {Tabela}",
            id, metadado.Tabela);

        return id;
    }
}
```

#### Dia 3: Unit of Work
- [ ] Criar `src/QueryBuilder.Domain/Interfaces/IUnitOfWork.cs`
- [ ] Implementar `src/QueryBuilder.Infra.Data/UnitOfWork.cs`

**UnitOfWork:**
```csharp
// IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<bool> CommitAsync(CancellationToken ct = default);
    void Rollback();
}

// UnitOfWork.cs (para Dapper com Oracle)
public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;

    public UnitOfWork(IDbConnection connection)
    {
        _connection = connection;
        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _transaction = _connection.BeginTransaction();
    }

    public async Task<bool> CommitAsync(CancellationToken ct = default)
    {
        try
        {
            _transaction?.Commit();
            return true;
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }
}
```

#### Dia 4-5: Refatorar Repositories
- [ ] Adicionar `IUnitOfWork` nos repositories
- [ ] Remover commits automáticos
- [ ] Deixar commit para handlers

---

### 4. 📋 Checklist Final - Padrão Herval Completo

#### CQRS ✅
- [ ] MediatR instalado e configurado
- [ ] Queries criadas (5+)
- [ ] QueryHandlers implementados (5+)
- [ ] Commands criados (3+)
- [ ] CommandHandlers implementados (3+)
- [ ] Controllers refatorados para usar IMediator
- [ ] Sem injeção direta de repositories em controllers

#### Notification Pattern ✅
- [ ] INotificationContext implementado
- [ ] NotificationContext registrado no DI
- [ ] Handlers usando NotificationContext
- [ ] Exceções substituídas por notificações (onde adequado)

#### FluentValidation ✅
- [ ] Validators criados para Queries/Commands
- [ ] ValidationBehavior implementado
- [ ] Assembly scanning configurado
- [ ] Pipeline de validação automático

#### Unit of Work ✅
- [ ] IUnitOfWork interface criada
- [ ] UnitOfWork implementado
- [ ] Handlers usando CommitAsync()
- [ ] Repositories sem commit automático

#### Pipeline Behaviors ✅
- [ ] ValidationBehavior registrado
- [ ] LoggingBehavior registrado
- [ ] TransactionBehavior registrado (opcional)

#### Dependency Injection ✅
- [ ] MediatR registrado com Assembly scanning
- [ ] Validators registrados automaticamente
- [ ] Behaviors registrados na ordem correta
- [ ] NotificationContext como Scoped
- [ ] UnitOfWork como Scoped

---

## ✅ CONCLUÍDO RECENTEMENTE

### Organização de Código (Padrão Herval) ⭐

**Completado:** 20 de Novembro de 2025

#### Controllers Simplificados
- ✅ MetadadosController: 323 → 101 linhas (-68%)
- ✅ ConsultaDinamicaController: 93 → 45 linhas (-52%)
- ✅ QueryBuilderTestController: 176 → 67 linhas (-62%)
- ✅ Removido: INotificationContext, ILogger manuais, try-catch desnecessários
- ✅ Padrão: Controllers injetam apenas IMediator (+ repository se necessário)
- ✅ Retornos diretos com operadores ternários

#### Interfaces Separadas
- ✅ IRepositories.cs (1 arquivo monolítico) → 5 arquivos individuais
- ✅ Estrutura organizada:
  ```
  Interfaces/
  ├── Repositories/
  │   ├── IMetadadosRepository.cs
  │   └── IConsultaDinamicaRepository.cs
  ├── IQueryBuilderService.cs
  ├── IIADataCatalogService.cs
  └── IValidacaoMetadadosService.cs
  ```
- ✅ SRP (Single Responsibility Principle) aplicado
- ✅ Melhor navegação e manutenção

#### Rotas Limpas
- ✅ Removidas rotas duplicadas (tabelas-conhecidas, extra tabelas-disponiveis)
- ✅ Rotas de teste mantidas apenas em QueryBuilderTestController
- ✅ Rotas públicas em ConsultaDinamicaController (consulta banco diretamente)

#### Dados de Teste Expandidos
- ✅ Tabela PAGAMENTOS criada (10 registros)
- ✅ FK para PEDIDOS implementada
- ✅ Múltiplos cenários: CREDITO, DEBITO, PIX, BOLETO, DINHEIRO
- ✅ Status variados: PENDENTE, APROVADO, RECUSADO, ESTORNADO
- ✅ Metadados atualizados em TABELA_DINAMICA
- ✅ Suporte a FK composta documentado (formato: TABELA:FK1+FK2:PK1+PK2)

#### Documentação
- ✅ CHANGELOG.md atualizado com versões 0.5.2, 0.5.3, 0.5.4
- ✅ Todas as mudanças documentadas

---

## 🎯 Prioridades Imediatas (Esta Semana)

### 1. 🔴 PRIORIDADE MÁXIMA: QueryBuilderService

**Por que é prioritário:**
- É o coração do sistema
- Sem ele, não há geração dinâmica de queries
- Bloqueia todos os outros desenvolvimentos

**Tempo estimado:** 5-7 dias
**Complexidade:** ⭐⭐⭐⭐

#### Checklist de Implementação

**Dia 1-2: Estrutura Básica**
- [ ] Criar arquivo `src/QueryBuilder.Domain/Services/QueryBuilderService.cs`
- [ ] Implementar interface `IQueryBuilderService`
- [ ] Injetar `IMetadadosRepository` no construtor
- [ ] Criar método base `MontarQueryAsync(string nomeTabela)`

**Código inicial:**
```csharp
public class QueryBuilderService : IQueryBuilderService
{
    private readonly IMetadadosRepository _metadadosRepository;
    private readonly OracleCompiler _compiler;

    public QueryBuilderService(IMetadadosRepository metadadosRepository)
    {
        _metadadosRepository = metadadosRepository;
        _compiler = new OracleCompiler();
    }

    public async Task<Query> MontarQueryAsync(string nomeTabela, bool incluirJoins = false)
    {
        // TODO: Implementar
    }
}
```

**Dia 3-4: Lógica de Geração de Queries**
- [ ] Buscar metadados da tabela
- [ ] Parsear campos disponíveis
- [ ] Criar query base com SELECT
- [ ] Implementar lógica de JOINs se `incluirJoins = true`
- [ ] Parsear vínculos entre tabelas

**Lógica de parsing de vínculos:**
```csharp
private List<(string TabelaDestino, string CampoFK, string CampoPK)> ParseVinculos(string vinculo)
{
    // Formato: "PEDIDOS:ID_CLIENTE:ID;ENDERECOS:ID_CLIENTE:ID"
    var vinculos = new List<(string, string, string)>();

    if (string.IsNullOrWhiteSpace(vinculo))
        return vinculos;

    foreach (var v in vinculo.Split(';'))
    {
        var partes = v.Split(':');
        if (partes.Length == 3)
        {
            vinculos.Add((partes[0].Trim(), partes[1].Trim(), partes[2].Trim()));
        }
    }

    return vinculos;
}
```

**Dia 5: JOINs Recursivos**
- [ ] Implementar profundidade de JOINs
- [ ] Prevenção de loops infinitos
- [ ] HashSet de tabelas já processadas
- [ ] Limite de profundidade configurável

**Dia 6-7: Testes e Refinamento**
- [ ] Criar testes unitários
- [ ] Testar com dados reais
- [ ] Validar SQL gerado
- [ ] Documentar uso

---

### 2. 🟡 ConsultaDinamicaRepository

**Tempo estimado:** 2-3 dias
**Complexidade:** ⭐⭐⭐

#### Checklist
- [ ] Criar `src/QueryBuilder.Infra.Data/Repositories/ConsultaDinamicaRepository.cs`
- [ ] Implementar `ExecutarQueryAsync(Query query)`
- [ ] Mapeamento dinâmico com Dapper
- [ ] Tratamento de timeout
- [ ] Tratamento de erros Oracle
- [ ] Logging de queries executadas

**Código base:**
```csharp
public class ConsultaDinamicaRepository : IConsultaDinamicaRepository
{
    private readonly IDbConnection _connection;
    private readonly OracleCompiler _compiler;
    private readonly ILogger<ConsultaDinamicaRepository> _logger;

    public async Task<IEnumerable<dynamic>> ExecutarQueryAsync(Query query)
    {
        var compiled = _compiler.Compile(query);

        _logger.LogInformation("Executando query: {Sql}", compiled.Sql);

        try
        {
            return await _connection.QueryAsync<dynamic>(
                compiled.Sql,
                compiled.NamedBindings,
                commandTimeout: 30
            );
        }
        catch (OracleException ex)
        {
            _logger.LogError(ex, "Erro ao executar query");
            throw;
        }
    }
}
```

---

### 3. 🟡 ConsultaDinamicaController

**Tempo estimado:** 2 dias
**Complexidade:** ⭐⭐

#### Checklist
- [ ] Criar `src/QueryBuilder.Api/Controllers/ConsultaDinamicaController.cs`
- [ ] Endpoint GET `/api/consulta/{tabela}`
- [ ] Validação de nome de tabela (WhiteList)
- [ ] Injetar QueryBuilderService
- [ ] Injetar ConsultaDinamicaRepository
- [ ] Tratamento de erros
- [ ] Documentação Swagger

**Código base:**
```csharp
[ApiController]
[Route("api/consulta")]
public class ConsultaDinamicaController : ControllerBase
{
    private readonly IQueryBuilderService _queryBuilder;
    private readonly IConsultaDinamicaRepository _repository;
    private readonly ILogger<ConsultaDinamicaController> _logger;

    [HttpGet("{tabela}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarTabela(
        string tabela,
        [FromQuery] bool incluirJoins = false)
    {
        try
        {
            // Validar tabela permitida
            if (!TabelaPermitida(tabela))
                return BadRequest(new { Erro = "Tabela não autorizada" });

            // Gerar query
            var query = await _queryBuilder.MontarQueryAsync(tabela, incluirJoins);

            // Executar
            var resultados = await _repository.ExecutarQueryAsync(query);

            return Ok(new
            {
                Tabela = tabela,
                Total = resultados.Count(),
                Dados = resultados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar tabela {Tabela}", tabela);
            return StatusCode(500, new { Erro = "Erro ao executar consulta" });
        }
    }

    private bool TabelaPermitida(string tabela)
    {
        var permitidas = new[] { "CLIENTES", "PEDIDOS", "PRODUTOS", "CATEGORIAS", "ITENS_PEDIDO", "ENDERECOS" };
        return permitidas.Contains(tabela.ToUpper());
    }
}
```

---

### 4. 🟢 Registrar no DI Container

**Tempo estimado:** 30 minutos
**Complexidade:** ⭐

#### Checklist
- [ ] Abrir `src/QueryBuilder.Infra.CrossCutting.IoC/DependencyInjection.cs`
- [ ] Registrar `IQueryBuilderService` → `QueryBuilderService`
- [ ] Registrar `IConsultaDinamicaRepository` → `ConsultaDinamicaRepository`
- [ ] Registrar `OracleCompiler` como Singleton

**Código:**
```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... código existente ...

    // Domain Services
    services.AddScoped<IQueryBuilderService, QueryBuilderService>();

    // Repositories
    services.AddScoped<IMetadadosRepository, MetadadosRepository>();
    services.AddScoped<IConsultaDinamicaRepository, ConsultaDinamicaRepository>();

    // SqlKata
    services.AddSingleton<OracleCompiler>();

    return services;
}
```

---

## 📅 Cronograma Detalhado

### Semana 1 (13/11 - 19/11)
```
Seg: [█████░░░░░] QueryBuilderService - Estrutura básica
Ter: [█████████░] QueryBuilderService - Geração de queries
Qua: [██████████] QueryBuilderService - JOINs recursivos
Qui: [█████░░░░░] ConsultaDinamicaRepository - Implementação
Sex: [██████████] ConsultaDinamicaRepository - Testes
```

### Semana 2 (20/11 - 26/11)
```
Seg: [█████░░░░░] ConsultaDinamicaController - Endpoint básico
Ter: [██████████] ConsultaDinamicaController - Validações
Qua: [█████░░░░░] Testes end-to-end
Qui: [█████████░] Filtros dinâmicos - Implementação
Sex: [██████████] Documentação e refinamento
```

---

## 🧪 Como Testar Cada Componente

### Teste 1: QueryBuilderService (Isolado)

```csharp
// Criar teste unitário
[Fact]
public async Task MontarQuery_DeveGerarQueryComJoins()
{
    // Arrange
    var mockRepo = new Mock<IMetadadosRepository>();
    mockRepo.Setup(r => r.ObterPorNomeTabelaAsync("CLIENTES"))
        .ReturnsAsync(new TabelaDinamica { /* ... */ });

    var service = new QueryBuilderService(mockRepo.Object);

    // Act
    var query = await service.MontarQueryAsync("CLIENTES", incluirJoins: true);
    var compiler = new OracleCompiler();
    var sql = compiler.Compile(query);

    // Assert
    Assert.Contains("JOIN", sql.Sql);
    Assert.Contains("PEDIDOS", sql.Sql);
}
```

### Teste 2: Endpoint Completo (Integração)

```http
### Teste básico
GET http://localhost:5249/api/consulta/CLIENTES
Content-Type: application/json

### Com JOINs
GET http://localhost:5249/api/consulta/CLIENTES?incluirJoins=true
Content-Type: application/json

### Validar SQL gerado (adicionar endpoint debug)
GET http://localhost:5249/api/consulta/CLIENTES/debug?incluirJoins=true
Content-Type: application/json
```

---

## 📝 Checklist de Validação

Antes de considerar a tarefa completa:

### QueryBuilderService ✅
- [ ] Gera query simples (sem JOINs)
- [ ] Gera query com JOINs de 1 nível
- [ ] Gera query com JOINs de 2+ níveis
- [ ] Previne loops infinitos
- [ ] Respeita limite de profundidade
- [ ] Lida com tabelas sem vínculos
- [ ] Lida com vínculos malformados
- [ ] SQL gerado é válido
- [ ] Testes unitários passando

### ConsultaDinamicaRepository ✅
- [ ] Executa query simples
- [ ] Executa query com JOINs
- [ ] Retorna resultados corretos
- [ ] Lida com timeout
- [ ] Lida com erros Oracle
- [ ] Log de queries funciona
- [ ] Parâmetros são sanitizados

### ConsultaDinamicaController ✅
- [ ] Endpoint responde 200
- [ ] Valida tabela permitida
- [ ] Retorna 404 para tabela inexistente
- [ ] Retorna 400 para tabela não autorizada
- [ ] JSON de resposta correto
- [ ] Swagger documentado
- [ ] Tratamento de erros funciona

---

## 🎯 Definição de Pronto (DoD)

Uma tarefa só está completa quando:

✅ Código implementado
✅ Testes unitários criados e passando
✅ Testes de integração funcionando
✅ Código revisado (self-review)
✅ Sem warnings de compilação
✅ Documentação atualizada
✅ Swagger atualizado (se API)
✅ Commit com mensagem clara
✅ Funcionalidade testada manualmente

---

## 🚨 Riscos e Mitigações

### Risco 1: JOINs Recursivos Complexos
**Probabilidade:** Alta
**Impacto:** Alto
**Mitigação:**
- Implementar limite de profundidade
- HashSet de tabelas visitadas
- Testes extensivos com grafos de relacionamentos

### Risco 2: Performance de Queries
**Probabilidade:** Média
**Impacto:** Alto
**Mitigação:**
- Timeout configurável
- Logging de tempo de execução
- Cache de metadados
- Índices no banco

### Risco 3: SQL Injection
**Probabilidade:** Baixa
**Impacto:** Crítico
**Mitigação:**
- Usar SqlKata (já sanitiza)
- WhiteList de tabelas
- Validação rigorosa de entrada
- Testes de segurança

---

## 💡 Dicas de Implementação

### 1. Comece Simples
Implemente primeiro sem JOINs, depois adicione a complexidade.

### 2. Teste Incrementalmente
Não espere terminar tudo para testar. Teste cada método isoladamente.

### 3. Use TDD (Test-Driven Development)
Escreva o teste antes do código. Ajuda a pensar na interface.

### 4. Documente Conforme Desenvolve
Não deixe documentação para depois. Faça enquanto o contexto está fresco.

### 5. Commit Frequentemente
Commits pequenos e frequentes facilitam rollback se necessário.

---

## 📚 Recursos Úteis

### Documentação
- [SqlKata Documentation](https://sqlkata.com/docs)
- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [Oracle .NET Developer Center](https://www.oracle.com/database/technologies/appdev/dotnet.html)

### Referências de Código
- Ver exemplo em `docs/EXEMPLO_08_METADADOS.md`
- Estudar `MetadadosRepository.cs` existente

### Ferramentas
- **SQL Developer** - Para testar queries geradas manualmente
- **Postman/REST Client** - Para testar endpoints
- **Docker logs** - Para debug de erros Oracle

---

## 🎉 Marcos (Milestones)

### Milestone 1: Query Builder Básico ⏳
**Data alvo:** 19/11/2025
- [x] QueryBuilderService implementado
- [ ] Gera queries sem JOINs
- [ ] Testes unitários passando

### Milestone 2: Query Builder com JOINs ⏳
**Data alvo:** 22/11/2025
- [ ] JOINs de 1 nível funcionando
- [ ] JOINs recursivos funcionando
- [ ] Prevenção de loops

### Milestone 3: API Completa ⏳
**Data alvo:** 26/11/2025
- [ ] Endpoint de consulta funcionando
- [ ] Validações implementadas
- [ ] Testes end-to-end passando

### Milestone 4: MVP Funcional 🎯
**Data alvo:** 30/11/2025
- [ ] Filtros dinâmicos
- [ ] Ordenação
- [ ] Paginação
- [ ] Documentação completa

---

## 📞 Quando Pedir Ajuda

Se travar por mais de 2 horas no mesmo problema:
1. Revisar a documentação
2. Buscar exemplos similares
3. Fazer uma pausa (rubber duck debugging)
4. Perguntar em fóruns (.NET, Stack Overflow)

Lembre-se: **Travar faz parte do aprendizado!** 🧠

---

<div align="center">

**⏭️ Um passo de cada vez, mas sempre para frente! 🚀**

[← Voltar ao Índice](00_INDICE.md) | [Ver Roadmap Completo →](05_ROADMAP.md)

</div>
