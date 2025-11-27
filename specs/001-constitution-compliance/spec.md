# Feature Specification: Padronizacao do Projeto com a Constituicao

**Feature Branch**: `001-constitution-compliance`
**Created**: 2025-11-26
**Status**: Draft
**Input**: User description: "quero que padronize o projeto com base na constituição, pois comecei esse projeto sem ele"

---

## Analise de Conformidade Atual

### Resumo da Auditoria

O projeto MetaQuery foi iniciado antes da definicao da Constituicao Clean Architecture. A analise identificou os seguintes pontos:

| Principio | Status | Observacoes |
|-----------|--------|-------------|
| 2.1 Separacao de Camadas | ⚠️ Parcial | Estrutura existe mas nomenclatura difere do padrao |
| 2.2 CQRS + MediatR | ✅ Conforme | Commands, Handlers e MediatR implementados |
| 2.3 Dominio Rico | ✅ Conforme | Entity TabelaDinamica com comportamentos |
| 2.4 Validacao Orientada a Dominio | ⚠️ Parcial | FluentValidation OK, mas Entity usa exceptions |
| 2.5 IoC e Dependency Inversion | ✅ Conforme | Composition Root em IoC, interfaces no Domain |
| 2.6 Async-First | ⚠️ Parcial | Metodos async mas sem CancellationToken |
| 2.7 APIs e Contratos Estaveis | ⚠️ Parcial | Rotas nao estao em kebab-case |
| 2.8 Padroes de Banco de Dados | ✅ Conforme | Dapper com queries parametrizadas |

### Nao-Conformidades Identificadas

1. **Estrutura de Pastas**: Atual usa `src/` flat, Constituicao sugere `01-Services/`, `02-Domain/`, `03-Infra/`
2. **Entity com Exceptions**: `TabelaDinamica.Validar()` lanca `ArgumentException` ao inves de usar NotificationContext
3. **Falta CancellationToken**: Metodos async nao propagam CancellationToken
4. **Rotas nao kebab-case**: `/tabela/{nomeTabela}` deveria usar parametro kebab-case
5. **Controller com logica**: `ConsultaDinamicaController` tem try-catch com logica de tratamento

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Adequar Validacao do Dominio (Priority: P1)

Como desenvolvedor, quero que as entidades do dominio usem NotificationContext ao inves de exceptions para validacoes de negocio, para seguir o padrao da Constituicao e ter tratamento consistente de erros.

**Why this priority**: Validacao e o nucleo do dominio. Usar exceptions para regras de negocio quebra o principio fundamental 2.4 (MANDATORY).

**Independent Test**: Criar uma entidade invalida e verificar que NotificationContext contem os erros ao inves de lancar exception.

**Acceptance Scenarios**:

1. **Given** dados invalidos para TabelaDinamica, **When** tentar criar via factory method, **Then** NotificationContext deve conter os erros e nenhuma exception deve ser lancada
2. **Given** TabelaDinamica existente, **When** atualizar com dados invalidos, **Then** NotificationContext deve conter os erros

---

### User Story 2 - Propagar CancellationToken (Priority: P2)

Como desenvolvedor, quero que todos os metodos async recebam e propaguem CancellationToken, para seguir o principio Async-First da Constituicao.

**Why this priority**: Async-First e MANDATORY (principio 2.6). Permite cancelamento gracioso de operacoes longas.

**Independent Test**: Verificar que todos os metodos async em Repositories, Services e Controllers aceitam CancellationToken.

**Acceptance Scenarios**:

1. **Given** uma requisicao em andamento, **When** cliente cancelar a requisicao, **Then** operacao de banco deve ser cancelada
2. **Given** metodo de repository, **When** chamado, **Then** deve aceitar e propagar CancellationToken para a query

---

### User Story 3 - Padronizar Rotas da API (Priority: P3)

Como consumidor da API, quero que todas as rotas sigam o padrao kebab-case, para ter consistencia e previsibilidade nos endpoints.

**Why this priority**: APIs e Contratos Estaveis (principio 2.7) - importante mas nao bloqueia funcionalidade.

**Independent Test**: Verificar que todas as rotas seguem o padrao kebab-case via Swagger ou testes HTTP.

**Acceptance Scenarios**:

1. **Given** endpoint de metadados, **When** acessar via rota kebab-case, **Then** deve retornar dados corretamente
2. **Given** documentacao Swagger, **When** visualizar, **Then** todas as rotas devem estar em kebab-case

---

### User Story 4 - Remover Logica de Controllers (Priority: P3)

Como desenvolvedor, quero que Controllers contenham apenas roteamento, binding e status code, movendo qualquer logica para Domain Services ou Handlers.

**Why this priority**: Principio 2.7 - Controllers nao devem ter logica de negocio.

**Independent Test**: Verificar que Controllers nao possuem try-catch com logica de tratamento customizada.

**Acceptance Scenarios**:

1. **Given** requisicao que gera erro de negocio, **When** processada, **Then** tratamento deve ocorrer em middleware ou behavior, nao no Controller
2. **Given** Controller, **When** analisar codigo, **Then** deve conter apenas chamadas a mediator/repository e retorno de status

---

### Edge Cases

- O que acontece se uma Entity existente no banco for carregada com dados que violariam as novas validacoes?
- Como tratar backwards compatibility das rotas antigas durante transicao?

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Entity `TabelaDinamica` DEVE usar NotificationContext para reportar erros de validacao ao inves de lancar exceptions
- **FR-002**: Entity DEVE expor metodo `IsValid` que retorna se ha notificacoes
- **FR-003**: Todos os metodos async em Repositories DEVEM aceitar `CancellationToken` como parametro
- **FR-004**: Todos os metodos async em Domain Services DEVEM aceitar e propagar `CancellationToken`
- **FR-005**: Controllers DEVEM propagar `CancellationToken` da requisicao HTTP para os services
- **FR-006**: Rotas da API DEVEM seguir padrao kebab-case (ex: `/api/consulta-dinamica/tabelas-disponiveis`)
- **FR-007**: Controllers NAO DEVEM conter blocos try-catch com logica de tratamento customizada
- **FR-008**: Tratamento de erros DEVE ser feito via middleware global ou pipeline behaviors

### Assumptions

- A estrutura de pastas atual (`src/MetaQuery.*`) sera mantida, pois renomear para `01-Services/` etc. quebraria muitas referencias e a estrutura atual ja esta funcionando.
- O padrao de nomenclatura em portugues sera mantido conforme Ubiquitous Language.
- Backwards compatibility das rotas nao e necessaria pois a API ainda esta em desenvolvimento.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das validacoes de negocio em Entities usam NotificationContext (0 exceptions para validacao de negocio)
- **SC-002**: 100% dos metodos async em Repositories, Services e Controllers aceitam CancellationToken
- **SC-003**: 100% das rotas da API seguem padrao kebab-case
- **SC-004**: 0 blocos try-catch com logica de negocio em Controllers
- **SC-005**: Todos os testes existentes continuam passando apos as mudancas
- **SC-006**: Swagger UI mostra todas as rotas em kebab-case
