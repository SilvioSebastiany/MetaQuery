# Tasks: Padronizacao do Projeto com a Constituicao

**Input**: Design documents from `/specs/001-constitution-compliance/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Não solicitados na especificação. Apenas validação manual via quickstart.md.

**Organization**: Tasks organizadas por User Story para permitir implementação e teste independente de cada história.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode executar em paralelo (arquivos diferentes, sem dependências)
- **[Story]**: Qual User Story a tarefa pertence (US1, US2, US3, US4)
- Caminhos exatos incluídos nas descrições

## Path Conventions

Estrutura existente do projeto:
- **Services Layer**: `src/MetaQuery.Api/`
- **Domain Layer**: `src/MetaQuery.Domain/`
- **Infrastructure Layer**: `src/MetaQuery.Infra.*/`
- **Tests**: `tests/MetaQuery.Tests/`

---

## Phase 1: Setup (Preparação)

**Purpose**: Preparar ambiente para as mudanças de conformidade

- [X] T001 Verificar que branch `001-constitution-compliance` está ativa
- [X] T002 Executar `dotnet build` para garantir projeto compila antes das mudanças
- [X] T003 Executar `dotnet test` para garantir testes passam antes das mudanças

---

## Phase 2: Foundational (Infraestrutura Compartilhada)

**Purpose**: Criar componentes que serão usados por múltiplas User Stories

**⚠️ CRITICAL**: Estas tarefas devem ser concluídas ANTES de iniciar as User Stories

- [X] T004 [P] Criar `SlugifyParameterTransformer.cs` em `src/MetaQuery.Api/Infrastructure/SlugifyParameterTransformer.cs`
- [X] T005 [P] Criar `ExceptionHandlingMiddleware.cs` em `src/MetaQuery.Api/Middleware/ExceptionHandlingMiddleware.cs`
- [X] T006 Registrar SlugifyParameterTransformer em `src/MetaQuery.Api/Program.cs` (RouteTokenTransformerConvention)
- [X] T007 Registrar ExceptionHandlingMiddleware em `src/MetaQuery.Api/Program.cs` (app.UseMiddleware)

**Checkpoint**: Infraestrutura pronta - implementação das User Stories pode começar

---

## Phase 3: User Story 1 - Adequar Validação do Domínio (Priority: P1) 🎯 MVP

**Goal**: Entidades do domínio usam NotificationContext ao invés de exceptions para validações de negócio

**Independent Test**: Criar TabelaDinamica com dados inválidos → NotificationContext deve conter erros, sem exception lançada

### Implementation for User Story 1

- [X] T008 [US1] Adicionar propriedade `IsValid` e lista `_notifications` em `src/MetaQuery.Domain/Entities/TabelaDinamica.cs`
- [X] T009 [US1] Modificar método `Validar()` para popular `_notifications` ao invés de lançar exception em `src/MetaQuery.Domain/Entities/TabelaDinamica.cs`
- [X] T010 [US1] Modificar factory method `Criar()` para propagar notificações ao NotificationContext em `src/MetaQuery.Domain/Entities/TabelaDinamica.cs`
- [X] T011 [US1] Modificar método `Atualizar()` para usar NotificationContext ao invés de exception em `src/MetaQuery.Domain/Entities/TabelaDinamica.cs`
- [X] T012 [US1] Atualizar Handlers que usam TabelaDinamica para verificar `HasNotifications` antes de persistir

**Checkpoint**: User Story 1 completa - validações de domínio usam NotificationContext

---

## Phase 4: User Story 2 - Propagar CancellationToken (Priority: P2)

**Goal**: Todos os métodos async recebem e propagam CancellationToken

**Independent Test**: Verificar que todas interfaces e implementações async aceitam CancellationToken

### Implementation for User Story 2 - Interfaces (Domain Layer)

- [X] T013 [P] [US2] Adicionar `CancellationToken cancellationToken = default` em todos métodos de `src/MetaQuery.Domain/Interfaces/Repositories/IMetadadosRepository.cs`
- [X] T014 [P] [US2] Adicionar `CancellationToken cancellationToken = default` em todos métodos de `src/MetaQuery.Domain/Interfaces/Repositories/IConsultaDinamicaRepository.cs`

### Implementation for User Story 2 - Domain Services

- [X] T015 [P] [US2] Adicionar CancellationToken em métodos de `src/MetaQuery.Domain/DomainServices/ConsultaDinamicaDomainService.cs`
- [X] T016 [P] [US2] Adicionar CancellationToken em métodos de `src/MetaQuery.Domain/DomainServices/MetadadosDomainService.cs` (se existir)

### Implementation for User Story 2 - Repository Implementations

- [X] T017 [P] [US2] Implementar CancellationToken com CommandDefinition em `src/MetaQuery.Infra.Data/Repositories/MetadadosRepository.cs` (ou caminho equivalente)
- [X] T018 [P] [US2] Implementar CancellationToken com CommandDefinition em `src/MetaQuery.Infra.Data/Repositories/ConsultaDinamicaRepository.cs` (ou caminho equivalente)

### Implementation for User Story 2 - Controllers

- [X] T019 [P] [US2] Adicionar CancellationToken nos métodos de `src/MetaQuery.Api/Controllers/MetadadosController.cs`
- [X] T020 [P] [US2] Adicionar CancellationToken nos métodos de `src/MetaQuery.Api/Controllers/ConsultaDinamicaController.cs`
- [X] T021 [P] [US2] Adicionar CancellationToken nos métodos de `src/MetaQuery.Api/Controllers/QueryBuilderTestController.cs` (N/A - apenas métodos sync)

**Checkpoint**: User Story 2 completa - CancellationToken propagado em toda stack ✅

---

## Phase 5: User Story 3 - Padronizar Rotas da API (Priority: P3)

**Goal**: Todas as rotas seguem padrão kebab-case automaticamente

**Independent Test**: Acessar `/api/consulta-dinamica` (deve funcionar) e `/api/ConsultaDinamica` (deve retornar 404)

### Implementation for User Story 3

- [X] T022 [US3] Verificar que SlugifyParameterTransformer está registrado corretamente (T006)
- [X] T023 [P] [US3] Remover atributos `[Route]` hardcoded que conflitem com kebab-case em `src/MetaQuery.Api/Controllers/ConsultaDinamicaController.cs`
- [X] T024 [P] [US3] Remover atributos `[Route]` hardcoded que conflitem com kebab-case em `src/MetaQuery.Api/Controllers/QueryBuilderTestController.cs`
- [X] T025 [US3] Atualizar arquivos `.http` de teste em `tests.Http/` para usar novas rotas kebab-case

**Checkpoint**: User Story 3 completa - todas rotas em kebab-case

---

## Phase 6: User Story 4 - Remover Lógica de Controllers (Priority: P3)

**Goal**: Controllers contêm apenas roteamento, binding e status code - sem try-catch customizado

**Independent Test**: Verificar que nenhum Controller possui blocos try-catch com lógica de tratamento

### Implementation for User Story 4

- [X] T026 [US4] Verificar que ExceptionHandlingMiddleware está registrado corretamente (T007)
- [X] T027 [US4] Remover bloco try-catch de `src/MetaQuery.Api/Controllers/ConsultaDinamicaController.cs`
- [X] T028 [P] [US4] Verificar e remover try-catch de outros Controllers se existirem
- [X] T029 [US4] Verificar que erros de ArgumentException retornam 400 via middleware

**Checkpoint**: User Story 4 completa - Controllers limpos, middleware trata exceções

---

## Phase 7: Polish & Validação Final

**Purpose**: Validação final e documentação

- [X] T030 [P] Executar `dotnet build` para garantir compilação
- [X] T031 [P] Executar `dotnet test` para garantir testes passam
- [ ] T032 Executar validações do `quickstart.md` (requer API rodando):
  - [ ] T032a `GET /api/consulta-dinamica` responde
  - [ ] T032b `GET /api/ConsultaDinamica` retorna 404
  - [ ] T032c `GET /api/query-builder-test` responde
  - [ ] T032d `POST /api/metadados` com dados vazios retorna 400 com lista de erros
  - [ ] T032e Nenhum stack trace exposto em respostas de erro
- [X] T033 Atualizar Swagger/OpenAPI se necessário (N/A - configuração automática)
- [ ] T034 Commit final com mensagem: "feat: constitution compliance - all 4 user stories complete"

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências - pode iniciar imediatamente
- **Foundational (Phase 2)**: Depende de Setup - BLOQUEIA todas User Stories
- **User Stories (Phase 3-6)**: Dependem de Foundational (Phase 2)
  - US1 (P1): Pode iniciar após Phase 2
  - US2 (P2): Pode iniciar após Phase 2 (paralelo com US1)
  - US3 (P3): Depende de T004/T006 (SlugifyParameterTransformer)
  - US4 (P3): Depende de T005/T007 (ExceptionHandlingMiddleware)
- **Polish (Phase 7)**: Depende de todas User Stories

### User Story Dependencies

| Story | Depende de | Pode executar em paralelo com |
|-------|------------|-------------------------------|
| US1 (P1) | Phase 2 | US2 |
| US2 (P2) | Phase 2 | US1 |
| US3 (P3) | T004, T006 | US4 |
| US4 (P3) | T005, T007 | US3 |

### Parallel Opportunities per Story

**US1**: Tasks T008-T011 são sequenciais (mesmo arquivo)
**US2**: Tasks T013-T021 marcadas [P] podem executar em paralelo (arquivos diferentes)
**US3**: Tasks T023-T024 marcadas [P] podem executar em paralelo
**US4**: Tasks T027-T028 podem executar em paralelo

---

## Parallel Example: User Story 2

```bash
# Lançar todas interfaces em paralelo:
T013: "CancellationToken em IMetadadosRepository.cs"
T014: "CancellationToken em IConsultaDinamicaRepository.cs"

