# 🎯 Implementação CQRS + MediatR - MetaQuery

## 📅 Atualizado: Novembro 24, 2025

---

## 🎉 Objetivo

Migrar projeto para **padrão CQRS + MediatR**, alinhando 100% com os padrões corporativos da empresa Herval:
- **Commands** (escrita) = MediatR + Pipeline (Validation, Logging)
- **Queries** (leitura) = Repositório/DomainService direto (SEM MediatR)

---

## 🏗️ Arquitetura Implementada

### Padrão Herval (CQRS Pragmático)

#### ✅ WRITE Operations (Commands)
```
HTTP POST/PUT/DELETE
    ↓
Controller
    ↓
IMediator.Send(Command)
    ↓
ValidationBehavior (FluentValidation)
    ↓
LoggingBehavior (timing + logs)
    ↓
CommandHandler
    ↓
DomainService (regras de negócio)
    ↓
Repository
    ↓
Database
```

#### ✅ READ Operations (Queries - SEM MediatR)
```
HTTP GET
    ↓
Controller
    ↓
Repository.ObterAsync() ou DomainService.Method()
    ↓
Database
```

**Justificativa:**
Queries simples não precisam do overhead do MediatR. Repository direto é mais performático e mais fácil de entender.

---

## 📦 Pacotes Instalados

```xml
<!-- MetaQuery.Domain.csproj -->
<PackageReference Include="MediatR" Version="13.1.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.0" />

<!-- MetaQuery.Infra.CrossCutting.IoC.csproj -->
<PackageReference Include="MediatR" Version="13.1.0" />
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
```

---

## 📁 Estrutura Final do Projeto

```
MetaQuery.Domain/
├── Commands/                             ← Feature Folders (Padrão Herval)
│   ├── CriarMetadado/
│   │   ├── CriarMetadadoCommand.cs      ✅ class com IRequest<T>
│   │   ├── CriarMetadadoCommandHandler.cs  ✅ IRequestHandler
│   │   └── CriarMetadadoCommandValidator.cs ✅ AbstractValidator
│   ├── AtualizarMetadado/
│   │   ├── AtualizarMetadadoCommand.cs
│   │   ├── AtualizarMetadadoCommandHandler.cs
│   │   └── AtualizarMetadadoCommandValidator.cs
│   └── DesativarMetadado/
│       ├── DesativarMetadadoCommand.cs
│       ├── DesativarMetadadoCommandHandler.cs
│       └── DesativarMetadadoCommandValidator.cs
│
├── Behaviors/                            ← MediatR Behaviors
│   ├── LoggingBehavior.cs               ✅ IPipelineBehavior
│   └── ValidationBehavior.cs            ✅ IPipelineBehavior
│
├── Notifications/                        ← Notification Pattern
│   ├── Notification.cs                  ✅ record (Key, Message)
│   ├── INotificationContext.cs          ✅ Interface
│   ├── NotificationContext.cs           ✅ Implementação
│   └── ConsultaDinamicaResult.cs        ✅ DTO
│
├── DomainServices/                       ← Lógica de Negócio
│   ├── MetadadosDomainService.cs
│   └── ConsultaDinamicaDomainService.cs
│
└── Interfaces/
    └── Repositories/
        ├── IMetadadosRepository.cs      ← Usado direto em GETs
        └── IConsultaDinamicaRepository.cs
```

### ❌ O Que FOI REMOVIDO
```
❌ Queries/                              ← DELETADO (11 arquivos)
    ├── ConsultaDinamicaQuery.cs
    ├── Metadados/
    │   ├── ObterTodosMetadadosQuery.cs
    │   ├── ObterMetadadoPorIdQuery.cs
    │   └── ObterMetadadoPorTabelaQuery.cs
    └── Handlers/
        ├── ConsultaDinamicaQueryHandler.cs
        ├── ObterTodosMetadadosQueryHandler.cs
        ├── ObterMetadadoPorIdQueryHandler.cs
        └── ObterMetadadoPorTabelaQueryHandler.cs

❌ Validators/                           ← Query validators removidos
    ├── ConsultaDinamicaQueryValidator.cs
    ├── ObterMetadadoPorIdQueryValidator.cs
    └──ObterMetadadoPorTabelaQueryValidator.cs
```

---

## 🔧 Componentes Implementados

### 1. **Commands (Feature Folders)**

