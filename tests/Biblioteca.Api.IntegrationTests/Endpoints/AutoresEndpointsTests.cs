using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.IntegrationTests.Suporte;
using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Dtos.Livros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.IntegrationTests.Endpoints;

public sealed class AutoresEndpointsTests(BibliotecaApiFactory fabrica) : TesteDeIntegracao(fabrica)
{
    [Fact]
    public async Task Post_ComNomeValido_DeveRetornar201ComLocation()
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/autores", new CriarAutorRequest("Machado de Assis"));

        resposta.StatusCode.ShouldBe(HttpStatusCode.Created);
        resposta.Headers.Location.ShouldNotBeNull();

        var autor = await resposta.Content.ReadFromJsonAsync<AutorResponse>();
        autor!.Nome.ShouldBe("Machado de Assis");
        autor.Id.ShouldNotBe(Guid.Empty);
    }

    /// <summary>
    /// Nome só com espaços é barrado pela anotação <c>[Required]</c>, que descarta os
    /// espaços das bordas antes de decidir. Nem chega ao caso de uso.
    /// </summary>
    [Fact]
    public async Task Post_ComNomeEmBranco_DeveRetornar400DaValidacaoDeEntrada()
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/autores", new CriarAutorRequest("  "));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("requisicao.invalida");
    }

    /// <summary>
    /// Aqui a anotação não tem como ajudar: " a " tem três caracteres e passa no
    /// <c>StringLength</c>, mas vira "a" depois da normalização do objeto de valor.
    /// É o caso que mostra por que a validação de entrada não substitui a do domínio.
    /// </summary>
    [Fact]
    public async Task Post_ComNomeQueFicaCurtoAposNormalizacao_DeveRetornar400DoDominio()
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/autores", new CriarAutorRequest(" a "));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("autor.nome_tamanho_invalido");
    }

    [Fact]
    public async Task Post_DeveCarimbarDataDeCriacaoEDeixarAtualizacaoVazia()
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/autores", new CriarAutorRequest("Machado de Assis"));

        var autor = await resposta.Content.ReadFromJsonAsync<AutorResponse>();
        autor!.CriadoEm.ShouldNotBe(default);
        autor.AtualizadoEm.ShouldBeNull();
    }

    [Fact]
    public async Task Put_DeveCarimbarDataDeAtualizacaoPreservandoADeCriacao()
    {
        var criado = await CadastrarAutorAsync("Nome Antigo");

        await Cliente.PutAsJsonAsync($"/api/autores/{criado.Id}", new AtualizarAutorRequest("Nome Novo"));
        var atualizado = await Cliente.GetFromJsonAsync<AutorResponse>($"/api/autores/{criado.Id}");

        atualizado!.AtualizadoEm.ShouldNotBeNull();
        // A data de criação não pode ser reescrita a cada alteração.
        atualizado.CriadoEm.ShouldBe(criado.CriadoEm);
    }

    [Fact]
    public async Task Get_AposCadastro_DeveDevolverOAutorGravado()
    {
        var criado = await CadastrarAutorAsync("Clarice Lispector");

        var autor = await Cliente.GetFromJsonAsync<AutorResponse>($"/api/autores/{criado.Id}");

        autor!.Id.ShouldBe(criado.Id);
        autor.Nome.ShouldBe("Clarice Lispector");
    }

    [Fact]
    public async Task Get_ComIdInexistente_DeveRetornar404()
    {
        var resposta = await Cliente.GetAsync($"/api/autores/{Guid.CreateVersion7()}");

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DeveAlterarONomeGravado()
    {
        var criado = await CadastrarAutorAsync("Nome Antigo");

        var resposta = await Cliente.PutAsJsonAsync(
            $"/api/autores/{criado.Id}",
            new AtualizarAutorRequest("Nome Novo"));

        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);

        var autor = await Cliente.GetFromJsonAsync<AutorResponse>($"/api/autores/{criado.Id}");
        autor!.Nome.ShouldBe("Nome Novo");
    }

    [Fact]
    public async Task Delete_SemLivrosVinculados_DeveRemover()
    {
        var criado = await CadastrarAutorAsync();

        var resposta = await Cliente.DeleteAsync($"/api/autores/{criado.Id}");

        resposta.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await Cliente.GetAsync($"/api/autores/{criado.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Regra central do acervo: autor com livro vinculado não pode ser excluído.
    /// </summary>
    [Fact]
    public async Task Delete_ComLivrosVinculados_DeveRetornar409ComAQuantidade()
    {
        var autor = await CadastrarAutorAsync();
        var genero = await CadastrarGeneroAsync();

        await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Dom Casmurro", 1899, autor.Id, genero.Id));

        var resposta = await Cliente.DeleteAsync($"/api/autores/{autor.Id}");

        resposta.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var problema = await resposta.Content.ReadFromJsonAsync<ProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("autor.possui_livros_vinculados");
        problema.Detail!.ShouldContain("1");
    }

    [Fact]
    public async Task GetLista_DeveDevolverOsAutoresEmOrdemAlfabetica()
    {
        await CadastrarAutorAsync("Zeca");
        await CadastrarAutorAsync("Ana");

        var autores = await Cliente.GetFromJsonAsync<List<AutorResponse>>("/api/autores");

        autores!.Select(autor => autor.Nome).ShouldBe(["Ana", "Zeca"]);
    }
}
