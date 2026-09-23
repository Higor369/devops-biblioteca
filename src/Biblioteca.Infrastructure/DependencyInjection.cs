using Biblioteca.Domain.Repositories;
using Biblioteca.Infrastructure.Persistencia;
using Biblioteca.Infrastructure.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biblioteca.Infrastructure;

/// <inheritdoc cref="Biblioteca.Domain.Services.DependencyInjection"/>
public static class DependencyInjection
{
    /// <param name="popularComDadosDeExemplo">
    /// Grava o acervo de <see cref="DadosDeExemplo"/> quando as migrations criam um banco
    /// vazio. Serve para desenvolvimento; em produção fica desligado.
    /// </param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        bool popularComDadosDeExemplo = false)
    {
        // TryAdd: a camada de aplicação registra o mesmo relógio. Qualquer uma das duas
        // pode ser composta primeiro sem que a ordem mude o resultado.
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<InterceptadorDeAuditoria>();

        services.AddDbContext<BibliotecaDbContext>((provedor, options) =>
        {
            options
                .UseNpgsql(connectionString)
                .AddInterceptors(provedor.GetRequiredService<InterceptadorDeAuditoria>());

            // Só a versão assíncrona: a solução aplica as migrations sempre com MigrateAsync.
            if (popularComDadosDeExemplo)
            {
                var relogio = provedor.GetRequiredService<TimeProvider>();
                options.UseAsyncSeeding((contexto, _, cancellationToken) =>
                    DadosDeExemplo.PopularAsync(contexto, relogio, cancellationToken));
            }
        });

        services.AddScoped<IAutorRepository, AutorRepository>();
        services.AddScoped<IGeneroRepository, GeneroRepository>();
        services.AddScoped<ILivroRepository, LivroRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
