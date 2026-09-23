using Biblioteca.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Infraestrutura;

/// <summary>
/// Aplica as migrations pendentes na subida da aplicação.
/// <para>
/// Conveniência de projeto de estudo: sobe o compose e a API já encontra o schema pronto.
/// Em produção isto normalmente sai daqui e vira um passo próprio do pipeline, para que
/// uma migration demorada ou malsucedida não derrube a aplicação inteira — por isso o
/// comportamento é controlado por configuração, e não fixado no código.
/// </para>
/// </summary>
internal static class MigracaoDeBanco
{
    public static async Task AplicarMigrationsPendentesAsync(this WebApplication app)
    {
        await using var escopo = app.Services.CreateAsyncScope();

        var contexto = escopo.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
        var logger = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(MigracaoDeBanco));

        var pendentes = (await contexto.Database.GetPendingMigrationsAsync()).ToArray();
        if (pendentes.Length == 0)
        {
            logger.LogInformation("Banco já está atualizado: nenhuma migration pendente.");
            return;
        }

        logger.LogInformation("Aplicando {Quantidade} migration(s) pendente(s).", pendentes.Length);
        await contexto.Database.MigrateAsync();
        logger.LogInformation("Migrations aplicadas.");
    }
}
