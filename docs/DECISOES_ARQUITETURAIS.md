# 📐 Decisões Arquiteturais - MetaQuery

## 📅 Data: Novembro 19, 2025

---

## 🎯 Contexto e Motivação

Este documento registra as decisões arquiteturais tomadas durante o desenvolvimento do projeto MetaQuery, com foco especial na implementação do padrão CQRS + MediatR e DomainServices.

**Contexto Corporativo:**
- Empresa: Herval
- Padrão existente: CQRS pragmático (Commands com MediatR, leitura direta via DomainServices)
- Objetivo do projeto: MVP sustentável e escalável

---

## 🏗️ Decisão Principal: CQRS Completo + DomainServices

### ✅ Decisão Tomada

**Implementar CQRS completo (Queries + Commands via MediatR) mantendo DomainServices para lógica de negócio**

### 📋 Estrutura Adotada

```
src/MetaQuery.Domain/
├── Queries/                              ✅ CQRS Read (via MediatR)
│   ├── ConsultaDinamicaQuery.cs
│   └── Handlers/
│       └── ConsultaDinamicaQueryHandler.cs  → Orquestrador (magro)
│
├── Commands/                             ✅ CQRS Write (via MediatR)
│   └── CriarMetadado/
│       ├── CriarMetadadoCommand.cs
│       └── CriarMetadadoCommandHandler.cs   → Orquestrador (magro)
│
├── DomainServices/                       ✅ Lógica de Negócio
│   ├── ConsultaDinamicaDomainService.cs  ← Lógica complexa + Validações
│   └── MetadadosDomainService.cs         ← Lógica complexa + Validações
│
├── Behaviors/                            ✅ Cross-Cutting Concerns
│   ├── LoggingBehavior.cs                ← Logs automáticos
│   └── ValidationBehavior.cs             ← Validações automáticas
│
├── Validators/                           ✅ FluentValidation
│   └── ConsultaDinamicaQueryValidator.cs ← Regras de validação
│
└── Services/                             ✅ Auxiliares (sem lógica de negócio)
    └── QueryBuilderService.cs            ← Monta SQL (stateless)
```

---

## 🤔 Alternativas Consideradas

### Alternativa 1: Padrão Herval (CQRS Pragmático)

**Estrutura:**
```
Domain/
├── Commands/                    ✅ WRITE com MediatR
│   └── IntegrarEstoque/
│
├── DomainServices/              ✅ READ + WRITE direto
│   └── ProdutoDomainService.cs
│       ├── ObterProduto()       ← READ sem MediatR
│       └── AtualizarEstoque()   ← Usa Command
```

**Fluxo READ:**
```
Controller → DomainService → Repository
```

**Fluxo WRITE:**
```
Controller → IMediator.Send(Command) → Handler → DomainService → Repository
```

**Prós:**
- ✅ Menos código (queries sem MediatR)
- ✅ Simplicidade inicial
- ✅ Alinhado com padrão Herval existente

**Contras:**
- ❌ Inconsistência (2 padrões misturados)
- ❌ Behaviors não funcionam para READ (sem logs, validação, cache automático)
- ❌ Controller acoplado ao DomainService (dificulta testes)
- ❌ Difícil adicionar cross-cutting concerns em READ

---

### Alternativa 2: CQRS Completo SEM DomainServices

**Estrutura:**
```
Domain/
├── Queries/Handlers/
│   └── ConsultaDinamicaQueryHandler.cs  ← Handler com TODA lógica (100+ linhas)
```

**Prós:**
- ✅ CQRS puro e consistente
- ✅ Behaviors funcionam para tudo

**Contras:**
- ❌ Handlers muito grandes (violação SRP)
- ❌ Lógica de negócio não reutilizável
- ❌ Difícil testar lógica isoladamente

---

### Alternativa 3: CQRS Completo COM DomainServices (ESCOLHIDA ✅)

**Estrutura:** Ver seção "Estrutura Adotada" acima

**Prós:**
- ✅ **Consistência total** (Controller sempre usa `IMediator`)
- ✅ **Behaviors funcionam para READ e WRITE** (logs, validação, cache)
- ✅ **Handlers magros** (10-20 linhas, apenas orquestração)
- ✅ **DomainServices reutilizáveis** (pode usar em Jobs, outros Handlers)
- ✅ **Testabilidade superior** (lógica isolada em DomainServices)
- ✅ **Separação de responsabilidades clara**
- ✅ **Evolutivo** (fácil adicionar Event Sourcing, microserviços)
- ✅ **Padrão de mercado** (.NET community)

**Contras:**
- ⚠️ Mais código que alternativa 1
- ⚠️ Curva de aprendizado ligeiramente maior
- ⚠️ Diferente do padrão Herval (mas justificável)

---

## 📊 Comparação Detalhada

