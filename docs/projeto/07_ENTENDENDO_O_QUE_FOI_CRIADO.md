# 📦 Entendendo o Que Foi Criado

## 📋 Visão Geral

Este documento explica **em detalhes** tudo o que já foi criado no projeto, especialmente a infraestrutura Docker e suas dependências.

---

## 🐳 Docker Compose - O Coração da Infraestrutura

### Arquivo: `docker-compose.yaml`

Este arquivo orquestra **2 containers** que trabalham juntos:

```yaml
┌─────────────────────────────────────────────────────────┐
│                    DOCKER COMPOSE                        │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────────────┐       ┌───────────────────────┐│
│  │  oracle-db         │       │  querybuilder-api     ││
│  │  (Banco de Dados)  │◄──────┤  (API .NET)           ││
│  │                    │       │                       ││
│  │  Oracle XE 21c     │       │  ASP.NET Core 9.0     ││
│  │  Porta: 1522       │       │  Porta: 5249          ││
│  └────────────────────┘       └───────────────────────┘│
│           │                                              │
│           ▼                                              │
│  ┌────────────────────┐                                 │
│  │  oracle-data       │                                 │
│  │  (Volume)          │                                 │
│  └────────────────────┘                                 │
│                                                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │         querybuilder-network (Bridge)            │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## 🗄️ Container 1: Oracle Database

### Configuração Detalhada

```yaml
oracle-db:
  container_name: querybuilder-oracle-xe
  image: gvenzl/oracle-xe:21-slim
```

**Nota:** Este documento foca na infraestrutura Docker. Para detalhes sobre a arquitetura de código (controllers, interfaces, padrões), consulte **[04_STATUS_ATUAL.md](04_STATUS_ATUAL.md)**.

**O que significa:**
- **Nome do container:** `querybuilder-oracle-xe` (nome fixo para referência)
- **Imagem:** Oracle Express Edition 21c (versão slim = menor)
- **Fonte:** Docker Hub (gvenzl/oracle-xe)

### Portas Mapeadas

```yaml
ports:
  - "1522:1521"  # Oracle Database
  - "5501:5500"  # Enterprise Manager Web
```

**Como funciona:**
- **1522** (sua máquina) → **1521** (dentro do container)
  - Por que 1522? Para não conflitar com Oracle local (se houver)
  - 1521 é a porta padrão do Oracle

- **5501** (sua máquina) → **5500** (dentro do container)
  - Interface web de administração (opcional)
  - Acesse: `http://localhost:5501/em`

### Variáveis de Ambiente

```yaml
environment:
  - ORACLE_PASSWORD=oracle        # Senha do usuário SYSTEM
  - APP_USER=querybuilder        # Cria usuário adicional
  - APP_USER_PASSWORD=querybuilder123
```

**Usuários criados:**
1. **SYSTEM** / **oracle** (administrador)
2. **querybuilder** / **querybuilder123** (aplicação)

### Volumes (Persistência de Dados)

```yaml
volumes:
  - oracle-data:/opt/oracle/oradata
  - ./Exemplos/script_tabela_dinamica.sql:/docker-entrypoint-initdb.d/01_init.sql:ro
```

**Explicação:**

1. **oracle-data** (volume Docker)
   - Armazena os dados do banco permanentemente
   - Sobrevive a `docker compose down`
   - Só é deletado com `docker compose down -v`

2. **script_tabela_dinamica.sql** (bind mount)
   - Arquivo local: `Exemplos/script_tabela_dinamica.sql`
   - Montado em: `/docker-entrypoint-initdb.d/01_init.sql`
   - `:ro` = read-only (só leitura)
   - Executado automaticamente na primeira inicialização

### Health Check

```yaml
healthcheck:
  test: ["CMD-SHELL", "echo 'SELECT 1 FROM dual;' | sqlplus -s system/oracle@//localhost:1521/XE || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 5
  start_period: 60s
```

**O que faz:**
- Verifica a cada **30 segundos** se o Oracle está respondendo
- Executa query simples: `SELECT 1 FROM dual`
- Dá **60 segundos** iniciais antes de começar a verificar
- Tenta **5 vezes** antes de marcar como unhealthy

**Por que é importante:**
- A API só inicia quando o Oracle estiver **healthy**
- Evita erros de "conexão recusada"

