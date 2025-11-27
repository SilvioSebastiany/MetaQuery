# Research: Padronizacao do Projeto com a Constituicao

**Feature**: 001-constitution-compliance
**Date**: 2025-11-26

---

## 1. Entity Validation com NotificationContext

### Decisao
Modificar `TabelaDinamica.Criar()` e metodos de atualizacao para usar NotificationContext ao inves de lancar exceptions.

### Rationale
- Constituicao 2.4 (MANDATORY): "Falhas de validacao de negocio sao propagadas via Notification Context, nunca via excecoes"
- Permite acumular multiplos erros em uma unica validacao
- Handlers podem verificar `HasNotifications` antes de persistir

### Alternativas Consideradas
1. **Manter exceptions** - Rejeitado: viola principio MANDATORY da Constituicao
2. **Result pattern (Result<T>)** - Rejeitado: NotificationContext ja existe e e o padrao da org
3. **FluentValidation na Entity** - Rejeitado: FluentValidation e para Commands, nao Entities

### Implementacao
```csharp
// Antes (viola Constituicao)
private void Validar()
{
    if (erros.Any())
        throw new ArgumentException($"Erros: {string.Join(", ", erros)}");
}

// Depois (conforme Constituicao)
public bool IsValid => !_notifications.Any();
private readonly List<Notification> _notifications = new();

public static TabelaDinamica Criar(..., INotificationContext? notificationContext = null)
{
    var entity = new TabelaDinamica { ... };
    entity.Validar();
    notificationContext?.AddNotifications(entity._notifications);
    return entity;
}
```

---

## 2. CancellationToken Propagation

### Decisao
Adicionar `CancellationToken cancellationToken = default` em todos os metodos async de:
- Interfaces de Repository
- Implementacoes de Repository
- Domain Services
- Controllers (via HttpContext.RequestAborted)

### Rationale
- Constituicao 2.6 (MANDATORY): "Metodos assincronos recebem e propagam CancellationToken"
- Permite cancelamento gracioso quando cliente desconecta
- Best practice para operacoes de I/O longas

### Alternativas Consideradas
1. **Nao propagar** - Rejeitado: viola principio MANDATORY
2. **Usar timeout global** - Rejeitado: CancellationToken e mais granular e flexivel

### Implementacao
```csharp
// Interface
Task<TabelaDinamica?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

// Repository
public async Task<TabelaDinamica?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
{
    var dto = await _connection.QueryFirstOrDefaultAsync<MetadadoDto>(
        new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    return dto != null ? MapToEntity(dto) : null;
}

// Controller
public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
{
    var metadado = await _repository.ObterPorIdAsync(id, cancellationToken);
    return metadado == null ? NotFound() : Ok(metadado);
}
```

---

## 3. Kebab-case Routes

### Decisao
Configurar ASP.NET Core para usar kebab-case automaticamente via `SlugifyParameterTransformer`.

### Rationale
- Constituicao 2.7: "Rotas REST previsiveis, padronizadas e em kebab-case"
- Evita inconsistencia manual entre controllers
- Padrao REST amplamente adotado

### Alternativas Consideradas
1. **Renomear manualmente cada rota** - Rejeitado: propenso a erros e inconsistencias
2. **Usar [Route] explicito em cada action** - Rejeitado: verboso e dificil manutencao

### Implementacao
```csharp
// Program.cs
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

// SlugifyParameterTransformer.cs
public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;
        return Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}
```

**Resultado**:
- `MetadadosController` → `/api/metadados`
- `ConsultaDinamicaController` → `/api/consulta-dinamica`
- `QueryBuilderTestController` → `/api/query-builder-test`

---

## 4. Exception Handling Middleware

### Decisao
Criar middleware global para tratamento de excecoes, removendo try-catch dos Controllers.

### Rationale
- Constituicao 2.7: "Sem logica de negocio em Controllers — apenas roteamento, binding e status code"
- Centraliza tratamento de erros
- Controllers ficam mais limpos e focados

### Alternativas Consideradas
1. **Manter try-catch em Controllers** - Rejeitado: viola principio da Constituicao
2. **UseExceptionHandler built-in** - Rejeitado: menos controle sobre formato de resposta
3. **MediatR Pipeline Behavior** - Parcialmente aceito: para erros de negocio via NotificationContext

### Implementacao
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento invalido");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { Erro = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro nao tratado");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { Erro = "Erro interno" });
        }
    }
}
```

---

## 5. Arquivos a Modificar

### Domain Layer
| Arquivo | Mudanca |
|---------|---------|
| `TabelaDinamica.cs` | US1: NotificationContext nas validacoes |
| `IMetadadosRepository.cs` | US2: CancellationToken |
| `IConsultaDinamicaRepository.cs` | US2: CancellationToken |
| `ConsultaDinamicaDomainService.cs` | US2: CancellationToken |

### Infrastructure Layer
| Arquivo | Mudanca |
|---------|---------|
| `MetadadosRepository.cs` | US2: CancellationToken + CommandDefinition |
| `ConsultaDinamicaRepository.cs` | US2: CancellationToken |

### Services Layer
| Arquivo | Mudanca |
|---------|---------|
| `MetadadosController.cs` | US2: CancellationToken, US3: kebab-case |
| `ConsultaDinamicaController.cs` | US2: CancellationToken, US3: kebab-case, US4: remover try-catch |
| `QueryBuilderTestController.cs` | US3: kebab-case |
| `Program.cs` | US3: SlugifyParameterTransformer, US4: Middleware |
| `ExceptionHandlingMiddleware.cs` | US4: Novo arquivo |
| `SlugifyParameterTransformer.cs` | US3: Novo arquivo |

---

## 6. Breaking Changes

### Rotas (US3)
- `/api/ConsultaDinamica/*` → `/api/consulta-dinamica/*`
- `/api/QueryBuilderTest/*` → `/api/query-builder-test/*`
- `/api/metadados/tabela/{nomeTabela}` permanece (ja kebab-case)

**Mitigacao**: API ainda em desenvolvimento, nao ha consumidores externos.

### Validacao de Entity (US1)
- `TabelaDinamica.Criar()` nao lanca mais exception para dados invalidos
- Handlers devem verificar `notificationContext.HasNotifications` antes de persistir

**Mitigacao**: Handlers existentes ja usam NotificationContext para FluentValidation.
