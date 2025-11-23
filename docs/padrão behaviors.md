 README.md
Herval.Mediator
Implementação personalizada do padrão Mediator para .NET, inspirada na biblioteca MediatR, com suporte a validações, pipeline behaviors e tratamento de comandos com e sem retorno.

Instalação
# Via Package Manager
Install-Package Herval.Mediator

# Via .NET CLI
dotnet add package Herval.Mediator
Recursos Principais
Requests com Retorno: IRequest<TResult> para operações que retornam valores
Requests sem Retorno (void): IRequest<Unit> para operações void
Pipeline Behaviors: Suporte para validação e logging antes/depois da execução do handler e pipelines customizadas
Validação Integrada: Integração com FluentValidation
Notificações: Padrão de notificação para comunicação entre componentes
Dependências
Herval.Notifications
FluentValidation
Microsoft.Extensions.DependencyInjection
Guia de Uso Rápido
1. Configuração
Registre o Herval.Mediator nos serviços:

// No Startup.cs ou Program.cs
services.AddHervalMediator(Assembly.GetExecutingAssembly());
services.AddValidationBehavior(); // Opcional - para validação automática
services.AddLoggingBehavior();    // Opcional - para logging automático
services.AddScoped<INotificationContext, NotificationContext>();
2. Requests com Retorno
// 1. Request
public class PingRequest : IRequest<string>
{
    public string Message { get; set; }
}

// 2. Handler
public class PingRequestHandler : IRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Pong: {request.Message}");
    }
}

// 3. Uso
public class MyController
{
    private readonly IMediator _mediator;
    
    public MyController(IMediator mediator)
    {
        _mediator = mediator;
    }
      public async Task<IActionResult> Ping(string message)
    {
        var result = await _mediator.Send(new PingRequest { Message = message });
        return Ok(result); // Retorna "Pong: {message}"
    }
}
3. Requests sem Retorno (void/Unit)
// 1. Request void (sem retorno)
public class PingVoidRequest : IRequest<Unit>
{
    public string Message { get; set; }
}

// 2. Handler
public class PingVoidRequestHandler : IRequestHandler<PingVoidRequest, Unit>
{    
    private readonly ILogger<PingVoidRequestHandler> _logger;
    
    public PingVoidRequestHandler(ILogger<PingVoidRequestHandler> logger)
    {
        _logger = logger;
    }
    
    public Task<Unit> Handle(PingVoidRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request recebido: {Message}", request.Message);
        
        // Processamento aqui...
        
        // Retorne Unit.Value para indicar conclusão sem resultado
        return Task.FromResult(Unit.Value);
    }
}

// 3. Uso
public class MyApiController
{
    private readonly IMediator _mediator;
    
    public MyApiController(IMediator mediator)
    {
        _mediator = mediator;
    }
      public async Task<IActionResult> ProcessRequest(string message)
    {
        // Envia request e ignora o retorno (Unit.Value)
        await _mediator.Send(new PingVoidRequest { Message = message });
        return Ok("Processado com sucesso");
    }
}
Pipeline Behaviors
O Herval.Mediator suporta Pipeline Behaviors, permitindo adicionar comportamentos que são executados antes e depois dos handlers. Eles formam uma cadeia de execução, onde cada behavior pode interagir com o request e decidir se continua ou interrompe o pipeline.

Como Funciona o Pipeline
O pipeline é construído em ordem reversa, iniciando pelo handler e envolvendo cada behavior em torno da chamada anterior:

Behavior3 -> Behavior2 -> Behavior1 -> Handler -> Resultado
Cada behavior pode:

Executar código antes de passar para o próximo elemento na cadeia
Decidir se continua o pipeline ou retorna
Executar código após o retorno do próximo elemento
Modificar o resultado recebido do elemento seguinte
Behaviors Disponíveis
ValidationBehavior: Realiza validações usando FluentValidation (requer INotificationContext)
LoggingBehavior: Registra logs de entrada, saída e tempo de execução dos comandos
ValidationBehavior em Detalhe
O ValidationBehavior é uma implementação especial que integra o FluentValidation com o pipeline de request:

public class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{          
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly INotificationContext _notificationContext;
    
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        INotificationContext notificationContext)
    {
        _validators = validators;
        _notificationContext = notificationContext;
    }
    
    public async Task<TResult> Handle(TRequest request, HandleDelegate<TResult> next, CancellationToken cancellationToken)
    {        
        // Se não houver validadores, continua o pipeline
        if (!_validators.Any())
            return await next();
            
        // Executa todos os validadores e coleta falhas
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
        
        // Se houver falhas, adiciona ao contexto de notificação e retorna default
        if (failures.Any())
        {
            foreach (var failure in failures)
                _notificationContext.AddNotification(failure.PropertyName, failure.ErrorMessage);
            return default;
        }
        
        // Se a validação passar, continua para o próximo behavior ou handler
        return await next();
    }
}

### Como Registrar Behaviors

Use os métodos de extensão incluídos para os behaviors padrão:

