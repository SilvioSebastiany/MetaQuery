using System.Data;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using QueryBuilder.Domain.Behaviors;
using QueryBuilder.Domain.Interfaces;
using QueryBuilder.Domain.Notifications;
using QueryBuilder.Domain.Services;
using QueryBuilder.Infra.CrossCutting.Settings;
using QueryBuilder.Infra.Data.Repositories;
using SqlKata.Compilers;

namespace QueryBuilder.Infra.CrossCutting.IoC
{
    /// <summary>
    /// Configuração de Dependency Injection
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Settings
            var databaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
                ?? throw new InvalidOperationException("DatabaseSettings não configurado");

            services.AddSingleton(databaseSettings);

            // Database Connection
            services.AddScoped<IDbConnection>(provider =>
            {
                var connection = new OracleConnection(databaseSettings.ConnectionString);
                connection.Open();
                return connection;
            });

            // Repositories
            services.AddScoped<IMetadadosRepository, MetadadosRepository>();
            services.AddScoped<IConsultaDinamicaRepository, ConsultaDinamicaRepository>();
            services.AddScoped<IUnitOfWork, QueryBuilder.Infra.Data.UoW.UnitOfWork>();

            // Services (auxiliares)
            services.AddScoped<IQueryBuilderService, QueryBuilderService>();

            // DomainServices (lógica de negócio)
            services.AddScoped<QueryBuilder.Domain.DomainServices.ConsultaDinamicaDomainService>();
            services.AddScoped<QueryBuilder.Domain.DomainServices.MetadadosDomainService>();

            // SqlKata Compiler (Singleton pois é stateless)
            services.AddSingleton<OracleCompiler>();

            // Notification Context (Scoped - por request)
            services.AddScoped<INotificationContext, NotificationContext>();

            // MediatR com Assembly Scanning
            var domainAssembly = Assembly.Load("QueryBuilder.Domain");
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(domainAssembly);

                // Registrar behaviors na ordem: Logging → Validation → Handler
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // FluentValidation - Assembly Scanning automático
            services.AddValidatorsFromAssembly(domainAssembly);

            return services;
        }
    }
}
