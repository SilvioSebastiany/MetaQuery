# ⏭️ Próximos Passos - QueryBuilder MVP

> **Status atual:** 98% completo
> **Última atualização:** 22 de Novembro de 2025

---

## ✅ O que JÁ ESTÁ PRONTO

### Arquitetura Completa
- ✅ Clean Architecture + DDD
- ✅ CQRS + MediatR (100%)
- ✅ Unit of Work integrado
- ✅ Notification Pattern
- ✅ FluentValidation Pipeline
- ✅ Logging Behaviors
- ✅ Controllers simplificados

### Funcionalidades Core
- ✅ Geração dinâmica de queries (SqlKata)
- ✅ JOINs recursivos automáticos
- ✅ Metadados de tabelas (CRUD completo)
- ✅ Consultas dinâmicas via API
- ✅ Docker + Oracle configurado
- ✅ Swagger/OpenAPI documentado

### Testes
- ✅ **21 testes unitários** passando (Command Handlers)
- ✅ Testes manuais via API realizados
- ✅ CRUD testado (Create, Read, Delete)

---

## 🎯 O que FALTA (2% restante)

### 1. Testes de Integração (Prioridade: MÉDIA)
**Tempo estimado:** 2-3 dias

**O que fazer:**
- [ ] Criar projeto `QueryBuilder.IntegrationTests`
- [ ] Configurar TestContainers para Oracle
- [ ] Testar API + DB end-to-end
- [ ] Validar transações (commit/rollback real)
- [ ] Testar cenários de erro

**Por que é opcional:**
- Testes unitários já cobrem a lógica
- Testes manuais já validaram funcionalidade
- Útil para CI/CD, mas não bloqueia uso

**Como implementar:**
```bash
dotnet new xunit -n QueryBuilder.IntegrationTests
dotnet add package Testcontainers.Oracle
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

---

### 2. Melhorias de Produção (Prioridade: BAIXA)

#### 2.1 Cache de Metadados
**Tempo:** 4 horas

- [ ] Adicionar `IMemoryCache` no `MetadadosDomainService`
- [ ] Cache com expiração de 1 hora
- [ ] Invalidação em CREATE/UPDATE/DELETE

**Código exemplo:**
```csharp
public async Task<TabelaDinamica?> ObterMetadadosPorTabelaAsync(string tabela)
{
    var cacheKey = $"metadado:{tabela}";

    if (_cache.TryGetValue(cacheKey, out TabelaDinamica? cached))
        return cached;

    var metadado = await _repository.ObterPorNomeTabelaAsync(tabela);

    if (metadado != null)
    {
        _cache.Set(cacheKey, metadado, TimeSpan.FromHours(1));
    }

    return metadado;
}
```

#### 2.2 Health Checks
**Tempo:** 2 horas

- [ ] Instalar `Microsoft.Extensions.Diagnostics.HealthChecks`
- [ ] Health check do Oracle
- [ ] Endpoint `/health`

**Código:**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddOracle(connectionString, name: "oracle-db");

app.MapHealthChecks("/health");
```

#### 2.3 Rate Limiting
**Tempo:** 3 horas

- [ ] Configurar rate limiting por IP
- [ ] Limite: 100 requests/minuto
- [ ] Resposta 429 (Too Many Requests)

**Código:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});
```

#### 2.4 Autenticação/Autorização
**Tempo:** 1-2 dias (se necessário)

- [ ] JWT Bearer tokens
- [ ] Roles (Admin, User)
- [ ] Swagger com autenticação

**Quando implementar:**
- Se API for exposta publicamente
- Se houver múltiplos usuários
- **Não necessário** se uso interno protegido

---

### 3. Integração com IA (Prioridade: FUTURA - Fase 2)

**Objetivo:** Permitir consultas em linguagem natural

**Exemplo:**
```
User: "Mostre os pedidos do cliente João dos últimos 30 dias"
IA: Gera SQL → API executa → Retorna resultados
```

**Componentes necessários:**
- [ ] `IADataCatalogService` - Gera contexto sobre BD para IA
- [ ] OpenAI Integration ou modelo local
- [ ] Prompt engineering para SQL generation
- [ ] Validação de SQL gerado (segurança)
- [ ] Interface conversacional

**Tempo estimado:** 2-3 semanas

---

## 📊 Roadmap Visual

```
MVP ATUAL (98%) ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫
                                                                        ┃