```csharp
// No Startup.cs ou Program.cs
services.AddHervalMediator(Assembly.GetExecutingAssembly());

// Registrar ValidationBehavior
services.AddValidationBehavior();

// Registrar LoggingBehavior
services.AddLoggingBehavior();

// Para behaviors personalizados, use o método AddScoped diretamente:
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MeuBehaviorPersonalizado<,>));
Como Criar um Behavior Personalizado
public class TimingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly ILogger<TimingBehavior<TRequest, TResult>> _logger;

    public TimingBehavior(ILogger<TimingBehavior<TRequest, TResult>> logger)
    {
        _logger = logger;
    }    public async Task<TResult> Handle(TRequest request, HandleDelegate<TResult> next, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var result = await next();
        sw.Stop();
        
        _logger.LogInformation(
            "Request {RequestName} executado em {ElapsedMilliseconds}ms",
            typeof(TRequest).Name,
            sw.ElapsedMilliseconds);
            
        return result;
    }
}

// Registre o behavior diretamente no container de DI
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));
Arquitetura Interna e Desempenho
O Herval.Mediator foi projetado usando um sistema de wrappers que preserva informação de tipo durante todo o ciclo de execução, garantindo que validações, behaviors, e handlers sejam sempre executados com tipos concretos, não com interfaces genéricas.

TypeCache
Para melhorar o desempenho, o Herval.Mediator usa um sistema de cache de tipos para evitar reflexão excessiva:

// Exemplo: O mediator usa cache de tipos para evitar reflexão repetitiva
public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
{
    // Obter o tipo concreto do request e usar cache de tipos
    var requestType = request.GetType();
    var wrapperType = TypeCache.GetOrCreateCommandWrapperType<TResult>(requestType);
    
    // O restante do processo usa o tipo em cache...
}
Sistema de Wrapper de Tipos
O sistema de wrappers consiste em:

CommandHandlerWrapperBase: Classe base abstrata para todos os wrappers
CommandHandlerWrapper<TRequest, TResult>: Implementação concreta que preserva a tipagem
ConcreteCommandHandlerWrapper<TRequest, TResult, THandler>: Adapta handlers concretos para interfaces genéricas
Esses componentes trabalham juntos para garantir que o sistema opere de forma tipada, preservando a integridade do tipo desde o registro até a execução.

Usando Eventos (Padrão Event-Driven Architecture)
Além dos comandos, o Herval.Mediator também oferece suporte a eventos, implementando o padrão Observer para comunicação entre componentes desacoplados. Eventos são uma forma mais expressiva de transmitir ocorrências significativas dentro do sistema.

Criando um Evento
// 1. Defina uma classe de evento que implementa INotification
public class UserCreatedEvent : INotification
{
    public string Username { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// 2. Crie um ou mais handlers para o evento
public class EmailEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    
    public EmailEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task Handle(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Enviar email de boas-vindas quando o evento é recebido
        await _emailService.SendWelcomeEmailAsync(@event.Username);
    }
}

// 3. Publique o evento usando o mediator
await _mediator.Publish(new UserCreatedEvent { Username = "novoUsuario" });
Benefícios do Uso de Eventos
Padrão Event-Driven Architecture: Alinhamento com padrões modernos de arquitetura
Desacoplamento: Os publishers não precisam conhecer os subscribers
Extensibilidade: Adicione novos comportamentos sem modificar código existente
Múltiplos Handlers: Um único evento pode ser processado por vários handlers
Execução Paralela: Handlers de evento são executados de forma independente
Entendendo o Unit
Unit é uma estrutura especial que representa a "ausência de um valor" ou um equivalente tipado para void. É usado como tipo de retorno para comandos que não precisam retornar valores reais.

O que é Unit?
public readonly struct Unit
{
    // Valor padrão estático para uso em retornos de métodos void
    public static readonly Unit Value = default;
    
    // Representação de string para debugging e visualização
    public override string ToString() => "()";
}
Melhores Práticas
Aqui estão algumas recomendações para obter o melhor do Herval.Mediator:

Estrutura de Requests e Handlers
Requests Imutáveis: Defina todos os requests como imutáveis sempre que possível
Handlers Específicos: Mantenha os handlers focados em uma única responsabilidade
Validações Claras: Crie validadores com mensagens claras e descritivas
Behaviors
Ordem de Registro: Registre behaviors na ordem que deseja que sejam executados
Independência: Cada behavior deve ser independente e não assumir execução de outros
Responsabilidade Única: Cada behavior deve ter um propósito bem definido
Desempenho
Cache: O TypeCache já otimiza reflexão, mas esteja atento ao uso excessivo de behaviors
Behaviors Condicionais: Considere adicionar verificações rápidas para evitar operações custosas
Validação Eficiente: Use validadores simples e diretos para evitar sobrecarga
Solução de problemas
Problemas Comuns:
Validações não estão sendo executadas:

Verifique se AddValidationBehavior() está sendo chamado
Confirme que os validadores estão sendo registrados corretamente
Verifique se os validadores estão na mesma assembly ou se todas as assemblies relevantes estão sendo escaneadas
Handlers não estão sendo encontrados:

Verifique se AddHervalMediator() está sendo chamado com as assemblies corretas
Confirme que os handlers implementam a interface correta
Verifique possíveis erros de registro no DI
Behaviors não estão sendo executados na ordem correta:

Lembre-se que behaviors são executados na ordem reversa ao registro
Diagnóstico:
Para diagnosticar problemas, adicione temporariamente este behavior de diagnóstico:

public class DiagnosticBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly ILogger<DiagnosticBehavior<TRequest, TResult>> _logger;
    
    public DiagnosticBehavior(ILogger<DiagnosticBehavior<TRequest, TResult>> logger)
    {
        _logger = logger;
    }
    
    public async Task<TResult> Handle(TRequest request, HandleDelegate<TResult> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Executando request {RequestType} com behaviors registrados para tipo concreto",
            typeof(TRequest).FullName);
            
        var result = await next();
        
        _logger.LogInformation(
            "Request {RequestType} executado com sucesso, retornando {ResultType}",
            typeof(TRequest).FullName,
            result?.GetType()?.FullName ?? "null");
            
        return result;
    }
}