---

## 🌐 Container 2: QueryBuilder API

### Configuração Detalhada

```yaml
querybuilder-api:
  container_name: querybuilder-api
  build:
    context: .
    dockerfile: src/QueryBuilder.Api/Dockerfile
```

**O que significa:**
- **Nome:** `querybuilder-api`
- **Build:** Constrói a partir do Dockerfile
- **Context:** Raiz do projeto (`.`)
- **Dockerfile:** `src/QueryBuilder.Api/Dockerfile`

### Portas Mapeadas

```yaml
ports:
  - "5249:8080"  # HTTP
```

**Como funciona:**
- **5249** (sua máquina) → **8080** (dentro do container)
- Acesse a API: `http://localhost:5249`
- Swagger: `http://localhost:5249/swagger`

### Variáveis de Ambiente

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ASPNETCORE_HTTP_PORTS=8080
  - ASPNETCORE_URLS=http://+:8080
  - DatabaseSettings__ConnectionString=User Id=SYSTEM;Password=oracle;Data Source=oracle-db:1521/XE
  - DatabaseSettings__CommandTimeout=30
  - DatabaseSettings__EnableDetailedErrors=true
```

**Explicação de cada variável:**

1. **ASPNETCORE_ENVIRONMENT=Development**
   - Ativa modo de desenvolvimento
   - Habilita Swagger
   - Mostra erros detalhados

2. **ASPNETCORE_HTTP_PORTS=8080**
   - Define porta interna HTTP

3. **ASPNETCORE_URLS=http://+:8080**
   - Escuta em todas as interfaces na porta 8080

4. **DatabaseSettings__ConnectionString**
   - ⚠️ **IMPORTANTE:** `oracle-db:1521` (não localhost!)
   - `oracle-db` é o nome do container (resolvido pelo Docker)
   - Dentro do Docker, containers se comunicam por nome

5. **DatabaseSettings__CommandTimeout=30**
   - Timeout de 30 segundos para queries

6. **DatabaseSettings__EnableDetailedErrors=true**
   - Mostra erros SQL detalhados (dev apenas)

### Dependência

```yaml
depends_on:
  oracle-db:
    condition: service_healthy
```

**O que faz:**
- A API **só inicia** quando Oracle estiver **healthy**
- Garante que banco está pronto antes da API subir

### Restart Policy

```yaml
restart: unless-stopped
```

**Comportamento:**
- Reinicia automaticamente se cair
- Não reinicia se você parar manualmente

---

## 📁 Dockerfile da API - Multi-Stage Build

### Localização: `src/QueryBuilder.Api/Dockerfile`

Este Dockerfile usa **3 estágios** para otimização:

### Estágio 1: Build

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
```

**O que faz:**
- Usa imagem completa com SDK .NET 9.0
- Cria pasta `/src` como diretório de trabalho

### Copiando Projetos

```dockerfile
COPY ["src/QueryBuilder.Api/QueryBuilder.Api.csproj", "QueryBuilder.Api/"]
COPY ["src/QueryBuilder.Domain/QueryBuilder.Domain.csproj", "QueryBuilder.Domain/"]
# ... outros projetos
```

**Por que copiar só os .csproj primeiro?**
- Docker faz **cache de camadas**
- Se apenas o código muda (não dependências), reutiliza cache
- Restauração de pacotes é lenta → cache acelera builds

### Restaurando Dependências

```dockerfile
RUN dotnet restore "QueryBuilder.Api/QueryBuilder.Api.csproj"
```

**O que faz:**
- Baixa todos os pacotes NuGet
- Cria pasta `obj/` com dependências

### Copiando Código

```dockerfile
COPY src/ .
```

**Agora sim copia todo o código**

### Building

```dockerfile
WORKDIR "/src/QueryBuilder.Api"
RUN dotnet build "QueryBuilder.Api.csproj" -c Release -o /app/build
```

**O que faz:**
- Compila em modo **Release** (otimizado)
- Saída em `/app/build`

### Estágio 2: Publish

```dockerfile
FROM build AS publish
RUN dotnet publish "QueryBuilder.Api.csproj" -c Release -o /app/publish
```

**O que faz:**
- Cria versão final otimizada
- Remove arquivos desnecessários
- Prepara para execução

