# 📋 Padrão Arquitetural - Projeto Herval

> **Documentação da arquitetura e padrões utilizados no projeto Ecommerce.Servicos.Produtos da Herval**

Este documento descreve a arquitetura, padrões e boas práticas observados no projeto de referência da empresa Herval, servindo como base para comparação e decisões arquiteturais em novos projetos.

---

## 📑 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura Geral](#-arquitetura-geral)
- [Stack Tecnológica](#-stack-tecnológica)
- [Estrutura de Camadas](#-estrutura-de-camadas)
- [Padrão CQRS](#-padrão-cqrs-no-projeto-herval)
- [Padrões de Design](#-padrões-de-design)
- [Princípios SOLID](#-princípios-solid)
- [Segurança](#-segurança)
- [Comparação com MetaQuery](#-comparação-com-querybuilder-mvp)

---

## 🎯 Visão Geral

O projeto **Ecommerce.Servicos.Produtos** é um microserviço corporativo desenvolvido pela equipe Herval, focado no domínio de Produtos do sistema de e-commerce. Utiliza Clean Architecture com DDD e implementa **CQRS Pragmático** (somente Commands com MediatR, Queries diretas).

### Características Principais

- ✅ **Clean Architecture** - Separação clara de responsabilidades
- ✅ **Domain-Driven Design (DDD)** - Foco no domínio de negócio
- ✅ **CQRS Pragmático** - Commands com MediatR, READ operations diretas
- ✅ **Microserviços** - Serviço isolado e independente
- ✅ **Enterprise Grade** - Pronto para produção em larga escala

---

## 🏗️ Arquitetura Geral

### Tipo de Arquitetura

**Clean Architecture / Onion Architecture**
- Dependências apontam para o centro (Domain)
- Camadas bem definidas e isoladas
- Regras de negócio no núcleo (Domain)
- Infraestrutura nas bordas

**Domain-Driven Design (DDD)**
- Entidades ricas com comportamento
- Agregados bem definidos
- Value Objects para conceitos imutáveis
- Serviços de Domínio para lógica complexa

**CQRS Pragmático**
- ⚠️ **Diferença chave**: Apenas WRITE operations usam MediatR
- Commands → MediatR → Handlers
- Queries → Repository direto (sem MediatR)
- Justificativa: Simplicidade para operações de leitura

---

## 💻 Stack Tecnológica

### Backend
- **.NET 7.0** - Framework principal
- **ASP.NET Core Web API** - REST API
- **C# 11** - Linguagem de programação

### Banco de Dados & ORM
- **Oracle Database** - Banco de dados corporativo
- **Entity Framework Core** - ORM principal
- **Fluent API** - Mapeamento de entidades

### Bibliotecas Principais
- **MediatR** - Mediator pattern (apenas para Commands)
- **FluentValidation** - Validações consistentes
- **Newtonsoft.Json** - Serialização JSON
- **Swagger/OpenAPI** - Documentação de API

### Autenticação & Segurança
- **IdentityServer/OAuth 2.0** - Autenticação centralizada
- **JWT Tokens** - Tokens de acesso
- **Scopes específicos** - Autorização granular

### Observabilidade
- **Graylog** - Logging estruturado e centralizado
- **Health Checks** - Monitoramento de saúde
- **Correlation IDs** - Rastreamento de requisições

### DevOps
- **GitLab CI/CD** - Pipeline automatizado
- **Docker** - Containerização
- **FTP Deploy** - Deploy em servidores Windows

---

## 📦 Estrutura de Camadas

```
Ecommerce.Servicos.Produtos/
├── src/
│   ├── Api/                                    # 🌐 Apresentação
│   │   ├── Controllers/
│   │   ├── Filters/
│   │   ├── Models/
│   │   └── Program.cs
│   │
│   ├── Domain/                                 # 🎯 Núcleo de Negócio
│   │   ├── Commands/                          # Commands (CQRS)
│   │   │   ├── Produtos/
│   │   │   │   ├── IncluirProdutos/
│   │   │   │   │   ├── IncluirProdutosCommand.cs
│   │   │   │   │   ├── IncluirProdutosCommandHandler.cs
│   │   │   │   │   └── IncluirProdutosCommandValidator.cs
│   │   ├── Entities/                          # Entidades de Domínio
│   │   ├── Services/                          # Serviços de Domínio
│   │   ├── Interfaces/                        # Contratos
│   │   │   ├── Repositories/
│   │   │   └── DomainServices/
│   │   ├── ValueObjects/                      # Objetos de Valor
│   │   └── Validators/
│   │
│   ├── Infra.Data/                            # 💾 Acesso a Dados
│   │   ├── Context/
│   │   ├── Repositories/
│   │   ├── Mappings/
│   │   └── Migrations/
│   │
│   ├── Infra.CrossCutting/                    # 🛠️ Utilitários
│   │   ├── Constants/
│   │   ├── Enums/
│   │   ├── Extensions/
│   │   └── Settings/
│   │
│   ├── Infra.CrossCutting.IoC/                # 💉 Injeção de Dependência
│   │   └── DependencyInjection.cs
│   │
│   ├── Infra.ExternalServices.*/              # 🔌 Integrações
│   │   ├── Storex/
│   │   ├── SAP/
│   │   ├── OCC/
│   │   └── Apple/
│   │
│   └── Robo/                                  # 🤖 Jobs e Workers
│       ├── Robo.Consumer/
│       └── Consumers/
```

---

## 🎨 Padrão CQRS no Projeto Herval

### ⚠️ CQRS Pragmático (Não é CQRS Completo)

O projeto Herval implementa uma **variação pragmática do CQRS**:

#### ✅ WRITE Operations (Commands)
```
HTTP POST/PUT/DELETE
    ↓
Controller
    ↓
IMediator.Send(Command)
    ↓
ValidationBehavior (FluentValidation)
    ↓
CommandHandler
    ↓
DomainService (regras de negócio)
    ↓
Repository
    ↓
UnitOfWork.CommitAsync()
```

**Exemplo de Command:**
```csharp
// Command
public class IncluirProdutosCommand : IRequest<string>
{
    public string Nome { get; set; }
    public string Codigo { get; set; }
    public decimal Preco { get; set; }
}

// Handler
public class IncluirProdutosCommandHandler : IRequestHandler<IncluirProdutosCommand, string>
{
    private readonly IProdutoService _produtoService;
    private readonly IProdutoRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly INotificationContext _notifications;

    public async Task<string> Handle(IncluirProdutosCommand request, CancellationToken ct)
    {
        // 1. Validação de negócio
        var produto = await _produtoService.CriarNovoProduto(request);

        if (produto.Invalid)
        {
            _notifications.AddNotifications(produto.ValidationResult);
            return null;
        }

        // 2. Persistência
        await _repository.AddAsync(produto);
        await _uow.CommitAsync();

        return produto.Id;
    }
}

// Validator
public class IncluirProdutosCommandValidator : AbstractValidator<IncluirProdutosCommand>
{
    public IncluirProdutosCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Código é obrigatório")
            .Matches(@"^[A-Z0-9-]+$").WithMessage("Código deve conter apenas letras maiúsculas, números e hífen");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero");
    }
}
```

**Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarProdutoRequest request)
    {
        var command = new IncluirProdutosCommand
        {
            Nome = request.Nome,
            Codigo = request.Codigo,
            Preco = request.Preco
        };

        var resultado = await _mediator.Send(command);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado }, resultado);
    }
}
```

#### ❌ READ Operations (Queries - SEM MediatR)

```
HTTP GET
    ↓
Controller
    ↓
Repository.ObterAsync() → DIRETO (sem MediatR)
    ↓
Entity Framework Query
    ↓
Oracle Database
```

**Exemplo de Query (sem MediatR):**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoRepository _repository;
    private readonly INotificationContext _notifications;

    // ❌ READ direto - NÃO USA MediatR
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(string id)
    {
        var produto = await _repository.ObterPorIdAsync(id);

        if (produto == null)
            return NotFound();

        return Ok(produto);
    }

    // ❌ READ direto - NÃO USA MediatR
    [HttpGet]
    public async Task<IActionResult> ListarTodos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var produtos = await _repository.ObterTodosAsync(pagina, tamanhoPagina);
        var total = await _repository.ContarTotalAsync();

        return Ok(new
        {
            Total = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            Dados = produtos
        });
    }

    // ✅ WRITE usa MediatR
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarProdutoRequest request)
    {
        var command = new IncluirProdutosCommand { /* ... */ };
        var resultado = await _mediator.Send(command);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado }, resultado);
    }
}
```

### Justificativa do Padrão Herval

#### ✅ Vantagens do CQRS Pragmático

1. **Simplicidade para Leitura**
   - Queries diretas são mais simples de entender
   - Menos camadas para operações simples de consulta
   - Developer Experience melhor para queries básicas

2. **Performance**
   - Menos overhead para operações de leitura
   - Sem passagem desnecessária pelo pipeline MediatR
   - Queries diretas são mais rápidas

3. **Menos Código**
   - Não precisa criar Query/QueryHandler para cada consulta
   - Reduz boilerplate para operações simples

4. **Foco no que Importa**
   - MediatR onde faz diferença (Commands com validação)
   - Simplicidade onde não é necessário (Queries básicas)

#### ⚠️ Trade-offs

1. **Inconsistência Arquitetural**
   - Dois padrões diferentes (MediatR vs Direto)
   - Pode confundir novos desenvolvedores
   - Não é CQRS "puro"

2. **Sem Pipeline para Queries**
   - Não aproveita behaviors do MediatR (logging, caching)
   - Validação de queries fica no controller
   - Sem ponto central para cross-cutting concerns de leitura

3. **Dificuldade para Evoluir**
   - Se precisar de cache/logging em queries, precisa refatorar
   - Dificulta implementação de Event Sourcing futuro
   - Não escala bem para queries complexas

---

## 🎯 Camada Domain (Núcleo)

### 1. Entidades de Domínio

**Características:**
- Entidades **RICAS** com comportamento (não anêmicas)
- Encapsulamento de regras de negócio
- Setters privados, métodos públicos para mudança de estado
- Validações internas
- Herança de classe base `TrackableEntity`

**Exemplo:**
```csharp
public class Produto : TrackableEntity<string, string>
{
    // Propriedades com setters privados
    public string Nome { get; private set; }
    public string Codigo { get; private set; }
    public decimal Preco { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }

    // Navegação (relacionamentos)
    public virtual ICollection<ProdutoCategoria> ProdutoCategorias { get; private set; }
    public virtual ICollection<ProdutoSelo> ProdutoSelos { get; private set; }

    // Construtor privado (força uso de Factory Method)
    private Produto() { }

    // Factory Method
    public static Produto Criar(string nome, string codigo, decimal preco)
    {
        var produto = new Produto
        {
            Id = Guid.NewGuid().ToString(),
            Nome = nome,
            Codigo = codigo,
            Preco = preco,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        produto.Validar();
        return produto;
    }

    // Métodos públicos para mudança de estado
    public void AtualizarNome(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            AddNotification("Nome", "Nome não pode ser vazio");

        Nome = novoNome;
        Validar();
    }

    public void AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco <= 0)
            AddNotification("Preco", "Preço deve ser maior que zero");

        Preco = novoPreco;
    }

    public void Ativar()
    {
        if (!PossuiCategorias())
            AddNotification("Ativar", "Produto precisa ter ao menos uma categoria para ser ativado");

        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    // Validações internas
    private void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
            AddNotification("Nome", "Nome é obrigatório");

        if (Nome?.Length > 200)
            AddNotification("Nome", "Nome deve ter no máximo 200 caracteres");

        if (string.IsNullOrWhiteSpace(Codigo))
            AddNotification("Codigo", "Código é obrigatório");

        if (Preco <= 0)
            AddNotification("Preco", "Preço deve ser maior que zero");
    }

    private bool PossuiCategorias()
    {
        return ProdutoCategorias?.Any() ?? false;
    }
}
```

### 2. Value Objects

**Características:**
- Imutáveis (readonly properties)
- Igualdade por valor (não por identidade)
- Sem identidade própria
- Representam conceitos do domínio

**Exemplo:**
```csharp
public class ProdutoSeo : ValueObject
{
    public string MetaTitle { get; }
    public string MetaDescription { get; }
    public string UrlAmigavel { get; }

    public ProdutoSeo(string metaTitle, string metaDescription, string urlAmigavel)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        UrlAmigavel = urlAmigavel;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MetaTitle;
        yield return MetaDescription;
        yield return UrlAmigavel;
    }
}

public class ProdutoConfiguracao : ValueObject
{
    public bool PermiteVendaForaEstoque { get; }
    public int EstoqueMinimo { get; }
    public int EstoqueMaximo { get; }
    public bool ControlaEstoque { get; }

    public ProdutoConfiguracao(
        bool permiteVendaForaEstoque,
        int estoqueMinimo,
        int estoqueMaximo,
        bool controlaEstoque)
    {
        PermiteVendaForaEstoque = permiteVendaForaEstoque;
        EstoqueMinimo = estoqueMinimo;
        EstoqueMaximo = estoqueMaximo;
        ControlaEstoque = controlaEstoque;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PermiteVendaForaEstoque;
        yield return EstoqueMinimo;
        yield return EstoqueMaximo;
        yield return ControlaEstoque;
    }
}
```

### 3. Serviços de Domínio

**Quando usar:**
- Lógica que não pertence a uma entidade específica
- Operações que envolvem múltiplas entidades
- Cálculos complexos
- Integrações com sistemas externos

**Exemplo:**
```csharp
public interface IProdutoService
{
    Task<Produto> CriarNovoProduto(IncluirProdutosCommand command);
    Task<bool> ValidarDisponibilidadeEstoque(string produtoId, int quantidade);
    Task<decimal> CalcularPrecoComDesconto(string produtoId, string codigoCupom);
}

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IEstoqueExternalService _estoqueService;

    public async Task<Produto> CriarNovoProduto(IncluirProdutosCommand command)
    {
        // Validação: Código único
        var produtoExistente = await _produtoRepository.ObterPorCodigoAsync(command.Codigo);
        if (produtoExistente != null)
            throw new DomainException("Já existe um produto com este código");

        // Validação: Categoria existe
        var categoria = await _categoriaRepository.ObterPorIdAsync(command.CategoriaId);
        if (categoria == null)
            throw new DomainException("Categoria não encontrada");

        // Criar entidade
        var produto = Produto.Criar(command.Nome, command.Codigo, command.Preco);

        // Adicionar categoria
        produto.AdicionarCategoria(categoria);

        return produto;
    }

    public async Task<bool> ValidarDisponibilidadeEstoque(string produtoId, int quantidade)
    {
        // Consulta sistema externo de estoque
        var estoqueDisponivel = await _estoqueService.ObterEstoqueAsync(produtoId);
        return estoqueDisponivel >= quantidade;
    }
}
```

### 4. Interfaces (Contratos)

```csharp
// Repositórios
public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(string id);
    Task<Produto?> ObterPorCodigoAsync(string codigo);
    Task<IEnumerable<Produto>> ObterTodosAsync(int pagina, int tamanhoPagina);
    Task<int> ContarTotalAsync();
    Task AddAsync(Produto produto);
    Task UpdateAsync(Produto produto);
    Task DeleteAsync(string id);
}

// Unit of Work
public interface IUnitOfWork
{
    Task<bool> CommitAsync();
    Task RollbackAsync();
}

// Notification Pattern
public interface INotificationContext
{
    bool HasNotifications { get; }
    IReadOnlyCollection<Notification> Notifications { get; }
    void AddNotification(string key, string message);
    void AddNotifications(IEnumerable<Notification> notifications);
}
```

---

## 💾 Camada Infra.Data

### 1. DbContext com Audit Trail

```csharp
public class ProdutosContext : DbContext
{
    private readonly IUserResolverService _userResolverService;

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }

    public ProdutosContext(
        DbContextOptions<ProdutosContext> options,
        IUserResolverService userResolverService) : base(options)
    {
        _userResolverService = userResolverService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplicar todas as configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdutosContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Audit Trail automático
        foreach (var entry in ChangeTracker.Entries<ITrackable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _userResolverService.GetUserId();
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = _userResolverService.GetUserId();
                    entry.Property(nameof(ITrackable.CreatedAt)).IsModified = false;
                    entry.Property(nameof(ITrackable.CreatedBy)).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(ct);
    }
}
```

### 2. Repository Pattern

```csharp
public class ProdutoRepository : IProdutoRepository
{
    private readonly ProdutosContext _context;

    public ProdutoRepository(ProdutosContext context)
    {
        _context = context;
    }

    public async Task<Produto?> ObterPorIdAsync(string id)
    {
        return await _context.Produtos
            .Include(p => p.ProdutoCategorias)
                .ThenInclude(pc => pc.Categoria)
            .Include(p => p.ProdutoSelos)
            .AsNoTracking() // Performance: Não rastrear mudanças
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Produto?> ObterPorCodigoAsync(string codigo)
    {
        return await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == codigo);
    }

    public async Task<IEnumerable<Produto>> ObterTodosAsync(int pagina, int tamanhoPagina)
    {
        return await _context.Produtos
            .Include(p => p.ProdutoCategorias)
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarTotalAsync()
    {
        return await _context.Produtos.CountAsync();
    }

    public async Task AddAsync(Produto produto)
    {
        await _context.Produtos.AddAsync(produto);
    }

    public async Task UpdateAsync(Produto produto)
    {
        _context.Produtos.Update(produto);
    }

    public async Task DeleteAsync(string id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto != null)
            _context.Produtos.Remove(produto);
    }
}
```

### 3. Unit of Work

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ProdutosContext _context;

    public UnitOfWork(ProdutosContext context)
    {
        _context = context;
    }

    public async Task<bool> CommitAsync()
    {
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException ex)
        {
            // Log exception
            throw new InfrastructureException("Erro ao salvar dados no banco", ex);
        }
    }

    public async Task RollbackAsync()
    {
        // Entity Framework não precisa de rollback explícito
        // Apenas não chame SaveChangesAsync()
        await Task.CompletedTask;
    }
}
```

### 4. Fluent Mapping

```csharp
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("PRODUTOS");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("ID")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(p => p.Nome)
            .HasColumnName("NOME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Codigo)
            .HasColumnName("CODIGO")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Preco)
            .HasColumnName("PRECO")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Ativo)
            .HasColumnName("ATIVO")
            .IsRequired();

        builder.Property(p => p.DataCadastro)
            .HasColumnName("DATA_CADASTRO")
            .IsRequired();

        // Índices
        builder.HasIndex(p => p.Codigo)
            .IsUnique()
            .HasDatabaseName("UK_PRODUTOS_CODIGO");

        builder.HasIndex(p => p.Nome)
            .HasDatabaseName("IX_PRODUTOS_NOME");

        // Relacionamentos
        builder.HasMany(p => p.ProdutoCategorias)
            .WithOne(pc => pc.Produto)
            .HasForeignKey(pc => pc.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## 💉 Camada IoC (Dependency Injection)

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR();
        services.AddValidators();
        services.AddDomainServices();
        services.AddRepositories();
        services.AddNotifications();

        return services;
    }

    private static void AddMediatR(this IServiceCollection services)
    {
        var assembly = typeof(IncluirProdutosCommand).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Pipeline de validação
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Pipeline de logging
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    }

    private static void AddValidators(this IServiceCollection services)
    {
        var assembly = typeof(IncluirProdutosCommandValidator).Assembly;

        // Registrar todos os validadores automaticamente
        AssemblyScanner
            .FindValidatorsInAssembly(assembly)
            .ForEach(result => services.AddScoped(result.InterfaceType, result.ValidatorType));
    }

    private static void AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IEstoqueDomainService, EstoqueDomainService>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddNotifications(this IServiceCollection services)
    {
        services.AddScoped<INotificationContext, NotificationContext>();
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ProdutosContext>(options =>
            options.UseOracle(connectionString));

        // External Services
        services.AddExternalServices(configuration);

        return services;
    }

    private static void AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar HttpClients com Polly (retry, circuit breaker)
        services.AddHttpClient<IEstoqueExternalService, EstoqueExternalService>()
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    }
}
```

---

## 🎯 Padrões de Design

### 1. Command/Handler Pattern (MediatR)
- Cada operação de escrita é um Command
- Handler processa o Command
- Validator valida antes da execução

### 2. Repository Pattern
- Abstração de acesso a dados
- Interface no Domain, implementação no Infra
- Queries otimizadas com AsNoTracking

### 3. Unit of Work
- Gerencia transações
- Commit/Rollback centralizado
- Integrado com DbContext

### 4. Notification Pattern
- Coleta de erros sem exceptions
- Validações retornam notificações
- Controller verifica e retorna BadRequest

### 5. Domain Services
- Lógica de negócio que não pertence a entidades
- Orquestração entre múltiplas entidades
- Integrações com serviços externos

### 6. Value Objects
- Objetos imutáveis
- Igualdade por valor
- Representam conceitos do domínio

### 7. Rich Domain Model
- Entidades com comportamento
- Encapsulamento de regras de negócio
- Setters privados, métodos públicos

---

## 🛡️ Princípios SOLID

### ✅ Single Responsibility Principle (SRP)
- Cada classe tem uma única responsabilidade
- Handlers focados em um caso de uso
- Separação de concerns clara

### ✅ Open/Closed Principle (OCP)
- Aberto para extensão via interfaces
- Fechado para modificação
- Novos comportamentos via implementação

### ✅ Liskov Substitution Principle (LSP)
- Herança respeitando contratos
- Interfaces implementadas corretamente
- Polimorfismo adequado

### ✅ Interface Segregation Principle (ISP)
- Interfaces pequenas e focadas
- Clientes não dependem de métodos não utilizados
- Segregação por responsabilidade

### ✅ Dependency Inversion Principle (DIP)
- Dependência de abstrações (interfaces)
- Infraestrutura depende do Domain
- Inversão de controle via DI

---

## 🔐 Segurança

### Autenticação & Autorização
```csharp
// Startup.cs / Program.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["IdentityServer:Authority"];
        options.Audience = configuration["IdentityServer:Audience"];
        options.RequireHttpsMetadata = true;
    });