| Critério | Herval (Pragmático) | CQRS sem DomainServices | CQRS + DomainServices ✅ |
|----------|---------------------|-------------------------|-------------------------|
| **Consistência** | ⚠️ 2 padrões | ✅ 1 padrão | ✅ 1 padrão |
| **Handlers magros** | ✅ Sim (Commands) | ❌ Não | ✅ Sim (todos) |
| **Lógica reutilizável** | ✅ Sim | ❌ Não | ✅ Sim |
| **Behaviors em READ** | ❌ Não | ✅ Sim | ✅ Sim |
| **Testabilidade** | ⚠️ Média | ⚠️ Média | ✅ Alta |
| **Separação SRP** | ⚠️ Média | ❌ Baixa | ✅ Alta |
| **Evolutivo** | ⚠️ Limitado | ✅ Sim | ✅ Sim |
| **Linhas de código** | ✅ Menos | ⚠️ Mais | ⚠️ Médio |
| **Curva aprendizado** | ✅ 2 dias | ⚠️ 1 semana | ⚠️ 1 semana |

---

## 🎯 Separação de Responsabilidades (SRP)

### Handler (Orquestrador - 10-20 linhas)

```csharp
public class ConsultaDinamicaQueryHandler : IRequestHandler<...>
{
    private readonly ConsultaDinamicaDomainService _domainService;

    public async Task<ConsultaDinamicaResult?> Handle(...)
    {
        try
        {
            // ✅ APENAS orquestra - delega para DomainService
            return await _domainService.ConsultarTabelaAsync(
                request.Tabela,
                request.IncluirJoins,
                request.Profundidade);
        }
        catch (Exception ex)
        {
            // ✅ Trata exceções e popula NotificationContext
            _notificationContext.AddNotification("Erro", ex.Message);
            return null;
        }
    }
}
```

**Responsabilidade:** Orquestração (recebe request, chama DomainService, trata erros)

---

### DomainService (Lógica de Negócio - 50-200 linhas)

```csharp
public class ConsultaDinamicaDomainService
{
    public async Task<ConsultaDinamicaResult> ConsultarTabelaAsync(...)
    {
        // ✅ Validações de negócio
        ValidarTabelaPermitida(tabela);
        ValidarProfundidade(profundidade);

        // ✅ Lógica de negócio
        var sqlQuery = _queryBuilderService.MontarQuery(...);
        var compiledQuery = _queryBuilderService.CompilarQuery(sqlQuery);

        // ✅ Execução
        var dados = await _consultaRepository.ExecutarQueryAsync(sqlQuery);

        // ✅ Regras de negócio pós-execução
        ValidarLimiteRegistros(dados.Count(), tabela);

        return new ConsultaDinamicaResult(...);
    }

    // ✅ Métodos privados de validação
    private void ValidarTabelaPermitida(string tabela) { }
    private void ValidarProfundidade(int profundidade) { }
    private void ValidarLimiteRegistros(int total, string tabela) { }
}
```

**Responsabilidade:** Lógica de negócio (validações, transformações, regras complexas)

---

### Repository (Acesso a Dados - SQL puro)

```csharp
public class ConsultaDinamicaRepository : IConsultaDinamicaRepository
{
    public async Task<IEnumerable<dynamic>> ExecutarQueryAsync(Query query)
    {
        // ✅ APENAS executa SQL no banco
        var compiled = _compiler.Compile(query);
        return await _connection.QueryAsync<dynamic>(compiled.Sql, compiled.NamedBindings);
    }
}
```

**Responsabilidade:** Acesso ao banco de dados (SQL, Dapper)

---

### Service (Auxiliar - Stateless)

```csharp
public class QueryBuilderService : IQueryBuilderService
{
    public Query MontarQuery(string tabela, bool incluirJoins, int profundidade)
    {
        // ✅ Monta query SQL usando SqlKata
        // ✅ SEM lógica de negócio, SEM validações
        // ✅ Stateless (pode ser Singleton)
        return new Query(tabela).Select("*");
    }
}
```

**Responsabilidade:** Auxiliar técnico (monta SQL, sem lógica de negócio)

---

## 🔄 Fluxo Completo de Execução

### Exemplo: GET /api/ConsultaDinamica/CLIENTES?incluirJoins=true

```
1. HTTP Request
   ↓
2. ConsultaDinamicaController.Consultar()
   ├─ Cria: new ConsultaDinamicaQuery("CLIENTES", true, 2)
   └─ Chama: await _mediator.Send(query)
   ↓
3. MediatR Pipeline
   ├─ Identifica: ConsultaDinamicaQueryHandler
   └─ Executa Behaviors:
   ↓
4. LoggingBehavior
   ├─ Log: "Iniciando ConsultaDinamicaQuery"
   ├─ Inicia Stopwatch
   └─ Chama: next()
   ↓
5. ValidationBehavior
   ├─ Resolve: ConsultaDinamicaQueryValidator
   ├─ Valida: Tabela in whitelist? ✅
   ├─ Valida: Profundidade 1-3? ✅
   └─ Chama: next()
   ↓
6. ConsultaDinamicaQueryHandler (Orquestrador)
   └─ Chama: _domainService.ConsultarTabelaAsync(...)
   ↓
7. ConsultaDinamicaDomainService (Lógica de Negócio)
   ├─ ValidarTabelaPermitida("CLIENTES") ✅
   ├─ ValidarProfundidade(2) ✅
   ├─ Chama: _MetaQuery.MontarQuery(...)
   ├─ Chama: _repository.ExecutarQueryAsync(...)
   ├─ ValidarLimiteRegistros(150) ✅
   └─ Retorna: ConsultaDinamicaResult
   ↓
8. Volta para ValidationBehavior → LoggingBehavior
   ├─ Para Stopwatch: 87ms
   ├─ Log: "ConsultaDinamicaQuery executado em 87ms"
   └─ Retorna resultado
   ↓
9. Volta para Controller
   ├─ Verifica: _notificationContext.HasNotifications? ❌
   ├─ Retorna: Ok(200) com dados
   ↓
10. HTTP Response 200 OK
```

