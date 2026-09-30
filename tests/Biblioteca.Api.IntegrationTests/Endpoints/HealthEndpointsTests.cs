using System.Net;
using Biblioteca.Api.IntegrationTests.Suporte;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Biblioteca.Api.IntegrationTests.Endpoints;

/// <summary>
/// As duas rotas de saúde, com um banco que a API não alcança.
/// <para>
/// Aponta para um banco que não existe no container: é o jeito mais barato de a API
/// subir sem conseguir falar com o PostgreSQL, que é justamente o que distingue uma
/// rota da outra.
/// </para>
/// </summary>
[Collection(ColecaoDeIntegracao.Nome)]
public sealed class HealthEndpointsTests(BibliotecaApiFactory fabrica)
{
    [Fact]
    public async Task HealthLive_ComBancoInacessivel_DeveRetornar200()
    {
        await using var api = ApiSemBanco();

        var resposta = await api.CreateClient().GetAsync("/health/live");

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_ComBancoInacessivel_DeveRetornar503()
    {
        await using var api = ApiSemBanco();

        var resposta = await api.CreateClient().GetAsync("/health");

        resposta.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    private WebApplicationFactory<Program> ApiSemBanco() =>
        fabrica.WithWebHostBuilder(builder => builder.UseSetting(
            "ConnectionStrings:Biblioteca",
            fabrica.ConexaoParaOutroBanco($"inexistente_{Guid.NewGuid():N}")));
}
