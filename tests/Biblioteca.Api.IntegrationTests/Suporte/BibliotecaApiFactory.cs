using Biblioteca.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Biblioteca.Api.IntegrationTests.Suporte;

/// <summary>
/// Sobe a API real contra um PostgreSQL real em container.
/// <para>
/// Nada é substituído por dublê: o teste exercita controller, caso de uso, serviço de
/// domínio, EF Core e as constraints do banco. É o que um provider in-memory não
/// conseguiria cobrir — ele não valida SQL, tipo <c>citext</c> nem chave estrangeira.
/// </para>
/// </summary>
public sealed class BibliotecaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("biblioteca")
        .WithUsername("biblioteca")
        .WithPassword("biblioteca")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Biblioteca", _postgres.GetConnectionString());

        // A fixture aplica as migrations explicitamente em InitializeAsync, para não
        // depender de em que ponto o host de teste interrompe a execução do Program.
        builder.UseSetting("Banco:AplicarMigrationsNoStartup", "false");
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var escopo = Services.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<BibliotecaDbContext>();

        // As mesmas migrations que rodarão em produção — inclusive a criação da
        // extensão citext, de que depende a unicidade de gênero.
        await contexto.Database.MigrateAsync();
    }

    /// <summary>
    /// Connection string para outro banco no mesmo servidor do container.
    /// <para>
    /// O banco não é criado aqui — é justamente o que permite testar a criação feita
    /// pela própria aplicação ao aplicar as migrations.
    /// </para>
    /// </summary>
    public string ConexaoParaOutroBanco(string nomeDoBanco) =>
        new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString()) { Database = nomeDoBanco }
            .ConnectionString;

    /// <summary>
    /// Devolve o banco ao estado vazio entre testes, para que a ordem de execução
    /// não influencie o resultado.
    /// </summary>
    public async Task LimparBancoAsync()
    {
        await using var escopo = Services.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<BibliotecaDbContext>();

        await contexto.Database.ExecuteSqlRawAsync("TRUNCATE livros, autores, generos CASCADE;");
    }

    // Implementação explícita: WebApplicationFactory já expõe um DisposeAsync próprio,
    // e os dois não podem ocupar a mesma assinatura.
    Task IAsyncLifetime.DisposeAsync() => _postgres.DisposeAsync().AsTask();
}