---

## 🎨 Quando Usar Cada Componente

### ✅ USE DomainService quando:

1. **Lógica envolve múltiplos Repositories**
   ```csharp
   public async Task<TabelaDinamica> ObterComVinculosAsync(int id)
   {
       var metadado = await _metadadosRepo.ObterPorIdAsync(id);
       var vinculos = await _metadadosRepo.ObterPorVinculoAsync(metadado.Tabela);
       return EnriquecerMetadado(metadado, vinculos); // Lógica complexa
   }
   ```

2. **Validações de negócio complexas**
   ```csharp
   public async Task CriarAsync(TabelaDinamica tabela)
   {
       await ValidarTabelaExisteNoBancoAsync(tabela.Tabela);
       await ValidarCamposExistemAsync(tabela.Tabela, tabela.CamposDisponiveis);
       await ValidarVinculosAsync(tabela.VinculoEntreTabela);
       return await _repository.CriarAsync(tabela);
   }
   ```

3. **Lógica pode ser reutilizada**
   ```csharp
   // Usado pelo QueryHandler
   var resultado = await _domainService.ConsultarTabelaAsync(...);

   // Usado por um BackgroundJob
   var dados = await _domainService.ConsultarTabelaAsync(...);
   ```

### ❌ NÃO USE DomainService quando:

1. **Lógica é trivial** (1 linha, apenas passa dados)
   ```csharp
   // ❌ DomainService inútil
   public async Task<TabelaDinamica> ObterAsync(int id)
   {
       return await _repository.ObterPorIdAsync(id);
   }

   // ✅ Handler pode chamar Repository direto
   return await _repository.ObterPorIdAsync(request.Id);
   ```

---

## ✅ USE Service (Auxiliar) quando:

1. **Lógica técnica sem regras de negócio**
   ```csharp
   // QueryBuilderService: monta SQL
   public Query MontarQuery(string tabela, bool joins, int profundidade)
   {
       // Apenas monta SQL, sem validações de negócio
   }
   ```

2. **Lógica stateless reutilizável**
   ```csharp
   // EncryptionService: criptografa/descriptografa
   // EmailService: envia emails
   // LoggingService: formata logs
   ```

---

## 📝 Justificativa para Diferença com Herval

### Por que não seguimos 100% o padrão Herval?

**Razões técnicas:**

1. **Sustentabilidade a longo prazo**
   - CQRS completo facilita evolução para Event Sourcing
   - Padrão reconhecido na comunidade .NET
   - Documentação e suporte abundantes

2. **Cross-Cutting Concerns**
   - Behaviors funcionam automaticamente para READ e WRITE
   - Cache, retry policies, circuit breakers fáceis de adicionar
   - Logs e validações uniformes

3. **Testabilidade Superior**
   - Controller testa apenas orquestração (mock IMediator)
   - DomainService testa lógica de negócio isoladamente
   - Handlers testam integração

4. **Performance Futura**
   - Queries podem ir para read replicas
   - Commands vão para master database
   - CQRS facilita eventual consistency

5. **Escalabilidade**
   - Fácil separar em microserviços
   - Queries e Commands podem ser projetos separados
   - Event-driven architecture possível

**Razões de negócio:**

- Projeto MVP focado em **evolução contínua**
- Time pequeno, mas **expectativa de crescimento**
- **Flexibilidade** para adicionar features complexas (cache, IA, async processing)

---

## 🎯 Conclusão

**Decisão Final:** CQRS Completo + DomainServices

**Justificativa:**
- ✅ Consistência (1 padrão em todo o código)
- ✅ Sustentabilidade (fácil evoluir)
- ✅ Testabilidade (lógica isolada)
- ✅ Flexibilidade (behaviors automáticos)
- ✅ Padrão de mercado (.NET)

**Trade-off aceito:**
- ⚠️ Mais código inicial (vs. Herval pragmático)
- ⚠️ Curva de aprendizado (1 semana vs. 2 dias)
- ⚠️ Diferente do padrão Herval (mas documentado e justificado)

**Benefício esperado:**
- 🚀 Projeto escalável para 2+ anos
- 🚀 Fácil onboarding de novos devs (.NET padrão)
- 🚀 Redução de débito técnico futuro

---

## 📚 Referências

- [CQRS Pattern - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)

---

## 🔄 Revisões

| Data | Versão | Autor | Mudanças |
|------|--------|-------|----------|
| 2025-11-19 | 1.0 | GitHub Copilot | Criação inicial do documento |