```csharp
// CriarMetadadoCommand.cs
namespace MetaQuery.Domain.Commands.CriarMetadado;

public record CriarMetadadoCommand : IRequest<int>
{
    public string Tabela { get; init; } = string.Empty;
    public string CamposDisponiveis { get; init; } = string.Empty;
    public string ChavePk { get; init; } = string.Empty;
    public string? VinculoEntreTabela { get; init; }
    public string? DescricaoTabela { get; init; }
    public bool VisivelParaIA { get; init; } = true;
}

// CriarMetadadoCommandHandler.cs
public class CriarMetadadoCommandHandler : IRequestHandler<CriarMetadadoCommand, int>
{
    private readonly IMetadadosDomainService _domainService;
    private readonly INotificationContext _notificationContext;

    public async Task<int> Handle(CriarMetadadoCommand request, CancellationToken ct)
    {
        var id = await _domainService.CriarAsync(request);
        return id;
    }
}

// CriarMetadadoCommandValidator.cs
public class CriarMetadadoCommandValidator : AbstractValidator<CriarMetadadoCommand>
{
    public CriarMetadadoCommandValidator()
    {
        RuleFor(x => x.Tabela)
            .NotEmpty().WithMessage("O nome da tabela é obrigatório")
            .MaximumLength(100)
            .Matches("^[A-Z][A-Z0-9_]*$");

        RuleFor(x => x.CamposDisponiveis)
            .NotEmpty().WithMessage("Os campos disponíveis são obrigatórios");
    }
}
```

**Características:**
- Tudo em uma pasta (Command + Handler + Validator)
- FluentValidation executado automaticamente pelo ValidationBehavior
- Retorna tipos simples (int, bool)

---

### 2. **Controllers - Padrão Herval 100%**

```csharp
[ApiController]
[Route("api/[controller]")]
public class MetadadosController : ControllerBase
{
    private readonly IMediator _mediator;                // ← Para Commands
    private readonly IMetadadosRepository _repository;   // ← Para Queries

    public MetadadosController(
        IMediator mediator,
        IMetadadosRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    // ============ QUERIES (Leitura) - DIRETO ============

    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromQuery] bool apenasAtivos = true)
    {
        var metadados = await _repository.ObterTodosAsync(apenasAtivos); // ✅ DIRETO
        return Ok(new { Total = metadados.Count(), Metadados = metadados });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var metadado = await _repository.ObterPorIdAsync(id); // ✅ DIRETO
        return metadado == null ? NotFound() : Ok(metadado);
    }

    [HttpGet("tabela/{nomeTabela}")]
    public async Task<IActionResult> ObterPorTabela(string nomeTabela)
    {
        var metadado = await _repository.ObterPorNomeTabelaAsync(nomeTabela); // ✅ DIRETO
        return metadado == null ? NotFound() : Ok(metadado);
    }

    // ============ COMMANDS (Escrita) - MEDIATR ============

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarMetadadoCommand command)
    {
        var id = await _mediator.Send(command); // ✅ MEDIATR com pipeline
        return id > 0
            ? CreatedAtAction(nameof(ObterPorId), new { id }, new { id })
            : BadRequest();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarMetadadoCommand command)
    {
        var commandComId = command with { Id = id };
        var sucesso = await _mediator.Send(commandComId);
        return sucesso ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Desativar(int id)
    {
        var sucesso = await _mediator.Send(new DesativarMetadadoCommand(id));
        return sucesso ? Ok() : NotFound();
    }
}
```

```csharp
[ApiController]
[Route("api/[controller]")]
public class ConsultaDinamicaController : ControllerBase
{
    private readonly ConsultaDinamicaDomainService _consultaService; // ← DomainService direto
    private readonly IMetadadosRepository _metadadosRepository;

    // ============ QUERIES (Leitura) - DIRETO ============

    [HttpGet("{tabela}")]
    public async Task<IActionResult> Consultar(
        string tabela,
        [FromQuery] bool incluirJoins = false,
        [FromQuery] int profundidade = 2)
    {
        var resultado = await _consultaService.ConsultarTabelaAsync(
            tabela, incluirJoins, profundidade); // ✅ DIRETO

        return Ok(resultado);
    }

    [HttpGet("tabelas-disponiveis")]
    public async Task<IActionResult> ListarTabelasDisponiveis()
    {
        var metadados = await _metadadosRepository.ObterTodosAsync(apenasAtivos: true);
        var tabelas = metadados.Select(m => m.Tabela).OrderBy(t => t);
        return Ok(new { Total = tabelas.Count(), Tabelas = tabelas });
    }
}
```

**Características:**
- GETs = Repository/DomainService direto
- POST/PUT/DELETE = MediatR com pipeline completo
- Sem try/catch (tratado nos behaviors quando usando MediatR)
- Simplicidade máxima para leituras

---

### 3. **Pipeline Behaviors (Apenas para Commands)**