services.AddAuthorization(options =>
{
    options.AddPolicy("ProdutosLeitura", policy =>
        policy.RequireClaim("scope", "produtos.read"));

    options.AddPolicy("ProdutosEscrita", policy =>
        policy.RequireClaim("scope", "produtos.write"));
});
```

**Controller com Autorização:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Obrigatório por padrão
public class ProdutosController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ProdutosLeitura")]
    public async Task<IActionResult> ListarTodos()
    {
        // ...
    }

    [HttpPost]
    [Authorize(Policy = "ProdutosEscrita")]
    public async Task<IActionResult> Criar([FromBody] CriarProdutoRequest request)
    {
        // ...
    }
}
```

### Validação de Entrada
- FluentValidation em todas as Commands
- Validação no Domain (entidades)
- Sanitização de dados
- Notification Pattern para erros

---

## 📊 Integrações Externas

O projeto integra com diversos sistemas corporativos:

### Sistemas Integrados
- **Storex** - Gestão de estoque
- **SAP** - ERP corporativo
- **OCC (Oracle Commerce Cloud)** - E-commerce B2C
- **Apple** - Integrações específicas
- **AnyMarket** - Marketplace
- **Emarsys** - Marketing automation
- **MultiCD** - Gestão de centros de distribuição
- **SimFrete** - Cálculo de frete

