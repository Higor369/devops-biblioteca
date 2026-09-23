using System.Net.Http.Json;
using Biblioteca.Api.IntegrationTests.Suporte;
using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Dtos.Generos;
using Biblioteca.Application.Dtos.Livros;
using Biblioteca.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Biblioteca.Api.IntegrationTests.Persistencia;

/// <summary>
/// O que acontece quando a API sobe contra um servidor PostgreSQL que ainda não tem o banco.
/// <para>
/// Cada teste aponta para um banco de nome novo no mesmo container das demais classes —
/// um banco que, portanto, ainda não existe. Por isso esta classe não herda de
/// <see cref="TesteDeIntegracao"/>: ela não usa o banco compartilhado.
/// </para>
/// </summary>
[Collection(ColecaoDeIntegracao.Nome)]
public sealed class CriacaoDoBancoTests(BibliotecaApiFactory fabrica)
{
    [Fact]
    public async Task Migrate_ComBancoInexistente_DeveCriarOBancoVazio()
    {
        await using var api = ApiComBancoNovo(popularComDadosDeExemplo: false);
        await using var escopo = api.Services.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<BibliotecaDbContext>();

        (await contexto.Database.CanConnectAsync()).ShouldBeFalse();

        await contexto.Database.MigrateAsync();

        (await contexto.Database.CanConnectAsync()).ShouldBeTrue();
        (await contexto.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        (await contexto.Livros.AnyAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task Migrate_ComDadosDeExemploLigados_DevePopularOAcervo()
    {
        await using var api = ApiComBancoNovo(popularComDadosDeExemplo: true);
        await MigrarAsync(api);

        var cliente = api.CreateClient();
        var autores = await cliente.GetFromJsonAsync<List<AutorResponse>>("/api/autores");
        var generos = await cliente.GetFromJsonAsync<List<GeneroResponse>>("/api/generos");
        var livros = await cliente.GetFromJsonAsync<List<LivroResponse>>("/api/livros");

        autores.ShouldNotBeEmpty();
        generos.ShouldNotBeEmpty();
        livros.ShouldNotBeEmpty();
        livros.ShouldContain(livro =>
            livro.Titulo == "Dom Casmurro"
            && livro.Autor.Nome == "Machado de Assis"
            && livro.Genero.Nome == "Romance");
    }

    /// <summary>
    /// O EF Core chama o seeding em todo <c>MigrateAsync</c>, mesmo sem migration pendente.
    /// </summary>
    [Fact]
    public async Task Migrate_ExecutadoDeNovo_NaoDeveDuplicarOsDadosDeExemplo()
    {
        await using var api = ApiComBancoNovo(popularComDadosDeExemplo: true);
        var cliente = api.CreateClient();

        await MigrarAsync(api);
        var livrosNaCriacao = await cliente.GetFromJsonAsync<List<LivroResponse>>("/api/livros");

        await MigrarAsync(api);
        var livrosDepois = await cliente.GetFromJsonAsync<List<LivroResponse>>("/api/livros");

        livrosDepois!.Count.ShouldBe(livrosNaCriacao!.Count);
    }

    private WebApplicationFactory<Program> ApiComBancoNovo(bool popularComDadosDeExemplo) =>
        fabrica.WithWebHostBuilder(builder => builder
            .UseSetting(
                "ConnectionStrings:Biblioteca",
                fabrica.ConexaoParaOutroBanco($"biblioteca_{Guid.NewGuid():N}"))
            .UseSetting("Banco:PopularComDadosDeExemplo", popularComDadosDeExemplo.ToString()));

    private static async Task MigrarAsync(WebApplicationFactory<Program> api)
    {
        await using var escopo = api.Services.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<BibliotecaDbContext>();

        await contexto.Database.MigrateAsync();
    }
}
