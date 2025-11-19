using FluentValidation;
using MediatR;
using QueryBuilder.Domain.Notifications;

namespace QueryBuilder.Domain.Behaviors;

/// <summary>
/// Behavior do MediatR que executa validações automaticamente antes do Handler
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
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

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Se não tem validators, segue fluxo
        if (!_validators.Any())
        {
            return await next();
        }

        // Executar todas as validações
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        // Se tem erros, adiciona nas notificações e retorna default
        if (failures.Any())
        {
            foreach (var failure in failures)
            {
                _notificationContext.AddNotification(
                    failure.PropertyName,
                    failure.ErrorMessage);
            }

            return default!; // Retorna null/default se validação falhar
        }

        // Validação passou, prossegue para o handler
        return await next();
    }
}