### Padrões de Integração
```csharp
public interface IEstoqueExternalService
{
    Task<int> ObterEstoqueAsync(string produtoId);
    Task<bool> ReservarEstoqueAsync(string produtoId, int quantidade);
}

public class EstoqueExternalService : IEstoqueExternalService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EstoqueExternalService> _logger;

    public EstoqueExternalService(HttpClient httpClient, ILogger<EstoqueExternalService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<int> ObterEstoqueAsync(string produtoId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/estoque/{produtoId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var estoque = JsonConvert.DeserializeObject<EstoqueResponse>(content);

            return estoque.Quantidade;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar estoque do produto {ProdutoId}", produtoId);
            throw new IntegrationException("Erro ao consultar estoque", ex);
        }
    }
}
```

---

## 🎯 Comparação com MetaQuery

| Aspecto | Projeto Herval | MetaQuery |
|---------|----------------|------------------|
| **CQRS** | ⚠️ Pragmático (Commands com MediatR) | ✅ Completo (Queries + Commands) |
| **Queries** | ❌ Repositório direto | ✅ Query + QueryHandler + MediatR |
| **Commands** | ✅ Command + Handler + MediatR | ✅ Command + Handler + MediatR |
| **DomainServices** | ✅ Sim | ✅ Sim |
| **Pipeline Behaviors** | ⚠️ Apenas Commands | ✅ Queries + Commands |
| **Consistência** | ⚠️ Dois padrões diferentes | ✅ Um padrão para tudo |
| **Simplicidade** | ✅ Queries mais simples | ⚠️ Mais código inicial |
| **Testabilidade** | ⚠️ Controller depende de Repository | ✅ Tudo isolado via MediatR |
| **Cross-Cutting** | ⚠️ Apenas WRITE | ✅ READ + WRITE |
| **Escalabilidade** | ⚠️ Limitada para queries | ✅ Escala bem |
| **Curva de Aprendizado** | ✅ Mais fácil | ⚠️ Mais conceitos |

