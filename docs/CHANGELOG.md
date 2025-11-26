# 📝 Changelog - MetaQuery

## [2.0.0] - 2025-11-25

### ✨ Novas Funcionalidades

#### Agrupamento Hierárquico Dinâmico
- **Feature**: Transforma resultados flat (com JOINs) em estrutura hierárquica aninhada
- **Benefício**: Reduz JSON em 60-80%, estrutura mais natural e fácil de consumir
- **Como usar**: `GET /api/consultadinamica/{tabela}?formatoHierarquico=true`
- **Arquivos**:
  - Novo: `HierarchicalGrouper.cs`
  - Modificados: `ConsultaDinamicaDomainService.cs`, `ConsultaDinamicaController.cs`, `DependencyInjection.cs`

#### Tratamento de Erros HTTP 400
- **Feature**: Erros Oracle ORA-00942 e ORA-00904 retornam 400 Bad Request com mensagens claras
- **Benefício**: Melhor debugging, mensagens em português, sem stack trace exposto
- **Erros tratados**:
  - ORA-00942: Tabela não existe
  - ORA-00904: Coluna não existe
- **Arquivos**:
  - Modificados: `ConsultaDinamicaRepository.cs`, `ConsultaDinamicaController.cs`

### 📚 Documentação
- Atualizado `CQRS_IMPLEMENTATION.md` com padrão Herval 100%
- Criado `CHANGELOG.md` (este arquivo)
- Criado `novas_funcionalidades.md` com documentação detalhada

### 🔧 Melhorias
- Queries sem MediatR (repositório direto) para melhor performance
- Commands em feature folders com FluentValidation
- Todos os testes passando (21/21)

---

## [1.0.0] - 2025-11-24

### 🎉 Lançamento Inicial

#### Renomeação do Projeto
- QueryBuilderMVP → MetaQuery
- Atualização de todos os namespaces e referências
- Configurações VS Code atualizadas

#### Reorganização Arquitetural
- Migração para padrão CQRS + MediatR (Herval)
- Commands reorganizados em feature folders
- FluentValidation adicionado (3 validators)
- Remoção de Queries do MediatR (11 arquivos deletados)
- ~600 linhas de código simplificadas

#### Features Core
- Consultas dinâmicas baseadas em metadados
- JOINs automáticos com profundidade configurável
- SqlKata + Dapper para geração de queries
- Oracle 21c XE support
- Docker containerization

### 📦 Tecnologias
- .NET 9.0
- Oracle.ManagedDataAccess 23.6.1
- SqlKata 3.0.0-beta
- Dapper 2.1.66
- MediatR 13.1.0
- FluentValidation 12.1.0

---

## Legendas

- ✨ Nova Funcionalidade
- 🔧 Melhoria
- 🐛 Correção de Bug
- 📚 Documentação
- ⚠️ Breaking Change
- 🗑️ Remoção