### Estágio 3: Runtime (Final)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueryBuilder.Api.dll"]
```

**O que faz:**
- Usa imagem **aspnet** (só runtime, menor)
- **Não tem SDK** (segurança + tamanho)
- Copia apenas arquivos publicados do estágio anterior
- Define comando de inicialização

**Resultado:**
- Imagem final: ~220MB (vs ~1GB com SDK)
- Mais segura (sem ferramentas de build)
- Mais rápida para deploy

---

## 🌐 Rede Docker

```yaml
networks:
  querybuilder-network:
    driver: bridge
```

**O que é:**
- Rede interna do Docker
- Tipo **bridge** (padrão)
- Containers podem se comunicar por **nome**

**Como funciona:**
```
API → oracle-db:1521 ✅ (funciona via nome)
API → localhost:1522 ❌ (não funciona - localhost é do container)
```

---

## 💾 Volume Docker

```yaml
volumes:
  oracle-data:
    driver: local
```

**O que é:**
- Armazenamento persistente gerenciado pelo Docker
- Localização: `/var/lib/docker/volumes/` (Linux/Mac) ou equivalente Windows

**Comandos úteis:**
```powershell
# Ver volumes
docker volume ls

# Inspecionar volume
docker volume inspect querybuilder_oracle-data

# Remover volume (⚠️ APAGA DADOS)
docker volume rm querybuilder_oracle-data
```

---

## ⚙️ Configurações da API

### Arquivo: `src/QueryBuilder.Api/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DatabaseSettings": {
    "ConnectionString": "User Id=SYSTEM;Password=oracle;Data Source=localhost:1522/XE",
    "CommandTimeout": 30,
    "EnableDetailedErrors": true
  }
}
```

**Observações importantes:**

1. **localhost:1522** é usado quando roda **localmente** (sem Docker)
2. **oracle-db:1521** é usado quando roda **no Docker**
3. Docker sobrescreve via variável de ambiente

**Precedência:**
```
Variável de ambiente (Docker) > appsettings.json
```

---

## 🔧 Script de Gerenciamento: debug-manager.ps1

### Arquivo: `debug-manager.ps1`

Script PowerShell para facilitar o desenvolvimento.

### Comandos Disponíveis

#### 1. Check (Verificar Porta)
```powershell
.\debug-manager.ps1 check
```

**O que faz:**
- Verifica se porta **5249** está livre
- Mostra qual processo está usando (se houver)
- Retorna PID do processo

#### 2. Free (Liberar Porta)
```powershell
.\debug-manager.ps1 free
```

**O que faz:**
- Para o container `querybuilder-api` se estiver rodando
- Libera porta 5249 para debug local
- Aguarda 2 segundos e verifica novamente

**Quando usar:**
- Quer debugar localmente (F5 no VS Code)
- Docker está usando a porta

#### 3. Docker-Down (Parar Containers)
```powershell
.\debug-manager.ps1 docker-down
```

**O que faz:**
- Para e remove todos os containers
- Remove a rede
- **NÃO** remove volumes (dados persistem)

#### 4. Docker-Up (Iniciar Containers)
```powershell
.\debug-manager.ps1 docker-up
```

**O que faz:**
- Inicia todos os containers em background (`-d`)
- Cria rede se não existir
- Espera Oracle ficar healthy antes de subir API

#### 5. Status (Ver Status Geral)
```powershell
.\debug-manager.ps1 status
```

**O que mostra:**
- Status da porta 5249
- Containers Docker rodando
- Status do Oracle (porta 1522)

**Exemplo de saída:**
```
Status do Ambiente QueryBuilder
==================================

Porta 5249:
Porta 5249 está LIVRE para debug local!

Containers Docker:
NAMES                       STATUS          PORTS
querybuilder-oracle-xe     Up 2 hours      0.0.0.0:1522->1521/tcp

Oracle Database:
  Oracle rodando na porta 1522