---

## 📝 Convenções de Código

### Nomenclatura
- **Entidades**: PascalCase (ex: `Produto`, `Categoria`)
- **Commands**: `[Verbo][Entidade]Command` (ex: `IncluirProdutosCommand`)
- **Handlers**: `[Command]Handler` (ex: `IncluirProdutosCommandHandler`)
- **Validators**: `[Command]Validator` (ex: `IncluirProdutosCommandValidator`)
- **Repositories**: `I[Entidade]Repository` (ex: `IProdutoRepository`)
- **Services**: `I[Entidade]Service` (ex: `IProdutoService`)

### Organização
- Commands em pastas por agregado/entidade
- Cada Command, Handler e Validator em arquivo separado
- Validators sempre com sufixo `Validator`
- Interfaces no Domain, implementações na Infra

### Código Limpo
- Métodos pequenos (< 20 linhas idealmente)
- Comentários em português para regras de negócio
- Validações claras e mensagens descritivas
- Evitar magic numbers - usar constantes
- Favorecer composição sobre herança

---

## 🎯 Decisão: Por Que MetaQuery Usa CQRS Completo?

### Justificativa da Diferença

Embora o projeto Herval use **CQRS Pragmático**, o MetaQuery optou por **CQRS Completo** pelos seguintes motivos:

