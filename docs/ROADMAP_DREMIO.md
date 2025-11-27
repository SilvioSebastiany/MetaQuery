# 🔮 Roadmap: Integração MetaQuery + Dremio

> **Status**: 📋 Planejado
> **Prioridade**: Média
> **Esforço Estimado**: 2-3 sprints

---

## 🎯 Objetivo

Integrar o Dremio como camada de virtualização de dados sob o MetaQuery, mantendo toda a inteligência e lógica de negócio existente enquanto ganha os benefícios de multi-fonte e otimização de queries do Dremio.

---

## 💡 Por Que Integrar?

### Problemas Atuais
- ❌ Limitado a Oracle como fonte única
- ❌ Queries complexas podem ser lentas
- ❌ Sem cache nativo de resultados
- ❌ Impossível juntar dados de múltiplas fontes

### Benefícios Esperados
- ✅ **Multi-fonte**: Oracle + PostgreSQL + Parquet (S3) + MongoDB
- ✅ **Performance**: Cache inteligente do Dremio
- ✅ **Otimização**: Query rewriting automático
- ✅ **Escalabilidade**: Cluster distribuído
- ✅ **Zero mudança no frontend**: API permanece igual

---

## 🏗️ Arquitetura Proposta

### Arquitetura Atual (v2.0)
```
┌─────────────┐
│   Frontend  │
└──────┬──────┘
       │ REST API
       ▼
┌─────────────────────────┐
│   MetaQuery API         │
│  - Validações           │
│  - Agrupamento hierárq. │
│  - Transformações       │
└──────┬──────────────────┘
       │ SqlKata + Dapper
       ▼
┌─────────────────────────┐
│      Oracle 21c         │
└─────────────────────────┘
```

### Arquitetura Futura (v3.0)
```
┌─────────────┐
│   Frontend  │
└──────┬──────┘
       │ REST API (sem mudanças)
       ▼
┌─────────────────────────────────┐
│   MetaQuery API                 │
│  - Validações                   │
│  - Agrupamento hierárquico      │
│  - Transformações               │
│  - Cache Redis (novo)           │
└──────┬──────────────────────────┘
       │ SqlKata + Dapper
       ▼
┌─────────────────────────────────┐
│      Dremio (novo)              │
│  - Virtual Data Layer           │
│  - Query optimization           │
│  - Reflections (cache)          │
└──────┬──────────────────────────┘
       │
   ┌───┴────┬─────────┬──────────┐
   ▼        ▼         ▼          ▼
Oracle  Postgres   S3       MongoDB
```

---

## 📋 Plano de Implementação

### Fase 1: Setup e Prova de Conceito (1 semana)

#### 1.1 Instalação do Dremio
```bash
# Docker Compose
version: '3.8'
services:
  dremio:
    image: dremio/dremio-oss:latest
    ports:
      - "9047:9047"  # Web UI
      - "31010:31010" # ODBC/JDBC
      - "32010:32010" # Apache Arrow Flight
    volumes:
      - dremio_data:/opt/dremio/data
```

#### 1.2 Configurar Fonte Oracle no Dremio
- Adicionar Oracle como Data Source
- Criar Views virtuais das tabelas principais
- Testar queries básicas

#### 1.3 Criar Connection String Alternativa
```json
// appsettings.json
{
  "DatabaseSettings": {
    "OracleConnection": "Data Source=oracle:1521/XE;...",
    "DremioConnection": "Data Source=dremio:31010;User Id=admin;...",
    "UseDremio": false  // Feature flag
  }
}
```

#### 1.4 Implementar DataSource Router
```csharp
// Novo arquivo: IDataSourceProvider.cs
public interface IDataSourceProvider
{
    IDbConnection GetConnection();
}

public class DataSourceProvider : IDataSourceProvider
{
    private readonly IConfiguration _config;

    public IDbConnection GetConnection()
    {
        var useDremio = _config.GetValue<bool>("DatabaseSettings:UseDremio");
        var connString = useDremio
            ? _config["DatabaseSettings:DremioConnection"]
            : _config["DatabaseSettings:OracleConnection"];

        var conn = new OracleConnection(connString);
        conn.Open();
        return conn;
    }
}
```

**Entregável**: MetaQuery funcionando com Dremio via feature flag

---

### Fase 2: Dual Source (2 semanas)

#### 2.1 Implementar Query Routing Inteligente
```csharp
public class SmartDataSourceRouter : IDataSourceProvider
{
    public IDbConnection GetConnection(QueryContext context)
    {
        // Queries com JOINs complexos → Dremio (cache)
        if (context.IncluiJoins && context.Profundidade > 1)
            return _dremioConnection;

        // Queries simples → Oracle direto (mais rápido)
        return _oracleConnection;
    }
}
```

#### 2.2 Adicionar Métricas
```csharp
// Monitorar qual fonte está sendo usada
public class QueryMetrics
{
    public string DataSource { get; set; } // "Oracle" ou "Dremio"
    public TimeSpan ExecutionTime { get; set; }
    public int RowCount { get; set; }
}
```

#### 2.3 Implementar Fallback
```csharp
try
{
    // Tenta Dremio primeiro
    return await ExecuteOnDremio(query);
}
catch (DremioUnavailableException)
{
    _logger.LogWarning("Dremio indisponível, usando Oracle direto");
    return await ExecuteOnOracle(query);
}
```

**Entregável**: Sistema híbrido com fallback automático

---

### Fase 3: Cache Distribuído (1 semana)

#### 3.1 Adicionar Redis
```bash
docker-compose.yml
services:
  redis:
    image: redis:alpine
    ports:
      - "6379:6379"
```

