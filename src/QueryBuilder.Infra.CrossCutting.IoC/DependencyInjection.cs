using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using QueryBuilder.Domain.Interfaces;
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

            // Domain Services
            services.AddScoped<IQueryBuilderService, QueryBuilderService>();

            // SqlKata Compiler (Singleton pois é stateless)
            services.AddSingleton<OracleCompiler>();

            return services;
        }
    }
}