# Lançar todas implementações em paralelo:
T015: "CancellationToken em ConsultaDinamicaDomainService.cs"
T017: "CancellationToken em MetadadosRepository.cs"
T018: "CancellationToken em ConsultaDinamicaRepository.cs"

# Lançar todos Controllers em paralelo:
T019: "CancellationToken em MetadadosController.cs"
T020: "CancellationToken em ConsultaDinamicaController.cs"
T021: "CancellationToken em QueryBuilderTestController.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup ✓
2. Complete Phase 2: Foundational ✓
3. Complete Phase 3: User Story 1 (Validação com NotificationContext)
4. **STOP e VALIDAR**: Testar TabelaDinamica.Criar() com dados inválidos
5. Commit parcial: "feat: US1 - entity validation with NotificationContext"

### Incremental Delivery (Recomendado)

1. Setup + Foundational → Infraestrutura pronta
2. US1 → Testar → Commit (MVP - Princípio 2.4 MANDATORY)
3. US2 → Testar → Commit (Princípio 2.6 MANDATORY)
4. US3 + US4 → Testar → Commit (Princípio 2.7)
5. Polish → Validação final → Commit

### Full Parallel (Se múltiplos desenvolvedores)

1. Todos completam Setup + Foundational juntos
2. Após Foundational:
   - Dev A: User Story 1 (Domain)
   - Dev B: User Story 2 (Async stack)
   - Dev C: User Story 3 + 4 (API layer)
3. Merge e validação final

---

## Summary

| Métrica | Valor |
|---------|-------|
| Total de Tasks | 34 |
| Tasks Phase 1 (Setup) | 3 |
| Tasks Phase 2 (Foundational) | 4 |
| Tasks US1 | 5 |
| Tasks US2 | 9 |
| Tasks US3 | 4 |
| Tasks US4 | 4 |
| Tasks Phase 7 (Polish) | 5 |
| Tasks Paralelizáveis [P] | 15 |
| MVP Scope | US1 (5 tasks) |
| Estimated Story Points | US1: 3, US2: 5, US3: 2, US4: 2 |

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências
- [Story] label mapeia tarefa para User Story específica
- Cada User Story pode ser completada e testada independentemente
- Commit após cada tarefa ou grupo lógico
- Pare em qualquer checkpoint para validar história independentemente
- Referência: `quickstart.md` para validação manual das mudanças
