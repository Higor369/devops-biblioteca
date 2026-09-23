using Biblioteca.Application.Contratos;
using Biblioteca.Application.Servicos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biblioteca.Application;

/// <inheritdoc cref="Biblioteca.Domain.Services.DependencyInjection"/>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAutorAppService, AutorAppService>();
        services.AddScoped<IGeneroAppService, GeneroAppService>();
        services.AddScoped<ILivroAppService, LivroAppService>();

        // Relógio real da aplicação. Nos testes ele é trocado por um relógio fixo,
        // e é isso que torna a validação de ano determinística.
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
