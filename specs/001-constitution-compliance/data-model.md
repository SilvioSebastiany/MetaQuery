# Data Model: Constitution Compliance

**Feature**: 001-constitution-compliance
**Date**: 2025-11-26

---

## Entity: TabelaDinamica

### Mudancas Propostas

A entidade `TabelaDinamica` sera modificada para usar NotificationContext ao inves de exceptions.

#### Campos Novos

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `_notifications` | `List<Notification>` | Lista interna de notificacoes de validacao |
| `IsValid` | `bool` (readonly) | Retorna `true` se nao houver notificacoes |

#### Metodos Modificados

| Metodo | Antes | Depois |
|--------|-------|--------|
| `Criar()` | Chama `Validar()` que lanca exception | Chama `Validar()` que popula `_notifications` |
| `Validar()` | `throw new ArgumentException(...)` | `_notifications.Add(...)` |
| `Atualizar()` | Lanca exception se invalido | Popula `_notifications` |

---

## Entity: Notification (Existente)

Ja existe em `NotificationContext.cs`. Usaremos a estrutura existente.

```csharp
public record Notification(string Key, string Message);
```

---

## Interface Changes

### IMetadadosRepository

| Metodo | Assinatura Atual | Assinatura Nova |
|--------|------------------|-----------------|
| `ObterPorIdAsync` | `Task<TabelaDinamica?>` | `Task<TabelaDinamica?> (..., CancellationToken cancellationToken = default)` |
| `ObterTodosAsync` | `Task<IEnumerable<TabelaDinamica>>` | `Task<IEnumerable<TabelaDinamica>> (..., CancellationToken cancellationToken = default)` |
| `ObterPorNomeAsync` | `Task<TabelaDinamica?>` | `Task<TabelaDinamica?> (..., CancellationToken cancellationToken = default)` |
| `CriarAsync` | `Task<int>` | `Task<int> (..., CancellationToken cancellationToken = default)` |
| `AtualizarAsync` | `Task` | `Task (..., CancellationToken cancellationToken = default)` |
| `DesativarAsync` | `Task` | `Task (..., CancellationToken cancellationToken = default)` |
| `ExisteAsync` | `Task<bool>` | `Task<bool> (..., CancellationToken cancellationToken = default)` |
| `ObterPorEsquemaAsync` | `Task<IEnumerable<TabelaDinamica>>` | `Task<IEnumerable<TabelaDinamica>> (..., CancellationToken cancellationToken = default)` |
| `ObterAtivosAsync` | `Task<IEnumerable<TabelaDinamica>>` | `Task<IEnumerable<TabelaDinamica>> (..., CancellationToken cancellationToken = default)` |

### IConsultaDinamicaRepository

Todos os metodos async receberao `CancellationToken cancellationToken = default` como ultimo parametro.

---

## Domain Services Changes

### ConsultaDinamicaDomainService

Todos os metodos publicos async receberao e propagarao CancellationToken.

---

## Validation Rules (Existentes)

As regras de validacao em `TabelaDinamica.Validar()` permanecem as mesmas, apenas mudam de `throw` para `notification`:

| Campo | Regra | Mensagem |
|-------|-------|----------|
| `NomeTabela` | Required, non-empty | "Nome da tabela e obrigatorio" |
| `NomeExibicao` | Required, non-empty | "Nome de exibicao e obrigatorio" |
| `Esquema` | Required, non-empty | "Esquema e obrigatorio" |
| `ColunaChavePrimaria` | Required, non-empty | "Coluna chave primaria e obrigatoria" |

---

## State Transitions

Nenhuma mudanca em state transitions. O fluxo permanece:

```
[Novo] --Criar()--> [Ativo] --Desativar()--> [Inativo]
                       ^                          |
                       |--------Reativar()--------|
```
