silvio-sebastiany: consegue documentar como o MediatR é configurado nesse projeto

e handler e command, quero implementar em um outro projeto

GitHub Copilot: Vou analisar como o MediatR está configurado neste projeto para documentar a implementação. Deixe-me explorar a estrutura e configuração. Agora vou explorar um exemplo de Command e Handler para entender a estrutura completa: Agora vou ver como um controller utiliza o MediatR: Perfeito! Agora tenho todas as informações necessárias. Vou criar uma documentação completa sobre a configuração do MediatR neste projeto.

## 📚 Documentação - Configuração do MediatR no Projeto

Com base na análise do projeto, vou explicar como o **MediatR** está configurado e como você pode implementá-lo em outro projeto.

---

## **1. PACOTES NUGET NECESSÁRIOS**

### **Projeto Domain (onde ficam Commands e Handlers)**
```xml
<PackageReference Include="Herval.Mediator" Version="1.0.32" />
<PackageReference Include="FluentValidation" Version="11.x.x" />
```
*Observação: O pacote "Herval.Mediator" é um wrapper customizado. Para implementação padrão, use:*
```xml
<PackageReference Include="MediatR" Version="11.1.0" />
```

### **Projeto API**
```xml
<PackageReference Include="MediatR" Version="11.1.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

### **Projeto IoC (Injeção de Dependência)**
```xml
<PackageReference Include="MediatR" Version="11.1.0" />
<PackageReference Include="FluentValidation" Version="11.x.x" />
```

---

## **2. ESTRUTURA DO PROJETO**

```
src/
├── Ecommerce.Servicos.Produtos.Api/          # API
│   ├── Controllers/                           # Controllers que usam MediatR
│   └── Startup.cs                            # Configuração da aplicação
│
├── Ecommerce.Servicos.Produtos.Domain/       # Camada de Domínio
│   └── Commands/                             # Commands e Handlers
│       └── SalvarPrecoLoja/                  # Exemplo de feature
│           ├── SalvarPrecoLojaCommand.cs           # Command (Request)
│           ├── SalvarPrecoLojaCommandHandler.cs   # Handler (Processamento)
│           └── SalvarPrecoLojaCommandValidator.cs # Validação
│
└── Ecommerce.Servicos.Produtos.CrossCutting.Ioc/  # Injeção de Dependência
    ├── MediatorInjection.cs                  # Configuração do MediatR
    └── DependencyInjection.cs                # Outras injeções
