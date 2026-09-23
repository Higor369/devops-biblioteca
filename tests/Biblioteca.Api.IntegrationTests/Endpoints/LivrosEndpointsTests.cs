using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.IntegrationTests.Suporte;
using Biblioteca.Application.Dtos.Livros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.IntegrationTests.Endpoints;

public sealed class LivrosEndpointsTests(BibliotecaApiFactory fabrica) : TesteDeIntegracao(fabrica)
{
    [Fact]
    public async Task Post_ComAutorEGeneroValidos_DeveRetornar201ComOsDadosExpandidos()
    {
        var autor = await CadastrarAutorAsync("Machado de Assis");
        var genero = await CadastrarGeneroAsync("Romance");

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Dom Casmurro", 1899, autor.Id, genero.Id));

        resposta.StatusCode.ShouldBe(HttpStatusCode.Created);

        var livro = await resposta.Content.ReadFromJsonAsync<LivroResponse>();
        livro!.Titulo.ShouldBe("Dom Casmurro");
        livro.AnoDePublicacao.ShouldBe(1899);
        livro.Autor.Nome.ShouldBe("Machado de Assis");
        livro.Genero.Nome.ShouldBe("Romance");
    }

    [Fact]
    public async Task Post_ComAutorInexistente_DeveRetornar404()
    {
        var genero = await CadastrarGeneroAsync();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Livro Órfão", 1899, Guid.CreateVersion7(), genero.Id));

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("autor.nao_encontrado");
    }

    [Fact]
    public async Task Post_ComGeneroInexistente_DeveRetornar404()
    {
        var autor = await CadastrarAutorAsync();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Livro Órfão", 1899, autor.Id, Guid.CreateVersion7()));

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("genero.nao_encontrado");
    }

    [Fact]
    public async Task Post_ComAnoNoFuturo_DeveRetornar400()
    {
        var autor = await CadastrarAutorAsync();
        var genero = await CadastrarGeneroAsync();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Obra Futura", DateTime.UtcNow.Year + 1, autor.Id, genero.Id));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("livro.ano_publicacao_invalido");
    }

    /// <summary>
    /// Requisito explícito: o acervo aceita dois livros idênticos, distinguidos só pelo id.
    /// </summary>
    [Fact]
    public async Task Post_DuasVezesComOsMesmosDados_DeveCriarDoisRegistrosDistintos()
    {
        var autor = await CadastrarAutorAsync();
        var genero = await CadastrarGeneroAsync();
        var requisicao = new CriarLivroRequest("Dom Casmurro", 1899, autor.Id, genero.Id);

        var primeira = await Cliente.PostAsJsonAsync("/api/livros", requisicao);
        var segunda = await Cliente.PostAsJsonAsync("/api/livros", requisicao);

        primeira.StatusCode.ShouldBe(HttpStatusCode.Created);
        segunda.StatusCode.ShouldBe(HttpStatusCode.Created);

        var primeiroLivro = await primeira.Content.ReadFromJsonAsync<LivroResponse>();
        var segundoLivro = await segunda.Content.ReadFromJsonAsync<LivroResponse>();

        segundoLivro!.Id.ShouldNotBe(primeiroLivro!.Id);

        var livros = await Cliente.GetFromJsonAsync<List<LivroResponse>>("/api/livros");
        livros!.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Put_DeveAlterarOsDadosGravados()
    {
        var autor = await CadastrarAutorAsync();
        var genero = await CadastrarGeneroAsync();
        var criacao = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Título Antigo", 1899, autor.Id, genero.Id));
        var livro = (await criacao.Content.ReadFromJsonAsync<LivroResponse>())!;

        var resposta = await Cliente.PutAsJsonAsync(
            $"/api/livros/{livro.Id}",
            new AtualizarLivroRequest("Título Novo", 1900, autor.Id, genero.Id));

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);

        var atualizado = await Cliente.GetFromJsonAsync<LivroResponse>($"/api/livros/{livro.Id}");
        atualizado!.Titulo.ShouldBe("Título Novo");
        atualizado.AnoDePublicacao.ShouldBe(1900);
    }

    [Fact]
    public async Task Delete_DeveRemoverOLivroELiberarAExclusaoDoAutor()
    {
        var autor = await CadastrarAutorAsync();
        var genero = await CadastrarGeneroAsync();
        var criacao = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Dom Casmurro", 1899, autor.Id, genero.Id));
        var livro = (await criacao.Content.ReadFromJsonAsync<LivroResponse>())!;

        (await Cliente.DeleteAsync($"/api/livros/{livro.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Sem livros vinculados, o autor volta a poder ser excluído.
        (await Cliente.DeleteAsync($"/api/autores/{autor.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Get_ComIdInexistente_DeveRetornar404()
    {
        var resposta = await Cliente.GetAsync($"/api/livros/{Guid.CreateVersion7()}");

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
