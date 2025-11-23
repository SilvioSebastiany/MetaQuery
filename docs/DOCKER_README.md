# QueryBuilder - Docker Setup

## 📦 Containers

Este projeto utiliza Docker Compose com 2 serviços:

### 1. **oracle-db** (Banco de Dados)
- **Imagem:** `gvenzl/oracle-xe:21-slim`
- **Container:** `querybuilder-oracle-xe`
- **Portas:**
  - `1521` - Oracle Database
  - `5500` - Enterprise Manager (opcional)
- **Credenciais:**
  - SYSTEM: `oracle`
  - APP_USER: `querybuilder` / `querybuilder123`

### 2. **querybuilder-api** (.NET API)
- **Build:** Dockerfile customizado (.NET 9.0)
- **Container:** `querybuilder-api`
- **Portas:**
  - `5249` - HTTP (http://localhost:5249)
  - `7249` - HTTPS (https://localhost:7249)

## 🚀 Comandos

### Iniciar todos os serviços
```bash
docker-compose up -d
```

### Verificar status dos containers
```bash
docker-compose ps
```

### Ver logs da API
```bash
docker-compose logs -f querybuilder-api
```

### Ver logs do Oracle
```bash
docker-compose logs -f oracle-db
```

### Parar todos os serviços
```bash
docker-compose down
```

### Parar e remover volumes (⚠️ Remove dados do banco)
```bash
docker-compose down -v
```

### Rebuild da API (após alterações no código)
```bash
docker-compose up -d --build querybuilder-api
```

## 🔧 Configuração

### String de Conexão
A API está configurada para conectar ao Oracle usando a variável de ambiente:
```
User Id=SYSTEM;Password=oracle;Data Source=oracle-db:1521/XE
```

O hostname `oracle-db` é resolvido automaticamente pelo Docker Network.

### Script de Inicialização
O script `Exemplos/script_tabela_dinamica.sql` é executado automaticamente quando o container Oracle é criado pela primeira vez, criando:
- Tabela `TABELA_DINAMICA`
- Dados de exemplo

## 🧪 Testando

### 1. Aguardar os containers iniciarem
```bash
# Monitorar logs até ver "Application started"
docker-compose logs -f querybuilder-api
```

### 2. Testar a API
```bash
# Rota de teste
curl http://localhost:5249/api/metadados/teste

# Swagger UI
http://localhost:5249/swagger
```

### 3. Conectar ao Oracle via SQL*Plus (dentro do container)
```bash
docker exec -it querybuilder-oracle-xe sqlplus system/oracle@XE
```

### 4. Conectar ao Oracle via ferramenta externa
- **Host:** `localhost`
- **Port:** `1521`
- **Service Name:** `XE`
- **User:** `SYSTEM`
- **Password:** `oracle`

## 📁 Estrutura de Arquivos Docker

```
querybuilder/
├── docker-compose.yaml              # Orquestração dos containers
├── .dockerignore                    # Arquivos ignorados no build
├── src/
│   └── MetaQuery.Api/
│       └── Dockerfile               # Build da API .NET
└── Exemplos/
    └── script_tabela_dinamica.sql   # Script de inicialização do DB
```

## 🔄 Workflow de Desenvolvimento

### 1. Primeira vez (Setup completo)
```bash
# Build e start
docker-compose up -d --build

# Aguardar Oracle ficar healthy (30-60s)
docker-compose ps

# Testar API
curl http://localhost:5249/api/metadados/teste
```

### 2. Após alterar código da API
```bash
# Rebuild apenas a API
docker-compose up -d --build querybuilder-api

# Ver logs para confirmar
docker-compose logs -f querybuilder-api
```

### 3. Reset completo (limpar tudo)
```bash
# Parar e remover containers, volumes e networks
docker-compose down -v

# Rebuild tudo
docker-compose up -d --build
```

## 🐛 Troubleshooting

### API não conecta ao Oracle
**Problema:** `ORA-12514: TNS:listener does not currently know of service`

**Solução:**
```bash
# Verificar se Oracle está healthy
docker-compose ps

# Ver logs do Oracle
docker-compose logs oracle-db

# Aguardar até ver "DATABASE IS READY TO USE!"
```

### Container Oracle não inicia
**Problema:** Porta 1521 já em uso

**Solução:**
```bash
# Ver quem está usando a porta
netstat -ano | findstr :1521

# Parar outro Oracle se estiver rodando
# Ou alterar a porta no docker-compose.yaml para "1522:1521"
```

### API build falha
**Problema:** Erro de compilação

**Solução:**
```bash
# Testar build localmente primeiro
dotnet build MetaQuery.Solution.sln

# Limpar cache do Docker
docker-compose build --no-cache querybuilder-api
```

### Dados não persistem após restart
**Problema:** Volume não configurado

**Solução:** O volume `oracle-data` está configurado. Verifique:
```bash
docker volume ls
docker volume inspect querybuilder_oracle-data
```

## 📊 Recursos

### Limites de recursos (opcional)
Adicione ao `docker-compose.yaml` se necessário:

```yaml
querybuilder-api:
  deploy:
    resources:
      limits:
        cpus: '1.0'
        memory: 1G
      reservations:
        cpus: '0.5'
        memory: 512M
```

### Networks
Os containers estão na mesma network `querybuilder-network`, permitindo comunicação por nome:
- API → Oracle: `oracle-db:1521`

## 🔒 Segurança

⚠️ **IMPORTANTE para PRODUÇÃO:**

1. **Alterar senhas padrão** no `docker-compose.yaml`
2. **Usar secrets** do Docker em vez de environment variables
3. **Configurar HTTPS** com certificados válidos
4. **Restringir portas** expostas (usar reverse proxy)
5. **Usar .env** para variáveis sensíveis:

```bash
# Criar .env na raiz
ORACLE_PASSWORD=SuaSenhaSegura123
DATABASE_CONNECTION=User Id=SYSTEM;Password=SuaSenhaSegura123;Data Source=oracle-db:1521/XE
```

```yaml
# Usar no docker-compose.yaml
environment:
  - ORACLE_PASSWORD=${ORACLE_PASSWORD}
  - DatabaseSettings__ConnectionString=${DATABASE_CONNECTION}
```

## ✅ Checklist de Deploy

- [ ] Containers iniciam sem erros
- [ ] Oracle passa no healthcheck
- [ ] API conecta ao Oracle
- [ ] Swagger está acessível
- [ ] Rota de teste retorna 200
- [ ] Dados persistem após restart
- [ ] Logs não mostram erros críticos

---

**🎉 Seu ambiente está dockerizado!** Execute `docker-compose up -d` e acesse http://localhost:5249/swagger