```

---

## **3. CONFIGURAÇÃO DO MEDIATR**

### **3.1 - Arquivo: MediatorInjection.cs**
```csharp
using FluentValidation;
using Herval.Mediator.PipelineBehavior;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Ecommerce.Servicos.Produtos.Infra.CrossCutting.Ioc
{
    public static class MediatorInjection
    {
        // Assembly onde estão os Commands e Handlers
        private static Assembly DomainAssembly => 
            AppDomain.CurrentDomain.Load("Ecommerce.Servicos.Produtos.Domain");

        public static void AddMediator(this IServiceCollection services)
        {
            // 1. Registra o MediatR procurando handlers no assembly Domain
            services.AddMediatR(DomainAssembly);

            // 2. Registra todos os validators do FluentValidation
            AssemblyScanner
                .FindValidatorsInAssembly(DomainAssembly)
                .ForEach(result => services.AddScoped(result.InterfaceType, result.ValidatorType));

            // 3. Adiciona o Pipeline Behavior de Validação (executa antes dos handlers)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }
    }
}
```

### **3.2 - Arquivo: Startup.cs (na API)**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers(options =>
    {
        options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
        options.Filters.Add(typeof(GlobalExceptionFilter));
        options.Filters.Add(typeof(CustomNotificationFilter));
        options.Filters.Add(typeof(CustomNotFoundFilter));
    })
    .AddNewtonsoftJson();

    // ... outras configurações

    // IMPORTANTE: Registra o MediatR
    services.AddMediator();
    
    // ... outras configurações
}
```

---

## **4. CRIANDO UM COMMAND (REQUEST)**

### **Arquivo: SalvarPrecoLojaCommand.cs**
```csharp
using System.Collections.Generic;
using Herval.Notifications.Entities;
using MediatR;

namespace Ecommerce.Servicos.Produtos.Domain.Commands.SalvarPrecoLoja
{
    // Command implementa IRequest<TResponse>
    // TResponse é o tipo de retorno que o Handler irá retornar
    public class SalvarPrecoLojaCommand : IRequest<IList<Notification>>
    {
        public IEnumerable<SalvarPrecoLojaItemCommand> PrecoLojas { get; set; }
    }

    public class SalvarPrecoLojaItemCommand
    {
        public string CodigoProdutoSap { get; set; }
        internal string CodigoProdutoSapLimpo => CodigoProdutoSap.TrimStart('0');
        public string Centro { get; set; }
        public decimal PrecoLista { get; set; }
        public decimal? PrecoVenda { get; set; }
        // ... outras propriedades
    }
}
```

**Tipos comuns de IRequest:**
- `IRequest<TResponse>` - Retorna um resultado
- `IRequest` - Não retorna resultado (void)

---

## **5. CRIANDO UM HANDLER**

### **Arquivo: SalvarPrecoLojaCommandHandler.cs**
```csharp
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Herval.Notifications.Entities;

namespace Ecommerce.Servicos.Produtos.Domain.Commands.SalvarPrecoLoja
{
    // Handler implementa IRequestHandler<TRequest, TResponse>
    public class SalvarPrecoLojaCommandHandler : IRequestHandler<SalvarPrecoLojaCommand, IList<Notification>>
    {
        // Injeção de dependências via construtor
        private readonly ISiteApiService _siteApiService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IPrecoLojaRepository _precoLojaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SalvarPrecoLojaCommandHandler(
            ISiteApiService siteApiService,
            IProdutoRepository produtoRepository,
            IPrecoLojaRepository precoLojaRepository,
            IUnitOfWork unitOfWork)
        {
            _siteApiService = siteApiService;
            _produtoRepository = produtoRepository;
            _precoLojaRepository = precoLojaRepository;
            _unitOfWork = unitOfWork;
        }

        // Método Handle que processa o comando
        public async Task<IList<Notification>> Handle(
            SalvarPrecoLojaCommand request, 
            CancellationToken cancellationToken)
        {
            var notificacoes = new List<Notification>();

            // Lógica de negócio aqui
            foreach (var precoLoja in request.PrecoLojas)
            {
                // Processar cada item...
            }

            // Salvar mudanças
            await _unitOfWork.CommitAsync();

            return notificacoes;
        }
    }
}
```

---

## **6. CRIANDO UM VALIDATOR (OPCIONAL MAS RECOMENDADO)**

### **Arquivo: SalvarPrecoLojaCommandValidator.cs**
```csharp
using FluentValidation;

namespace Ecommerce.Servicos.Produtos.Domain.Commands.SalvarPrecoLoja
{
    // Validator herda de AbstractValidator<TCommand>
    public class SalvarPrecoLojaCommandValidator : AbstractValidator<SalvarPrecoLojaCommand>
    {
        public SalvarPrecoLojaCommandValidator()
        {
            // Valida cada item da coleção
            RuleForEach(x => x.PrecoLojas)
                .SetValidator(new SalvarPrecoLojaItemCommandValidator());
        }
    }
    
    public class SalvarPrecoLojaItemCommandValidator : AbstractValidator<SalvarPrecoLojaItemCommand> 
    {
        public SalvarPrecoLojaItemCommandValidator() 
        {
            RuleFor(x => x.CodigoProdutoSap)
                .NotEmpty()
                .WithMessage("O campo '{PropertyName}' é obrigatório.");
            
            RuleFor(x => x.Centro)
                .NotEmpty()
                .WithMessage("O campo '{PropertyName}' é obrigatório.");

            RuleFor(x => x.DataInicioPrecoVenda)
                .Must((x, dataInicio) => dataInicio <= x.DataFimPrecoVenda)
                .When(x => x.DataFimPrecoVenda.HasValue && x.DataInicioPrecoVenda.HasValue)
                .WithMessage("A '{PropertyName}' não pode ser maior que a data de fim.");
        }
    }
}
```

---

## **7. USANDO O MEDIATR NO CONTROLLER**

### **Arquivo: BundlesController.cs (Exemplo)**
```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce.Servicos.Produtos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BundlesController : ControllerBase
    {
        // Injetar o IMediator
        private readonly IMediator _mediator;

        public BundlesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Exemplo de endpoint que usa o MediatR
        /// </summary>
        [HttpPost, Route("/api/bundles/sap")]
        public async Task<IActionResult> CriarComboBundleAsync([FromBody] CriarComboBundleCommand command)
        {
            // Envia o comando para o MediatR processar
            // O MediatR irá:
            // 1. Executar o Validator (se existir)
            // 2. Executar o Handler correspondente
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Outro exemplo
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterBundleAsync([FromRoute] string id)
        {
            // Cria o command inline e envia
            var result = await _mediator.Send(new ObterBundleCommand(id));

            return Ok(result);
        }
    }
}
```

---

## **8. FLUXO DE EXECUÇÃO**

```
1. Controller recebe a requisição HTTP
          ↓
2. Controller cria/recebe um Command
          ↓
3. Controller chama _mediator.Send(command)
          ↓
4. MediatR executa os Pipeline Behaviors (ex: ValidationBehavior)
          ↓
5. MediatR encontra e executa o Handler correspondente
          ↓
6. Handler processa a lógica de negócio
          ↓
7. Handler retorna o resultado
          ↓
8. MediatR retorna para o Controller
          ↓
9. Controller retorna a resposta HTTP
```

---

## **9. PADRÃO DE NOMENCLATURA**

### **Commands:**
- `{Acao}{Entidade}Command.cs`
- Exemplos: `SalvarPrecoLojaCommand`, `ObterBundleCommand`, `CriarProdutoCommand`

### **Handlers:**
- `{Acao}{Entidade}CommandHandler.cs`
- Exemplos: `SalvarPrecoLojaCommandHandler`, `ObterBundleCommandHandler`

### **Validators:**
- `{Acao}{Entidade}CommandValidator.cs`
- Exemplos: `SalvarPrecoLojaCommandValidator`, `ObterBundleCommandValidator`

### **Estrutura de Pastas:**
```
Commands/
└── SalvarPrecoLoja/
    ├── SalvarPrecoLojaCommand.cs
    ├── SalvarPrecoLojaCommandHandler.cs
    └── SalvarPrecoLojaCommandValidator.cs
```

---

## **10. BENEFÍCIOS DO MEDIATR**

✅ **Desacoplamento** - Controllers não conhecem os handlers diretamente  
✅ **Single Responsibility** - Cada handler tem uma única responsabilidade  
✅ **Testabilidade** - Fácil mockar e testar handlers isoladamente  
✅ **Pipeline Behaviors** - Adiciona comportamentos transversais (logging, validação, etc)  
✅ **Clean Code** - Controllers ficam mais limpos e focados em HTTP  

---

## **11. EXEMPLO COMPLETO DE HERANÇA**

O projeto também usa herança de Commands:

```csharp
// Command Base
public class SalvarPrecoLojaCommand : IRequest<IList<Notification>>
{
    public IEnumerable<SalvarPrecoLojaItemCommand> PrecoLojas { get; set; }
}

// Command específico que herda do base
public class SalvarPrecoLojaSapCommand : SalvarPrecoLojaCommand
{ 
    // Pode adicionar propriedades específicas ou apenas herdar
}

// Handler específico
public class SalvarPrecoLojaSapCommandHandler : IRequestHandler<SalvarPrecoLojaSapCommand, IList<Notification>>
{
    // Implementação específica para SAP
}
```

---

Essa documentação cobre toda a configuração e uso do MediatR neste projeto! 

Posso gerar um documento Markdown completo ou criar exemplos práticos adicionais?

silvio-sebastiany: markdown

GitHub Copilot: Vou criar um arquivo Markdown completo com toda a documentação sobre o MediatR neste projeto! 