```csharp
// ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly INotificationContext _notificationContext;

    public async Task<TResponse> Handle(...)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
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
            return default!; // Curto-circuita pipeline
        }

        return await next();
    }
}
```

```csharp
// LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(...)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Iniciando {RequestName}", requestName);

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "{RequestName} executado em {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao executar {RequestName}", requestName);
            throw;
        }
    }
}
```

---

### 4. **Notification Pattern**

```csharp
public record Notification(string Key, string Message);

public interface INotificationContext
{
    void AddNotification(string key, string message);
    void AddNotifications(IEnumerable<Notification> notifications);
    bool HasNotifications { get; }
    IReadOnlyCollection<Notification> Notifications { get; }
    void Clear();
}

public class NotificationContext : INotificationContext
{
    private readonly List<Notification> _notifications = new();

    public void AddNotification(string key, string message)
        => _notifications.Add(new Notification(key, message));

    public void AddNotifications(IEnumerable<Notification> notifications)
        => _notifications.AddRange(notifications);

    public bool HasNotifications => _notifications.Any();

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    public void Clear() => _notifications.Clear();
}
```

---

### 5. **Dependency Injection**

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Notification Context (Scoped - por request)
    services.AddScoped<INotificationContext, NotificationContext>();

    // MediatR com Assembly Scanning
    var domainAssembly = Assembly.Load("MetaQuery.Domain");
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(domainAssembly);

        // Ordem dos behaviors: Logging → Validation → Handler
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    // FluentValidation - Assembly Scanning automático
    services.AddValidatorsFromAssembly(domainAssembly);

    // Repositories (usados direto em GETs)
    services.AddScoped<IMetadadosRepository, MetadadosRepository>();
    services.AddScoped<IConsultaDinamicaRepository, ConsultaDinamicaRepository>();

    // Domain Services (usados direto em GETs)
    services.AddScoped<IMetadadosDomainService, MetadadosDomainService>();
    services.AddScoped<ConsultaDinamicaDomainService>();

    return services;
}
```

---

## 🔄 Fluxo Completo

### Command (POST) - COM MediatR
```
POST /api/metadados
{
  "tabela": "CLIENTES",
  "camposDisponiveis": "ID,NOME,EMAIL"
}

Controller
    ↓ _mediator.Send(CriarMetadadoCommand)
MediatR Pipeline
    ↓ LoggingBehavior (log início + timing)
    ↓ ValidationBehavior (FluentValidation)
        • Tabela NotEmpty? ✅
        • Tabela Matches regex? ✅
    ↓ CriarMetadadoCommandHandler
        • _domainService.CriarAsync()
        • Regras de negócio
        • _repository.CriarAsync()
    ↓ LoggingBehavior (log fim)
Controller
    ↓ return CreatedAtAction(...)

HTTP 201 Created
```

### Query (GET) - SEM MediatR
```
GET /api/metadados

Controller
    ↓ _repository.ObterTodosAsync() [DIRETO!]
Repository
    ↓ Execute SQL with Dapper
Controller
    ↓ return Ok(...)

HTTP 200 OK
```

---

## 📊 Métricas Finais

### Arquivos Removidos
```
❌ 11 arquivos de Queries + QueryHandlers deletados
❌ ~600 linhas de código removidas
```

### Build
```
✅ Compilação: SUCCESS
✅ Testes: 21/21 passando
⏱️  Tempo: 6.3s
⚠️  Avisos: 7 (MediatR version, nullability - não-críticos)
❌ Erros: 0
```

### Conformidade
```
✅ Padrão Herval: 100%
✅ Feature Folders: ✓
✅ FluentValidation: ✓
✅ Behaviors (MediatR): ✓
✅ Queries sem MediatR: ✓
✅ CQRS Pragmático: ✓
```

---

## 🎯 Vantagens do Padrão Herval

### 1. **Simplicidade para Queries**
- Leituras diretas = mais fácil de entender
- Menos camadas = melhor performance
- Código mais conciso

### 2. **Poder para Commands**
- Validação automática (ValidationBehavior)
- Logging automático (LoggingBehavior)
- Separação clara de responsabilidades
- Testabilidade máxima

### 3. **CQRS Verdadeiro**
- Commands = Complexos (precisam pipeline)
- Queries = Simples (não precisam overhead)

### 4. **Manutenibilidade**
- Adicionar query = 1 método no repository
- Adicionar command = 3 arquivos em pasta de feature
- Padrões claros e consistentes

---

## 📚 Referências

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [CQRS Pattern by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- Padrão Herval (documentação interna da empresa)