```

---

## 🎯 VS Code - Configurações de Debug

### Arquivo: `.vscode/launch.json`

Configurações de debug para diferentes cenários.

### Configuração 1: Debug Local (HTTP)

```json
{
  "name": "QueryBuilder.Api - Debug (HTTP)",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "prepare-api-debug",
  "program": "${workspaceFolder}/src/QueryBuilder.Api/bin/Debug/net9.0/QueryBuilder.Api.dll",
  "env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS": "http://localhost:5249"
  },
  "serverReadyAction": {
    "action": "openExternally",
    "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
    "uriFormat": "%s/swagger"
  }
}
```

**O que faz:**
1. Executa task `prepare-api-debug` antes (build + liberar porta)
2. Roda a API localmente na porta **5249**
3. Quando API estiver pronta, abre Swagger automaticamente

**Fluxo completo:**
```
F5 pressionado
    ↓
prepare-api-debug task executada
    ↓
    ├── free-port-5249 (para container Docker)
    └── build-api (compila código)
    ↓
API inicia localmente
    ↓
Browser abre http://localhost:5249/swagger
```

### Configuração 2: Debug no Container

```json
{
  "name": "QueryBuilder.Api - Attach to Container",
  "type": "coreclr",
  "request": "attach"
}
```

**O que faz:**
- Conecta ao debugger **dentro** do container
- Requer configuração adicional (vsdbg no container)

---

## 📋 VS Code - Tasks

### Arquivo: `.vscode/tasks.json`

Tasks automatizadas para desenvolvimento.

### Task: prepare-api-debug

```json
{
  "label": "prepare-api-debug",
  "dependsOn": [
    "free-port-5249",
    "build-api"
  ],
  "dependsOrder": "sequence"
}
```

**O que faz:**
1. Executa `free-port-5249` (para container)
2. Depois executa `build-api` (compila)
3. Em sequência (não paralelo)

### Task: docker-compose-up

```json
{
  "label": "docker-compose-up",
  "type": "shell",
  "command": "docker",
  "args": ["compose", "up", "-d"]
}
```

**Como usar:**
- `Ctrl+Shift+P` → `Tasks: Run Task` → `docker-compose-up`

### Task: setup-database

```json
{
  "label": "setup-database",
  "dependsOn": [
    "copy-sql-script",
    "init-database"
  ]
}
```

**O que faz:**
1. Copia `init-database.sql` para container
2. Executa script via sqlplus

---

## 🔄 Fluxo de Inicialização Completo

### Quando você executa `docker compose up -d`:

```
1. Docker lê docker-compose.yaml
   ↓
2. Cria rede querybuilder-network
   ↓
3. Cria volume oracle-data (se não existir)
   ↓
4. Puxa imagem gvenzl/oracle-xe:21-slim (se não tiver)
   ↓
5. Inicia container oracle-db
   ↓
   ├── Monta volumes
   ├── Configura variáveis de ambiente
   ├── Executa script de inicialização
   ├── Oracle inicia (~30-60 segundos)
   └── Health check verifica a cada 30s
   ↓
6. Quando Oracle fica HEALTHY:
   ↓
7. Build da API (se necessário)
   ↓
   ├── Stage 1: Restore packages
   ├── Stage 2: Build código
   ├── Stage 3: Publish otimizado
   └── Cria imagem final
   ↓
8. Inicia container querybuilder-api
   ↓
   ├── Conecta à rede
   ├── Injeta variáveis de ambiente
   ├── Espera Oracle estar healthy
   └── API inicia
   ↓
9. API conecta ao Oracle via oracle-db:1521
   ↓
10. Sistema pronto! 🎉
```

---

## 🗂️ Estrutura de Arquivos Docker

```
QueryBuilderMVP/
├── docker-compose.yaml              # Orquestração principal
├── debug-manager.ps1                # Script de gerenciamento
│
├── src/
│   └── QueryBuilder.Api/
│       ├── Dockerfile              # Build da API
│       ├── appsettings.json        # Configurações
│       └── Program.cs              # Entry point
│
├── scripts/
│   └── init-database.sql           # Script de inicialização do banco
│
└── .vscode/
    ├── launch.json                 # Configurações de debug
    └── tasks.json                  # Tasks automatizadas
