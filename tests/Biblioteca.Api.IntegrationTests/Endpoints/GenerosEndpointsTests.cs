using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.IntegrationTests.Suporte;
using Biblioteca.Application.Dtos.Generos;
using Biblioteca.Application.Dtos.Livros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.IntegrationTests.Endpoints;

public sealed class GenerosEndpointsTests(BibliotecaApiFactory fabrica) : TesteDeIntegracao(fabrica)
{
    [Fact]
    public async Task Post_ComNomeValido_DeveRetornar201()
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/generos", new CriarGeneroRequest("Ficção Científica"));

        resposta.StatusCode.ShouldBe(HttpStatusCode.Created);

        var genero = await resposta.Content.ReadFromJsonAsync<GeneroResponse>();
        genero!.Nome.ShouldBe("Ficção Científica");
    }

    [Fact]
    public async Task Post_ComNomeJaCadastrado_DeveRetornar409()
    {
        await CadastrarGeneroAsync("Romance");

        var resposta = await Cliente.PostAsJsonAsync("/api/generos", new CriarGeneroRequest("Romance"));

        resposta.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("genero.nome_ja_cadastrado");
    }

    /// <summary>
    /// Depende da coluna <c>citext</c>: com uma coluna de texto comum este teste falharia,
    /// porque "romance" e "ROMANCE" seriam valores distintos para o banco.
    /// </summary>
    [Theory]
    [InlineData("romance")]
    [InlineData("ROMANCE")]
    [InlineData("RoMaNcE")]
    public async Task Post_ComNomeQueSoDifereNoCaixa_DeveRetornar409(string variacao)
    {
        await CadastrarGeneroAsync("Romance");

        var resposta = await Cliente.PostAsJsonAsync("/api/generos", new CriarGeneroRequest(variacao));

        resposta.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_ComOMesmoNomeDoProprioGenero_DeveSerAceito()
    {
        var genero = await CadastrarGeneroAsync("Romance");

        var resposta = await Cliente.PutAsJsonAsync(
            $"/api/generos/{genero.Id}",
            new AtualizarGeneroRequest("Romance"));

        // Renomear um gênero para o nome que ele já tem não é duplicidade.
        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ComLivrosVinculados_DeveRetornar409()
    {
        var autor = await CadastrarAutorAsync();
        var genero = await CadastrarGeneroAsync();

        await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Dom Casmurro", 1899, autor.Id, genero.Id));

        var resposta = await Cliente.DeleteAsync($"/api/generos/{genero.Id}");

        resposta.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("genero.possui_livros_vinculados");
    }

    [Fact]
    public async Task Delete_SemLivrosVinculados_DeveRemover()
    {
        var genero = await CadastrarGeneroAsync();

        var resposta = await Cliente.DeleteAsync($"/api/generos/{genero.Id}");

        resposta.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