#### 1. **Consistência Arquitetural**
- ✅ Um único padrão para tudo (READ + WRITE)
- ✅ Menos confusão para novos desenvolvedores
- ✅ Documentação mais clara

#### 2. **Aproveitamento do Pipeline**
- ✅ Behaviors funcionam para READ + WRITE
- ✅ Logging automático em todas as operações
- ✅ Validação centralizada
- ✅ Caching futuro facilitado

#### 3. **Testabilidade Superior**
- ✅ Todos os casos de uso isolados
- ✅ Mocks mais fáceis (apenas IMediator)
- ✅ Testes unitários simplificados

#### 4. **Sustentabilidade**
- ✅ Fácil adicionar novos behaviors
- ✅ Escala melhor para queries complexas
- ✅ Event Sourcing futuro facilitado

#### 5. **Trade-off Aceitável**
- ⚠️ Mais código inicial → Benefício a longo prazo
- ⚠️ Curva de aprendizado → Padrão consistente compensa
- ⚠️ Overhead mínimo → Performance adequada

### Quando Usar Cada Abordagem?

**Use CQRS Pragmático (Herval) quando:**
- ✅ Time menor ou menos experiente
- ✅ Queries muito simples (CRUD básico)
- ✅ Performance crítica em leitura
- ✅ Prazo apertado
- ✅ Projeto pequeno/médio

