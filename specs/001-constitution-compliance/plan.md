# Implementation Plan: Padronizacao do Projeto com a Constituicao

**Branch**: `001-constitution-compliance` | **Date**: 2025-11-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-constitution-compliance/spec.md`

## Summary

Adequar o projeto MetaQuery existente aos principios da Constituicao Clean Architecture v1.0.0.
As principais mudancas envolvem: (1) substituir exceptions por NotificationContext nas validacoes de dominio,
(2) propagar CancellationToken em todos os metodos async, (3) padronizar rotas para kebab-case,
(4) remover logica de tratamento de erros dos Controllers.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0
**Primary Dependencies**: MediatR 13.1.0, FluentValidation 12.1.0, Dapper 2.1.66, SqlKata 4.0.1
**Storage**: Oracle Database 21c XE (via Oracle.ManagedDataAccess.Core)
**Testing**: xUnit (projeto MetaQuery.Tests existente)
**Target Platform**: Windows/Linux server (ASP.NET Core Web API)
**Project Type**: Clean Architecture - API backend
**Performance Goals**: N/A para esta feature (refactoring de conformidade)
**Constraints**: Manter backwards compatibility funcional, nao quebrar testes existentes
**Scale/Scope**: 3 Controllers, 1 Entity, 2 Repositories, 2 Domain Services

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Gate | Status Pre-Design | Acao |
|-----------|------|-------------------|------|
| 2.1 Separacao de Camadas | ✅ PASS | Estrutura correta | Manter `src/MetaQuery.*` (justificado) |
| 2.2 CQRS + MediatR | ✅ PASS | Implementado | Nenhuma |
| 2.3 Dominio Rico | ✅ PASS | Entity com comportamentos | Nenhuma |
| 2.4 Validacao Orientada a Dominio | ❌ FAIL | Entity usa exceptions | **US1**: Migrar para NotificationContext |
| 2.5 IoC | ✅ PASS | Composition Root OK | Nenhuma |
| 2.6 Async-First | ❌ FAIL | Falta CancellationToken | **US2**: Adicionar em todos metodos async |
| 2.7 APIs Estaveis | ❌ FAIL | Rotas nao kebab-case + try-catch em controller | **US3/US4**: Padronizar |
| 2.8 Banco de Dados | ✅ PASS | Dapper parametrizado | Nenhuma |

**Pre-Design Gate**: 3 FAIL → Implementacao necessaria

## Project Structure

### Documentation (this feature)

```text
specs/001-constitution-compliance/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # N/A (no new entities)
├── quickstart.md        # Phase 1 output
├── contracts/           # N/A (refactoring only)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── MetaQuery.Api/                       # Services Layer
│   ├── Controllers/
│   │   ├── ConsultaDinamicaController.cs   # US3, US4: kebab-case + remover try-catch
│   │   ├── MetadadosController.cs          # US2, US3: CancellationToken + kebab-case
│   │   └── QueryBuilderTestController.cs   # US3: kebab-case
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs  # US4: Novo - tratamento global
│   └── Program.cs                          # US4: Registrar middleware
│
├── MetaQuery.Domain/                    # Domain Layer
│   ├── Entities/
│   │   └── TabelaDinamica.cs               # US1: Migrar validacoes para NotificationContext
│   ├── DomainServices/
│   │   ├── ConsultaDinamicaDomainService.cs # US2: CancellationToken
│   │   └── MetadadosDomainService.cs        # US2: CancellationToken
│   ├── Interfaces/
│   │   └── Repositories/
│   │       ├── IMetadadosRepository.cs      # US2: CancellationToken nas assinaturas
│   │       └── IConsultaDinamicaRepository.cs # US2: CancellationToken nas assinaturas
│   └── Notifications/
│       └── NotificationContext.cs           # Existente - sera usado por US1
│
├── MetaQuery.Infra.Data/                # Infrastructure Layer
│   └── Repositories/
│       ├── MetadadosRepository.cs           # US2: CancellationToken
│       └── ConsultaDinamicaRepository.cs    # US2: CancellationToken
│
└── MetaQuery.Infra.CrossCutting.IoC/
    └── DependencyInjection.cs              # US4: Registrar middleware

tests/
└── MetaQuery.Tests/
    └── Commands/                           # Testes existentes - validar apos mudancas
```

**Structure Decision**: Manter estrutura `src/MetaQuery.*` existente. A Constituicao sugere
`01-Services/`, `02-Domain/`, `03-Infra/` mas a estrutura atual ja implementa corretamente
a separacao de camadas. Renomear quebraria muitas referencias sem ganho funcional.

## Constitution Check - Post Design

*Re-evaluation after Phase 1 design completion*

| Principio | Gate | Status Pos-Design | Resolucao |
|-----------|------|-------------------|-----------|
| 2.1 Separacao de Camadas | ✅ PASS | Estrutura mantida | Justificativa documentada |
| 2.2 CQRS + MediatR | ✅ PASS | Sem mudancas | N/A |
| 2.3 Dominio Rico | ✅ PASS | Sem mudancas | N/A |
| 2.4 Validacao Orientada a Dominio | 🔄 PLANNED | research.md documenta solucao | US1: NotificationContext em TabelaDinamica |
| 2.5 IoC | ✅ PASS | Sem mudancas | N/A |
| 2.6 Async-First | 🔄 PLANNED | research.md documenta padrao | US2: CancellationToken em todos async |
| 2.7 APIs Estaveis | 🔄 PLANNED | contracts/ define novas rotas | US3: SlugifyParameterTransformer, US4: Middleware |
| 2.8 Banco de Dados | ✅ PASS | Sem mudancas | N/A |

**Post-Design Gate**: ✅ PASS - Todas violacoes tem plano de resolucao documentado

## Phase 0 Output

- [x] `research.md` - Decisoes tecnicas documentadas
  - NotificationContext pattern para Entity validation
  - CancellationToken propagation com CommandDefinition
  - SlugifyParameterTransformer para kebab-case
  - ExceptionHandlingMiddleware pattern

## Phase 1 Output

- [x] `data-model.md` - Mudancas em TabelaDinamica documentadas
- [x] `contracts/api-routes.md` - Novas rotas kebab-case documentadas
- [x] `quickstart.md` - Guia de validacao das mudancas

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Estrutura de pastas diferente da Constituicao | Projeto existente com muitas referencias | Renomear para `01-Services/` etc. quebraria todo o projeto sem ganho real |

## Next Steps

Execute `/speckit.tasks` para gerar `tasks.md` com as tarefas detalhadas de implementacao.
