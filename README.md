# 🚀 QueryBuilder MVP - Sistema de Consultas Dinâmicas

> **Sistema inteligente de consultas dinâmicas ao banco de dados Oracle com geração automática de queries baseada em metadados**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Oracle](https://img.shields.io/badge/Oracle-21c%20XE-F80000?style=flat&logo=oracle)](https://www.oracle.com/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat&logo=docker)](https://www.docker.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green?style=flat)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

## 📖 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Objetivo e Motivação](#-objetivo-e-motivação)
- [Aprendizados Técnicos](#-aprendizados-técnicos)
- [Arquitetura](#-arquitetura)
- [Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [Quick Start](#-quick-start)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Como Funciona](#-como-funciona)
- [Endpoints da API](#-endpoints-da-api)
- [Gerenciamento de Ambiente](#-gerenciamento-de-ambiente)
- [Troubleshooting](#-troubleshooting)
- [Roadmap](#-roadmap)

---

## 🎯 Sobre o Projeto

O **QueryBuilder MVP** é um sistema que revoluciona a forma de consultar bancos de dados, eliminando a necessidade de escrever SQL repetitivo. Através de uma **tabela de metadados** (`TABELA_DINAMICA`), o sistema aprende sobre a estrutura do banco e **gera queries automaticamente**, incluindo JOINs complexos e relacionamentos entre tabelas.

### 💡 O Problema Resolvido

**Antes:**
```csharp
// Para cada nova consulta, escrever SQL manualmente...
var query1 = new Query("PEDIDOS")
    .Select("PEDIDOS.*", "CLIENTES.NOME")
    .Join("CLIENTES", "CLIENTES.ID", "PEDIDOS.ID_CLIENTE")
    .Join("ENDERECOS", "ENDERECOS.ID", "CLIENTES.ID_ENDERECO");

var query2 = new Query("PRODUTOS")
    .Select("PRODUTOS.*", "CATEGORIAS.NOME")
    .Join("CATEGORIAS", "CATEGORIAS.ID", "PRODUTOS.ID_CATEGORIA");
// Repetir para cada tabela... 😫
```

**Depois:**
```csharp
// Uma única linha para qualquer tabela! 🎉
var query = queryBuilder.MontarQuery("PEDIDOS", incluirJoins: true);
var query2 = queryBuilder.MontarQuery("PRODUTOS", incluirJoins: true);
```

### 🌟 Características Principais

- ✅ **Queries Dinâmicas**: Geração automática baseada em metadados
- ✅ **JOINs Recursivos**: Relacionamentos multi-nível automáticos
- ✅ **Clean Architecture**: Separação clara de responsabilidades
- ✅ **DDD**: Entidades ricas com validações de domínio
- ✅ **RESTful API**: Endpoints prontos para consumo
- ✅ **Docker**: Ambiente completo containerizado
- ✅ **Type-Safe**: SqlKata garante queries válidas
- ✅ **Performance**: Dapper para acesso otimizado ao banco

---

## 🎓 Objetivo e Motivação

### Por Que Este Projeto?

Este projeto nasceu como uma **jornada de aprendizado prático** em desenvolvimento backend moderno com .NET, combinando diversos conceitos avançados:

1. **Reduzir Código Repetitivo**: Eliminar a necessidade de escrever SQL manualmente para cada tabela
2. **Aprender Clean Architecture**: Aplicar separação de camadas na prática
3. **Dominar DDD**: Implementar entidades ricas, agregados e value objects
4. **Trabalhar com Oracle**: Experiência com banco enterprise
5. **Containerização**: Orquestrar ambiente completo com Docker
6. **Query Builders**: Usar SqlKata para gerar SQL type-safe
7. **APIs RESTful**: Construir endpoints seguindo boas práticas

### Casos de Uso Reais

- 🏢 **APIs Genéricas**: Um único endpoint para consultar qualquer tabela
- 📊 **Relatórios Dinâmicos**: Usuários escolhem tabelas e filtros na tela
- 🏗️ **Multi-Tenant**: Cada cliente pode ter estrutura diferente
- 🔌 **Integrações**: Sistema se adapta automaticamente a mudanças de schema
- 🤖 **IA/Assistentes**: Fornecer contexto estruturado sobre o banco

---

## 🎓 Aprendizados Técnicos

Durante o desenvolvimento deste projeto, foram aplicados e aprofundados os seguintes conceitos:

### 1. **Clean Architecture & DDD**
- ✅ Separação em camadas (Domain, Application, Infrastructure, API)
- ✅ Inversão de dependências com interfaces
- ✅ Entidades ricas com encapsulamento (`TabelaDinamica`)
- ✅ Value Objects imutáveis (`CampoTabela`, `VinculoTabela`)
- ✅ Factory Methods para criação consistente
- ✅ Validações no domínio

**Exemplo Prático:**
```csharp
// Entity com encapsulamento e validações
var metadado = TabelaDinamica.Criar(
    tabela: "CLIENTES",
    camposDisponiveis: "ID,NOME,EMAIL",
    chavePk: "ID"
);

metadado.AtualizarVinculo("PEDIDOS:ID_CLIENTE:ID");
metadado.AlterarVisibilidadeIA(true);
```

### 2. **Dependency Injection & IoC**
- ✅ Container de DI configurado manualmente
- ✅ Registro de dependências por camada
- ✅ Lifetime management (Scoped, Singleton, Transient)
- ✅ Pattern Repository abstraindo acesso a dados

**Exemplo Prático:**
```csharp
// DependencyInjection.cs
services.AddScoped<IDbConnection>(provider =>
{
    var connection = new OracleConnection(connectionString);
    connection.Open();
    return connection;
});

services.AddScoped<IMetadadosRepository, MetadadosRepository>();
```

### 3. **Dapper + Oracle**
- ✅ Micro-ORM para performance
- ✅ Queries parametrizadas (prevenção de SQL Injection)
- ✅ Mapeamento automático para entities
- ✅ Tratamento de tipos Oracle específicos
- ✅ Transações e gerenciamento de conexões

**Exemplo Prático:**
```csharp
public async Task<TabelaDinamica?> ObterPorIdAsync(int id)
{
    const string sql = @"
        SELECT * FROM TABELA_DINAMICA
        WHERE ID = :Id";

    return await _connection.QueryFirstOrDefaultAsync<TabelaDinamica>(
        sql,
        new { Id = id }
    );
}
```

### 4. **SqlKata Query Builder**
- ✅ Queries fluentes e type-safe
- ✅ Compilação para diferentes dialetos SQL
- ✅ Suporte a JOINs, WHERE, ORDER BY, paginação
- ✅ Prevenção de SQL injection
- ✅ Queries dinâmicas baseadas em metadados

**Exemplo Prático:**
```csharp
var query = new Query("PEDIDOS")
    .Select("PEDIDOS.*")
    .Join("CLIENTES", "CLIENTES.ID", "PEDIDOS.ID_CLIENTE")
    .Where("PEDIDOS.STATUS", "ATIVO")
    .OrderBy("PEDIDOS.DATA_PEDIDO");

var compiled = compiler.Compile(query);
// SELECT * FROM PEDIDOS JOIN CLIENTES ON...
```

### 5. **Docker & Containerização**
- ✅ docker-compose.yaml para orquestração
- ✅ Multi-stage builds para otimização
- ✅ Redes internas entre containers
- ✅ Volumes para persistência de dados
- ✅ Healthchecks e dependências entre serviços

### 6. **ASP.NET Core Web API**
- ✅ Controllers RESTful
- ✅ Swagger/OpenAPI para documentação
- ✅ DTOs e validação de entrada
- ✅ Tratamento de erros consistente
- ✅ Logging estruturado

### 7. **Conceitos Avançados**
- ✅ **Algoritmos de Grafos**: JOINs recursivos com BFS/DFS
- ✅ **Metaprogramação**: Geração dinâmica de código
- ✅ **Padrões de Projeto**: Repository, Factory, Builder
- ✅ **Segurança**: WhiteList, validação, prevenção de loops

---

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** (Arquitetura Limpa) com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│              (QueryBuilder.Api)                             │
│         Controllers │ DTOs │ Swagger                        │
└───────────────────────┬─────────────────────────────────────┘
                        │ Depende ↓
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│              (QueryBuilder.Domain)                          │
│    Entities │ ValueObjects │ Interfaces │ Commands          │
└───────────────────────┬─────────────────────────────────────┘
                        │ Implementado por ↓
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│                                                              │
│  ┌────────────────────┐  ┌─────────────────────────────┐   │
│  │ Infra.Data         │  │ Infra.Externals             │   │
│  │ • Repositories     │  │ • APIs externas             │   │
│  │ • Dapper          │  │ • OpenAI Integration        │   │
│  │ • Oracle          │  │                             │   │
│  └────────────────────┘  └─────────────────────────────┘   │
│                                                              │
│  ┌────────────────────┐  ┌─────────────────────────────┐   │
│  │ CrossCutting       │  │ CrossCutting.IoC            │   │
│  │ • Settings         │  │ • Dependency Injection      │   │
│  │ • Extensions       │  │ • Service Registration      │   │
│  └────────────────────┘  └─────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 📂 Estrutura de Diretórios

```
QueryBuilderMVP/
├── src/
│   ├── QueryBuilder.Api/                    # 🌐 Camada de Apresentação
│   │   ├── Controllers/
│   │   │   └── MetadadosController.cs      # Endpoints REST
│   │   ├── Responses/                      # DTOs de resposta
│   │   ├── Program.cs                      # Configuração da API
│   │   ├── appsettings.json               # Configurações
│   │   └── Dockerfile                     # Build da API
│   │
│   ├── QueryBuilder.Domain/                 # 🎯 Camada de Domínio
│   │   ├── Entities/
│   │   │   └── TabelaDinamica.cs          # Agregado raiz
│   │   ├── ValueObjects/
│   │   │   └── MetadadosValueObjects.cs   # CampoTabela, VinculoTabela
│   │   ├── Interfaces/
│   │   │   └── IRepositories.cs           # Contratos
│   │   ├── Services/                      # Lógica de domínio
│   │   └── Commands/                      # CQRS Commands
│   │
│   ├── QueryBuilder.Infra.Data/             # 💾 Acesso a Dados
│   │   ├── Repositories/
│   │   │   └── MetadadosRepository.cs     # Dapper + Oracle
│   │   └── Context/                       # DbContext
│   │
│   ├── QueryBuilder.Infra.Externals/        # 🔌 Serviços Externos
│   │   └── Services/                      # APIs externas
│   │
│   ├── QueryBuilder.Infra.CrossCutting/     # 🛠️ Recursos Compartilhados
│   │   ├── Settings/
│   │   │   └── DatabaseSettings.cs        # Configurações
│   │   └── Extensions/                    # Extension methods
│   │
│   └── QueryBuilder.Infra.CrossCutting.IoC/ # 💉 Injeção de Dependência
│       └── DependencyInjection.cs         # Container de DI
│
├── scripts/                                 # 📜 Scripts SQL
│   ├── init-database.sql                  # Criação da TABELA_DINAMICA
│   ├── check-table.sql                    # Verificação
│   └── count-records.sql                  # Contagem de registros
│
├── docs/                                    # 📚 Documentação
│   ├── COMANDOS.md                        # Comandos úteis
│   ├── DOCKER_README.md                   # Guia Docker
│   ├── EXEMPLO_08_METADADOS.md           # Tutorial metadados
│   └── STATUS_MIGRACAO.md                # Status do projeto
│
├── docker-compose.yaml                      # 🐳 Orquestração Docker
├── debug-manager.ps1                        # 🔧 Gerenciamento de debug
├── api-tests.http                          # 🧪 Testes da API
├── QueryBuilder.Solution.sln               # 📦 Solution .NET
└── README.md                               # 📖 Este arquivo
```

---

## 🛠️ Tecnologias Utilizadas

### Backend & Framework
- **[.NET 9.0](https://dotnet.microsoft.com/)** - Framework principal
- **[ASP.NET Core Web API](https://learn.microsoft.com/aspnet/core/)** - REST API
- **[C# 12](https://learn.microsoft.com/dotnet/csharp/)** - Linguagem de programação

### Banco de Dados & ORM
- **[Oracle Database 21c XE](https://www.oracle.com/database/technologies/xe-downloads.html)** - Banco de dados relacional
- **[SqlKata 4.0.1](https://sqlkata.com/)** - Query Builder fluente e type-safe
- **[Dapper 2.1.66](https://github.com/DapperLib/Dapper)** - Micro-ORM de alta performance
- **[Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/)** - Driver Oracle

### Arquitetura & Padrões
- **Clean Architecture** - Separação de responsabilidades
- **Domain-Driven Design (DDD)** - Modelagem orientada ao domínio
- **Repository Pattern** - Abstração de acesso a dados
- **Dependency Injection** - Inversão de controle

### DevOps & Containerização
- **[Docker](https://www.docker.com/)** - Containerização
- **[Docker Compose](https://docs.docker.com/compose/)** - Orquestração de containers

### Ferramentas de Desenvolvimento
- **[Visual Studio Code](https://code.visualstudio.com/)** - IDE
- **[C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)** - Extensão VS Code
- **[REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)** - Testar APIs
- **[PowerShell](https://learn.microsoft.com/powershell/)** - Scripts de automação

### Documentação & API
- **[Swagger/OpenAPI](https://swagger.io/)** - Documentação automática da API
- **[Markdown](https://www.markdownguide.org/)** - Documentação do projeto

---

## 🚀 Quick Start

### Pré-requisitos

Certifique-se de ter instalado:

- ✅ [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop)
- ✅ [VS Code](https://code.visualstudio.com/) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- ✅ [Git](https://git-scm.com/)

### Passo 1: Clonar o Repositório

```powershell
git clone https://github.com/SilvioSebastiany/QueryBuilderMVP.git
cd QueryBuilderMVP
```

### Passo 2: Iniciar Ambiente Docker

```powershell
# Subir Oracle Database
docker compose up -d oracle-db

# Aguardar Oracle inicializar (30-60 segundos)
docker logs -f querybuilder-oracle-xe
# Aguarde ver: "DATABASE IS READY TO USE!"
```

### Passo 3: Inicializar Banco de Dados

```powershell
# Copiar script SQL para o container
docker cp scripts/init-database.sql querybuilder-oracle-xe:/tmp/

# Executar script de inicialização
Get-Content scripts\init-database.sql | docker exec -i querybuilder-oracle-xe sqlplus -s system/oracle@XE
```

**Ou use a task do VS Code:**
- `Ctrl+Shift+P` → `Tasks: Run Task` → `setup-database`

### Passo 4: Rodar a API

**Opção A: Debug no VS Code (Recomendado)**
```
Pressione F5
```

**Opção B: Via Terminal**
```powershell
dotnet run --project src/QueryBuilder.Api/QueryBuilder.Api.csproj
```

**Opção C: Via Docker**
```powershell
docker compose up -d
```

### Passo 5: Testar a API

**Swagger UI (Interface Gráfica):**
```
http://localhost:5249/swagger
```

**Teste Manual com curl:**
```powershell
# Endpoint de teste
curl http://localhost:5249/api/metadados/teste

# Listar todos os metadados
curl http://localhost:5249/api/metadados

# Buscar por tabela
curl http://localhost:5249/api/metadados/tabela/CLIENTES
```

**Ou use o arquivo `api-tests.http`** (REST Client extension)

---

## 📊 Estrutura do Projeto

### Camada Domain (Núcleo do Sistema)

#### 1. Entity: `TabelaDinamica`
```csharp
public class TabelaDinamica
{
    public int Id { get; private set; }
    public string Tabela { get; private set; }
    public string CamposDisponiveis { get; private set; }
    public string ChavePk { get; private set; }
    public string? VinculoEntreTabela { get; private set; }
    public string? DescricaoTabela { get; private set; }
    public bool VisivelParaIA { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public bool Ativo { get; private set; }

    // Factory method
    public static TabelaDinamica Criar(...) { }

    // Comportamentos
    public void AtualizarCampos(...) { }
    public void AtualizarVinculo(...) { }
    public void AlterarVisibilidadeIA(...) { }
}
```

#### 2. Value Objects
```csharp
public record CampoTabela(string Nome, string Tipo, string? Descricao);
public record VinculoTabela(string TabelaDestino, string CampoFK, string CampoPK);
public record MetadadoDescricao(string Campo, string Descricao);
```

#### 3. Interfaces (Contratos)
```csharp
public interface IMetadadosRepository
{
    Task<IEnumerable<TabelaDinamica>> ObterTodosAsync(bool apenasAtivos = true);
    Task<TabelaDinamica?> ObterPorIdAsync(int id);
    Task<TabelaDinamica?> ObterPorNomeTabelaAsync(string nomeTabela);
    Task<int> CriarAsync(TabelaDinamica metadado);
    Task<bool> AtualizarAsync(TabelaDinamica metadado);
    Task<bool> ExisteAsync(string nomeTabela);
}
```

### Camada Infrastructure

#### 1. Repository (Dapper + Oracle)
```csharp
public class MetadadosRepository : IMetadadosRepository
{
    private readonly IDbConnection _connection;

    public async Task<TabelaDinamica?> ObterPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM TABELA_DINAMICA WHERE ID = :Id";
        return await _connection.QueryFirstOrDefaultAsync<TabelaDinamica>(
            sql,
            new { Id = id }
        );
    }
}
```

#### 2. Dependency Injection
```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Database Connection
    services.AddScoped<IDbConnection>(provider =>
    {
        var connection = new OracleConnection(connectionString);
        connection.Open();
        return connection;
    });

    // Repositories
    services.AddScoped<IMetadadosRepository, MetadadosRepository>();

    return services;
}
```

### Camada API

#### 1. Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class MetadadosController : ControllerBase
{
    private readonly IMetadadosRepository _repository;

    [HttpGet]
    public async Task<IActionResult> ObterTodos([FromQuery] bool apenasAtivos = true)
    {
        var metadados = await _repository.ObterTodosAsync(apenasAtivos);
        return Ok(new { Total = metadados.Count(), Dados = metadados });
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarMetadadoRequest request)
    {
        var metadado = TabelaDinamica.Criar(...);
        var id = await _repository.CriarAsync(metadado);
        return CreatedAtAction(nameof(ObterPorId), new { id }, metadado);
    }
}
```

---

## ⚙️ Como Funciona

### 1. Tabela de Metadados (`TABELA_DINAMICA`)

A tabela armazena informações sobre a estrutura do banco:

```sql
CREATE TABLE TABELA_DINAMICA (
    ID                    NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    TABELA                VARCHAR2(100) UNIQUE NOT NULL,
    CAMPOS_DISPONIVEIS    VARCHAR2(1000) NOT NULL,     -- Ex: "ID,NOME,EMAIL"
    CHAVE_PK              VARCHAR2(100) NOT NULL,      -- Ex: "ID"
    VINCULO_ENTRE_TABELA  VARCHAR2(1000),              -- Ex: "PEDIDOS:ID_CLIENTE:ID"
    DESCRICAO_TABELA      VARCHAR2(500),
    DESCRICAO_CAMPOS      VARCHAR2(2000),
    VISIVEL_PARA_IA       NUMBER(1) DEFAULT 1 NOT NULL,
    DATA_CRIACAO          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    DATA_ATUALIZACAO      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ATIVO                 NUMBER(1) DEFAULT 1 NOT NULL
);
```

### 2. Exemplo de Dados

```sql
INSERT INTO TABELA_DINAMICA VALUES (
    'CLIENTES',
    'ID,NOME,EMAIL,TELEFONE,CPF',
    'ID',
    'PEDIDOS:ID_CLIENTE:ID;ENDERECOS:ID_CLIENTE:ID',
    'Cadastro de clientes do sistema',
    'ID:Identificador único|NOME:Nome completo|EMAIL:E-mail para contato',
    1,
    CURRENT_TIMESTAMP,
    NULL,
    1
);
```

### 3. Geração Automática de Queries

**Entrada:**
```csharp
var metadado = await repository.ObterPorNomeTabelaAsync("CLIENTES");
var campos = metadado.ObterListaCampos(); // ["ID", "NOME", "EMAIL", ...]
var vinculos = metadado.ObterVinculos();  // ["PEDIDOS:ID_CLIENTE:ID", ...]

var query = new Query(metadado.Tabela)
    .Select(campos.ToArray());

// Adicionar JOINs automaticamente
foreach (var vinculo in vinculos)
{
    // Parse: "PEDIDOS:ID_CLIENTE:ID" → JOIN PEDIDOS ON PEDIDOS.ID_CLIENTE = CLIENTES.ID
    query.Join(...);
}
```

**SQL Gerado:**
```sql
SELECT ID, NOME, EMAIL, TELEFONE, CPF
FROM CLIENTES
JOIN PEDIDOS ON PEDIDOS.ID_CLIENTE = CLIENTES.ID
JOIN ENDERECOS ON ENDERECOS.ID_CLIENTE = CLIENTES.ID
```

### 4. Fluxo Completo

```
┌──────────┐       ┌──────────────┐       ┌──────────────┐       ┌───────────┐
│  Cliente │ ───>  │   API        │ ───>  │  Repository  │ ───>  │  Oracle   │
│  HTTP    │       │  Controller  │       │  + Dapper    │       │  Database │
└──────────┘       └──────────────┘       └──────────────┘       └───────────┘
     │                    │                       │                      │
     │  GET /api/         │                       │                      │
     │  metadados         │                       │                      │
     │─────────────────>  │                       │                      │
     │                    │  ObterTodosAsync()    │                      │
     │                    │─────────────────────> │                      │
     │                    │                       │  SELECT * FROM ...   │
     │                    │                       │ ──────────────────>  │
     │                    │                       │  <Resultados>        │
     │                    │                       │ <─────────────────   │
     │                    │  <List<Metadados>>    │                      │
     │                    │ <────────────────────  │                      │
     │  JSON Response     │                       │                      │
     │ <──────────────────│                       │                      │
```

---

## 🎯 Endpoints da API

### Base URL
```
http://localhost:5249/api/metadados
```

### 1. Endpoint de Teste
```http
GET /api/metadados/teste
```

**Resposta:**
```json
{
  "mensagem": "API QueryBuilder está funcionando! 🚀",
  "versao": "1.0.0",
  "timestamp": "2025-11-12T21:48:18.469Z",
  "endpoints": [
    "GET /api/metadados/teste",
    "GET /api/metadados",
    "GET /api/metadados/{id}",
    "GET /api/metadados/tabela/{nome}",
    "POST /api/metadados"
  ]
}
```

### 2. Listar Todos os Metadados
```http
GET /api/metadados?apenasAtivos=true
```

**Resposta:**
```json
{
  "total": 6,
  "dados": [
    {
      "id": 1,
      "tabela": "CLIENTES",
      "camposDisponiveis": "ID,NOME,EMAIL,TELEFONE,CPF",
      "chavePk": "ID",
      "vinculoEntreTabela": "PEDIDOS:ID_CLIENTE:ID;ENDERECOS:ID_CLIENTE:ID",
      "descricaoTabela": "Cadastro de clientes do sistema",
      "visivelParaIA": true,
      "ativo": true,
      "dataCriacao": "2025-11-12T21:48:18.472Z"
    }
  ]
}
```

### 3. Buscar por ID
```http
GET /api/metadados/1
```

### 4. Buscar por Nome da Tabela
```http
GET /api/metadados/tabela/CLIENTES
```

### 5. Criar Novo Metadado
```http
POST /api/metadados
Content-Type: application/json

{
  "tabela": "FORNECEDORES",
  "camposDisponiveis": "ID,NOME,CNPJ,EMAIL",
  "chavePk": "ID",
  "vinculoEntreTabela": "PRODUTOS:ID_FORNECEDOR:ID",
  "descricaoTabela": "Cadastro de fornecedores",
  "visivelParaIA": true
}
```

**Resposta:**
```json
{
  "id": 7,
  "mensagem": "Metadado criado com sucesso"
}
```

---

## 🔧 Gerenciamento de Ambiente

### Script `debug-manager.ps1`

Script PowerShell para facilitar o gerenciamento do ambiente de desenvolvimento:

```powershell
# Verificar status completo (portas, containers, processos)
.\debug-manager.ps1 status

# Liberar porta 5249 para debug local
.\debug-manager.ps1 free

# Verificar se porta 5249 está em uso
.\debug-manager.ps1 check

# Parar containers Docker
.\debug-manager.ps1 docker-down

# Iniciar containers Docker
.\debug-manager.ps1 docker-up
```

### Tasks do VS Code

Pressione `Ctrl+Shift+P` → `Tasks: Run Task` e escolha:

- **`build`** - Compilar a API
- **`build-all`** - Compilar toda a solution
- **`test`** - Executar testes
- **`watch-api`** - Watch mode (recompila automaticamente)
- **`docker-compose-up`** - Subir containers
- **`docker-compose-down`** - Parar containers
- **`setup-database`** - Inicializar banco de dados
- **`free-port-5249`** - Liberar porta de debug
- **`check-port-5249`** - Verificar porta

### Comandos Docker Úteis

```powershell
# Ver containers rodando
docker ps

# Ver logs da API
docker logs -f querybuilder-api

# Ver logs do Oracle
docker logs -f querybuilder-oracle-xe

# Entrar no container Oracle
docker exec -it querybuilder-oracle-xe bash

# Conectar ao SQL*Plus
docker exec -it querybuilder-oracle-xe sqlplus system/oracle@XE

# Verificar saúde do Oracle
docker inspect querybuilder-oracle-xe | Select-String "Health"

# Limpar tudo (⚠️ Remove dados)
docker compose down -v
docker system prune -a --volumes
```

---

## 🐛 Troubleshooting

### Problema: Oracle não conecta

**Sintomas:**
```
ORA-12514: TNS:listener does not currently know of service
ORA-00942: table or view does not exist
```

**Soluções:**

1. **Verificar se container está rodando:**
```powershell
docker ps | Select-String "oracle"
```

2. **Aguardar Oracle inicializar completamente (30-60s):**
```powershell
docker logs -f querybuilder-oracle-xe
# Aguarde ver: "DATABASE IS READY TO USE!"
```

3. **Verificar porta no `appsettings.json`:**
```json
{
  "DatabaseSettings": {
    "ConnectionString": "User Id=SYSTEM;Password=oracle;Data Source=localhost:1522/XE"
  }
}
```

4. **Reinicializar banco:**
```powershell
.\debug-manager.ps1 docker-down
.\debug-manager.ps1 docker-up
```

### Problema: Porta 5249 em uso

**Sintomas:**
```
Address already in use
```

**Soluções:**

```powershell
# Método 1: Script automático
.\debug-manager.ps1 free

# Método 2: Manual
netstat -ano | findstr :5249
# Anotar o PID e então:
Stop-Process -Id <PID> -Force
```

### Problema: Tabela TABELA_DINAMICA não existe

**Sintomas:**
```json
{
  "erro": "Erro ao obter metadados",
  "detalhes": "ORA-00942: a tabela ou view não existe"
}
```

**Soluções:**

```powershell
# Método 1: Via Task do VS Code
# Ctrl+Shift+P → Tasks: Run Task → setup-database

# Método 2: Via PowerShell
docker cp scripts/init-database.sql querybuilder-oracle-xe:/tmp/
Get-Content scripts\init-database.sql | docker exec -i querybuilder-oracle-xe sqlplus -s system/oracle@XE

# Verificar se foi criada
docker exec querybuilder-oracle-xe bash -c "echo 'SELECT COUNT(*) FROM TABELA_DINAMICA;' | sqlplus -s system/oracle@XE"
```

### Problema: API não compila

**Sintomas:**
```
Build failed
```

**Soluções:**

```powershell
# Restaurar dependências
dotnet restore QueryBuilder.Solution.sln

# Limpar e rebuild
dotnet clean QueryBuilder.Solution.sln
dotnet build QueryBuilder.Solution.sln

# Verificar versão do .NET
dotnet --version
# Deve ser 9.0 ou superior
```

### Problema: Docker não sobe

**Sintomas:**
```
Error response from daemon
```

**Soluções:**

```powershell
# Verificar se Docker Desktop está rodando
Get-Process "Docker Desktop"

# Limpar recursos do Docker
docker system prune -a --volumes

# Rebuild sem cache
docker compose build --no-cache
docker compose up -d
```

---

## 🗺️ Roadmap

### ✅ Fase 1: Fundação (Concluído)
- [x] Estrutura Clean Architecture
- [x] Domain Layer com DDD
- [x] Repository Pattern com Dapper
- [x] API REST básica
- [x] Docker Compose
- [x] Scripts SQL de inicialização
- [x] Documentação inicial

### 🚧 Fase 2: Funcionalidades Core (Em Andamento)
- [ ] QueryBuilderService (geração dinâmica de queries)
- [ ] ConsultaDinamicaRepository (execução de queries)
- [ ] ConsultaDinamicaController (endpoint de consulta dinâmica)
- [ ] JOINs recursivos automáticos
- [ ] Filtros dinâmicos (WHERE)
- [ ] Ordenação dinâmica (ORDER BY)
- [ ] Paginação

### 📋 Fase 3: Qualidade & Performance
- [ ] Testes unitários (xUnit)
- [ ] Testes de integração
- [ ] Cache de metadados (Redis)
- [ ] Logging estruturado (Serilog)
- [ ] Health checks
- [ ] Métricas e observabilidade

### 🎨 Fase 4: Melhorias
- [ ] Autenticação e autorização (JWT)
- [ ] Rate limiting
- [ ] CORS configurável
- [ ] Versionamento de API
- [ ] GraphQL endpoint
- [ ] Webhook notifications

### 🤖 Fase 5: IA & Automação
- [ ] Integração com OpenAI
- [ ] Geração de queries em linguagem natural
- [ ] Sugestões automáticas de índices
- [ ] Análise de performance de queries
- [ ] Documentação auto-gerada

### 🌐 Fase 6: Multi-Database
- [ ] Suporte a PostgreSQL
- [ ] Suporte a MySQL
- [ ] Suporte a SQL Server
- [ ] Adapter pattern para múltiplos bancos

---

## 📚 Documentação Adicional

Para mais detalhes, consulte:

- **[COMANDOS.md](docs/COMANDOS.md)** - Comandos úteis do dia a dia
- **[DOCKER_README.md](docs/DOCKER_README.md)** - Guia completo Docker
- **[EXEMPLO_08_METADADOS.md](docs/EXEMPLO_08_METADADOS.md)** - Tutorial de metadados
- **[STATUS_MIGRACAO.md](docs/STATUS_MIGRACAO.md)** - Status da migração
- **[api-tests.http](api-tests.http)** - Coleção de testes da API

---

## 📄 Licença

Este projeto é licenciado sob a **MIT License**.

O SqlKata original também é MIT - veja [SqlKata GitHub](https://github.com/sqlkata/querybuilder).

---

## 👨‍💻 Autor

**Silvio Sebastiany**

- GitHub: [@SilvioSebastiany](https://github.com/SilvioSebastiany)
- Projeto: Aprendizado prático de .NET, Clean Architecture, DDD e Oracle Database

---

## 🙏 Agradecimentos

- **[SqlKata](https://sqlkata.com/)** - Query Builder excepcional
- **[Dapper](https://github.com/DapperLib/Dapper)** - Micro-ORM performático
- **[Oracle](https://www.oracle.com/)** - Banco de dados enterprise
- **[Clean Architecture](https://blog.cleancoder.com/)** - Princípios arquiteturais
- **Comunidade .NET** - Recursos e aprendizado

---

## 📞 Suporte

Encontrou algum problema? Tem alguma sugestão?

1. **Issues**: Abra uma [issue no GitHub](https://github.com/SilvioSebastiany/QueryBuilderMVP/issues)
2. **Documentação**: Consulte a pasta `docs/`
3. **Logs**: Verifique `docker logs` para diagnóstico

---

<div align="center">

**🎉 QueryBuilder MVP - Menos SQL manual, mais produtividade! 🚀**

Feito com ❤️ usando .NET, SqlKata, Dapper e Oracle Database

[⬆ Voltar ao topo](#-querybuilder-mvp---sistema-de-consultas-dinâmicas)

</div>