**Use CQRS Completo (MetaQuery) quando:**
- ✅ Time experiente ou em aprendizado estruturado
- ✅ Queries complexas com validações
- ✅ Precisa de cross-cutting concerns em READ
- ✅ Projeto de longo prazo
- ✅ Testabilidade é prioridade
- ✅ Escalabilidade futura

---

## 📚 Referências

- **Clean Architecture**: Robert C. Martin (Uncle Bob)
- **Domain-Driven Design**: Eric Evans
- **CQRS Pattern**: Greg Young, Udi Dahan
- **MediatR**: Jimmy Bogard
- **FluentValidation**: Jeremy Skinner
- **Entity Framework Core**: Microsoft

---

## 🎉 Conclusão

O padrão arquitetural da Herval é **robusto, pragmático e testado em produção** em ambiente corporativo de larga escala. Serve como excelente referência para projetos .NET enterprise.

A principal diferença do MetaQuery (CQRS Completo vs Pragmático) é uma **decisão consciente baseada em objetivos de aprendizado e sustentabilidade de longo prazo**, não uma falha ou desconhecimento do padrão Herval.

Ambas as abordagens são válidas e devem ser escolhidas baseadas no **contexto do projeto, time e objetivos**.

---

**Última atualização**: 19 de novembro de 2025
**Autor**: Documentação baseada em análise do projeto Ecommerce.Servicos.Produtos da Herval