```

---

## 🔍 Como Verificar Se Está Funcionando

### 1. Verificar Containers

```powershell
docker ps
```

**Deve mostrar:**
```
CONTAINER ID   IMAGE                      STATUS                    PORTS
abc123def456   gvenzl/oracle-xe:21-slim  Up 5 minutes (healthy)    0.0.0.0:1522->1521/tcp
def456ghi789   querybuilder-api          Up 5 minutes              0.0.0.0:5249->8080/tcp
```

### 2. Verificar Logs do Oracle

```powershell
docker logs querybuilder-oracle-xe
```

**Deve ter:**
```
DATABASE IS READY TO USE!
```

### 3. Verificar Logs da API

```powershell
docker logs querybuilder-api
```

**Deve ter:**
```
Now listening on: http://[::]:8080
Application started.
```

### 4. Testar API

```powershell
curl http://localhost:5249/api/metadados/teste
```

**Deve retornar:**
```json
{
  "mensagem": "API QueryBuilder está funcionando! 🚀"
}
```

### 5. Acessar Swagger

Abra no navegador:
```
http://localhost:5249/swagger
```

---

## 🐛 Problemas Comuns e Soluções

### Problema 1: Porta 5249 em uso

**Sintoma:**
```
Error: bind: address already in use
```

**Solução:**
```powershell
.\debug-manager.ps1 free
```

### Problema 2: Oracle não fica healthy

**Sintoma:**
```
querybuilder-api is waiting for oracle-db to be healthy
```

**Causas possíveis:**
- Oracle ainda está inicializando (aguarde 60s)
- Falta de memória (Oracle precisa ~2GB RAM)
- Volume corrompido

**Solução:**
```powershell
# Ver logs
docker logs querybuilder-oracle-xe

# Se necessário, recriar tudo
docker compose down -v
docker compose up -d
```

### Problema 3: API não conecta ao Oracle

**Sintoma:**
```
ORA-12514: TNS:listener does not currently know of service
```

**Causa:**
- Connection string incorreta

**Verificar:**
```yaml
# No docker-compose.yaml deve ser:
DatabaseSettings__ConnectionString=User Id=SYSTEM;Password=oracle;Data Source=oracle-db:1521/XE
                                                                        ^^^^^^^^^ nome do container
```

### Problema 4: Build da API falha

**Sintoma:**
```
Error: failed to build
```

**Solução:**
```powershell
# Build sem cache
docker compose build --no-cache querybuilder-api
docker compose up -d
```

---

## 📊 Resumo Visual

```
┌─────────────────────────────────────────────────────────────┐
│ VOCÊ (Developer)                                            │
│                                                             │
│  VS Code                  Terminal                          │
│    │                        │                               │
│    ├─ F5 (Debug)           ├─ docker compose up -d         │
│    ├─ Tasks                ├─ .\debug-manager.ps1          │
│    └─ Swagger              └─ curl http://localhost:5249   │
└──────────────┬───────────────────────┬──────────────────────┘
               │                       │
               ▼                       ▼
┌─────────────────────────────────────────────────────────────┐
│ DOCKER (Orquestração)                                       │
│                                                             │
│  ┌─────────────────────┐     ┌───────────────────────────┐│
│  │ oracle-db           │     │ querybuilder-api          ││
│  │ localhost:1522      │◄────┤ localhost:5249            ││
│  │ (Oracle XE 21c)     │     │ (.NET 9.0 API)            ││
│  └──────────┬──────────┘     └───────────────────────────┘│
│             │                                               │
│             ▼                                               │
│  ┌─────────────────────┐                                   │
│  │ oracle-data         │                                   │
│  │ (Volume Persistente)│                                   │
│  └─────────────────────┘                                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Pontos-Chave Para Entender

### 1. Docker Compose coordena tudo
- Define serviços, redes e volumes
- Gerencia dependências entre containers
- Uma linha (`docker compose up -d`) sobe tudo

### 2. Containers são isolados
- API no Docker usa `oracle-db:1521`
- API local usa `localhost:1522`
- Portas mapeadas permitem acesso externo

### 3. Volumes garantem persistência
- Dados sobrevivem a `docker compose down`
- Só são perdidos com `-v` (volumes)

### 4. Health checks são críticos
- API só sobe quando Oracle está pronto
- Evita erros de conexão

### 5. Multi-stage build otimiza
- Imagem final pequena e segura
- Sem SDK no runtime

---

<div align="center">

**🐳 Docker = Ambiente reproduzível e isolado! 📦**

[← Voltar ao Índice](00_INDICE.md)

</div>
