using Biblioteca.Domain.Services.Contratos;
using Microsoft.Extensions.DependencyInjection;

namespace Biblioteca.Domain.Services;

/// <summary>
/// Cada camada registra os próprios serviços. O <c>Program.cs</c> compõe as camadas
/// sem precisar conhecer as classes concretas de nenhuma delas.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IAutorDomainService, AutorDomainService>();
        services.AddScoped<IGeneroDomainService, GeneroDomainService>();
        services.AddScoped<ILivroDomainService, LivroDomainService>();

        return services;
    }
}