#### 3.2 Implementar Cache Layer
```csharp
public class CachedQueryService
{
    private readonly IDistributedCache _cache;

    public async Task<object> ExecuteWithCache(string cacheKey, Func<Task<object>> queryFunc)
    {
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            _metrics.CacheHit();
            return JsonSerializer.Deserialize<object>(cached);
        }

        var result = await queryFunc();
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return result;
    }
}
```

**Entregável**: Cache Redis funcionando

---

### Fase 4: Multi-Fonte (2 semanas)

#### 4.1 Adicionar PostgreSQL ao Dremio
- Configurar Postgres como segunda fonte
- Criar Views federadas (Oracle + Postgres)

#### 4.2 Atualizar TABELA_DINAMICA
```sql
-- Adicionar coluna para indicar fonte
ALTER TABLE TABELA_DINAMICA ADD FONTE VARCHAR2(50) DEFAULT 'ORACLE';

-- Exemplos:
INSERT INTO TABELA_DINAMICA VALUES (..., 'ORACLE');
INSERT INTO TABELA_DINAMICA VALUES (..., 'POSTGRES');
INSERT INTO TABELA_DINAMICA VALUES (..., 'DREMIO_VIEW'); -- View federada
```

#### 4.3 Metadata-Driven Routing
```csharp
var metadata = await _metadadosRepository.ObterPorTabelaAsync(tabela);
var dataSource = metadata.Fonte; // "ORACLE", "POSTGRES", etc.
```

**Entregável**: Queries federadas funcionando

---

## 📊 Comparativo de Arquiteturas

| Aspecto | v2.0 (Atual) | v3.0 (com Dremio) |
|---------|--------------|-------------------|
| Fontes de dados | 1 (Oracle) | N (Oracle, Postgres, S3, etc.) |
| Cache | ❌ | ✅ (Redis + Dremio) |
| Otimização | SqlKata | SqlKata + Dremio |
| Escalabilidade | Vertical | Horizontal (cluster) |
| Complexidade | Baixa | Média |
| Custo infra | $ | $$ |

---

## 🔧 Alterações Necessárias

### Arquivos Novos
```
src/
├── MetaQuery.Infra.Data/
│   ├── Providers/
│   │   ├── IDataSourceProvider.cs         [NOVO]
│   │   ├── DataSourceProvider.cs          [NOVO]
│   │   └── SmartDataSourceRouter.cs       [NOVO]
│   └── Cache/
│       └── CachedQueryService.cs           [NOVO]
```

### Arquivos Modificados
```
src/
├── MetaQuery.Infra.Data/
│   └── Repositories/
│       └── ConsultaDinamicaRepository.cs   [MODIFICADO]
├── MetaQuery.Infra.CrossCutting.IoC/
│   └── DependencyInjection.cs              [MODIFICADO]
└── MetaQuery.Api/
    └── appsettings.json                     [MODIFICADO]
```

### NuGet Packages Adicionais
```xml
<PackageReference Include="StackExchange.Redis" Version="2.7.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
```

---

## ⚠️ Riscos e Mitigações

### Risco 1: Latência Adicional
**Mitigação**: Cache Redis + monitoramento de performance

### Risco 2: Complexidade Operacional
**Mitigação**: Feature flags para rollback rápido

### Risco 3: Custo de Infraestrutura
**Mitigação**: Começar com Dremio OSS (gratuito)

### Risco 4: Breaking Changes
**Mitigação**: API permanece 100% compatível (backward compatible)

---

## 📈 Métricas de Sucesso

### KPIs Técnicos
- [ ] 50% redução no tempo de queries com JOINs
- [ ] 80% cache hit rate
- [ ] 99.9% disponibilidade
- [ ] Zero breaking changes na API

### KPIs de Negócio
- [ ] Suporte a 3+ fontes de dados
- [ ] 10x aumento na capacidade de queries simultâneas
- [ ] Redução de 30% em custos de infraestrutura (cache)

---

## 🚀 Quick Start (Futuro)

```bash
# 1. Subir stack completa
docker-compose up -d

# 2. Configurar Dremio
open http://localhost:9047

# 3. Habilitar feature flag
# appsettings.json
"DatabaseSettings": {
  "UseDremio": true
}

# 4. Rodar testes
dotnet test

# 5. Deploy
dotnet publish -c Release
```

---

## 📚 Referências

- [Dremio Documentation](https://docs.dremio.com/)
- [Dremio JDBC Driver](https://docs.dremio.com/software/drivers/jdbc-driver/)
- [Redis Cache Best Practices](https://redis.io/docs/manual/patterns/)
- [Multi-Source Query Patterns](https://www.dremio.com/resources/)

---

## 🗓️ Timeline Estimado

```
Mês 1: Fase 1 (POC) + Fase 2 (Dual Source)
Mês 2: Fase 3 (Cache) + Fase 4 (Multi-fonte)
Mês 3: Testes, otimização e documentação
```

**Total**: ~3 meses para v3.0 completa

---

## ✅ Checklist de Preparação

- [ ] Provisionar servidor para Dremio (mínimo 8GB RAM)
- [ ] Criar backups do Oracle
- [ ] Documentar queries mais usadas (para otimizar)
- [ ] Definir estratégia de cache (TTL, invalidação)
- [ ] Treinar equipe em Dremio
- [ ] Planejar janela de manutenção
- [ ] Configurar monitoramento (Prometheus + Grafana)

---

**Criado em**: 26/11/2025
**Atualizado em**: 26/11/2025
**Versão**: 1.0
**Status**: 📋 Em Planejamento
