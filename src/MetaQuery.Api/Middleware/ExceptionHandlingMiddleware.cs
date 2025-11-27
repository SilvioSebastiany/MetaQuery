using System.Net;
using System.Text.Json;

namespace MetaQuery.Api.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções
/// Constitution 2.7: Controllers não devem conter try-catch customizado
/// Todas exceções são tratadas centralmente aqui
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            // Erros de validação/negócio retornam 400
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Errors = new List<ErrorDetail>
                    {
                        new() { Key = "Validation", Message = argEx.Message }
                    }
                }
            ),

            // Entidade não encontrada retorna 404
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Errors = new List<ErrorDetail>
                    {
                        new() { Key = "NotFound", Message = "Recurso não encontrado" }
                    }
                }
            ),

            // Operação cancelada pelo cliente retorna 499
            OperationCanceledException => (
                (HttpStatusCode)499, // Client Closed Request
                new ErrorResponse
                {
                    Errors = new List<ErrorDetail>
                    {
                        new() { Key = "Cancelled", Message = "Operação cancelada pelo cliente" }
                    }
                }
            ),

            // Qualquer outra exceção retorna 500 sem expor detalhes
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Errors = new List<ErrorDetail>
                    {
                        new() { Key = "Internal", Message = "Erro interno do servidor" }
                    }
                }
            )
        };

        // Log do erro (com stack trace apenas para erros internos)
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erro interno não tratado: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Erro tratado: {StatusCode} - {Message}", (int)statusCode, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

/// <summary>
/// Resposta padronizada de erro
/// </summary>
public class ErrorResponse
{
    public List<ErrorDetail> Errors { get; set; } = new();
}

/// <summary>
/// Detalhe de um erro individual
/// </summary>
public class ErrorDetail
{
    public string Key { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