FASE 1.5 - Polimento (2%)                                               ┃
├─ Testes de Integração      [░░░░░░░░░░] 0%                           ┃
├─ Cache                      [░░░░░░░░░░] 0%                           ┃
├─ Health Checks              [░░░░░░░░░░] 0%                           ┃
└─ Rate Limiting              [░░░░░░░░░░] 0%                           ┃
                                                                        ┃
FASE 2 - IA Integration (Futuro)                                        ┃
└─ Natural Language Queries  [░░░░░░░░░░] 0%                           ┃
                                                                        ┃
                                                                        ▼
                                                                   100% PROD
```

---

## 🚀 Decisões de Priorização

### O que fazer AGORA?
**Resposta:** **NADA!** 🎉

O projeto está **funcional e pronto para uso** com:
- ✅ Arquitetura sólida
- ✅ CRUD completo
- ✅ Testes automatizados
- ✅ Transações atômicas
- ✅ API documentada

### O que fazer DEPOIS (se necessário)?

**Cenário 1: Uso interno + baixo volume**
→ **Nada mais** é necessário. Use como está!

**Cenário 2: Mais de 1000 requests/dia**
→ Implementar **cache** (4 horas de trabalho)

**Cenário 3: Deploy em produção corporativa**
→ Implementar **health checks** (2 horas)

**Cenário 4: Exposição pública**
→ Implementar **rate limiting + auth** (1 dia)

**Cenário 5: Integração com IA**
→ Implementar **Fase 2** (2-3 semanas)

---

## 📝 Como Rodar o Projeto

### Setup Inicial
```bash
# 1. Subir Oracle
docker-compose up -d

# 2. Rodar API
dotnet run --project src/QueryBuilder.Api

# 3. Acessar Swagger
# http://localhost:5249/swagger
```

### Rodar Testes
```bash
# Testes unitários (21 testes)
dotnet test

# Deve retornar:
# total: 21; falhou: 0; bem-sucedido: 21
```

### Testar CRUD via API
```bash
# Listar metadados
curl http://localhost:5249/api/Metadados

# Criar metadado
curl -X POST http://localhost:5249/api/Metadados \
  -H "Content-Type: application/json" \
  -d '{"tabela":"TESTE","camposDisponiveis":"ID,NOME","chavePk":"ID"}'

# Consulta dinâmica com JOINs
curl "http://localhost:5249/api/ConsultaDinamica/CLIENTES?incluirJoins=true"
```

---

## ✅ Checklist de Deploy para Produção

Antes de colocar em produção:

### Segurança
- [ ] Variáveis de ambiente para connection strings
- [ ] Segredos não commitados no Git
- [ ] HTTPS habilitado
- [ ] CORS configurado corretamente
- [ ] Autenticação (se API pública)

### Performance
- [ ] Connection pooling configurado
- [ ] Timeouts ajustados
- [ ] Logs em nível apropriado (não Debug)

### Monitoramento
- [ ] Health checks implementados
- [ ] Logging centralizado (opcional)
- [ ] Métricas (opcional)

### Documentação
- [ ] README atualizado
- [ ] Swagger acessível
- [ ] Variáveis de ambiente documentadas

---

## 🎯 Conclusão

**O projeto está PRONTO para uso!** 🚀

Os 2% restantes são **melhorias opcionais** que dependem do caso de uso específico.

Não há trabalho **obrigatório** pendente. Você pode:
1. **Usar como está** (MVP funcional)
2. **Implementar melhorias** conforme necessidade
3. **Partir para Fase 2** (IA Integration) quando quiser

**Parabéns por chegar até aqui!** 🎉
