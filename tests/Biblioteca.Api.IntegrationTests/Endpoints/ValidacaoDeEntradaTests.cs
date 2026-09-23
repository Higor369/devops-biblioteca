using System.Net;
using System.Net.Http.Json;
using Biblioteca.Api.IntegrationTests.Suporte;
using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Dtos.Livros;
using Biblioteca.Domain.Entidades.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Api.IntegrationTests.Endpoints;

/// <summary>
/// Cobre a primeira barreira da API: as <c>DataAnnotations</c> nos DTOs de entrada.
/// <para>
/// Elas rejeitam a requisição malformada antes de qualquer caso de uso rodar, e a
/// resposta sai no mesmo formato dos erros de negócio — com <c>codigo</c> e a lista
/// de campos que falharam.
/// </para>
/// </summary>
public sealed class ValidacaoDeEntradaTests(BibliotecaApiFactory fabrica) : TesteDeIntegracao(fabrica)
{
    [Fact]
    public async Task Post_ComNomeAcimaDoTamanhoMaximo_DeveRetornar400ApontandoOCampo()
    {
        var nomeExcessivo = new string('a', NomeDeAutor.TamanhoMaximo + 1);

        var resposta = await Cliente.PostAsJsonAsync("/api/autores", new CriarAutorRequest(nomeExcessivo));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problema!.Extensions["codigo"]!.ToString().ShouldBe("requisicao.invalida");
        problema.Errors.ShouldContainKey("Nome");
    }

    [Fact]
    public async Task Post_DeveResponderComOTipoDeConteudoDeProblema()
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/autores", new CriarAutorRequest(""));

        resposta.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    /// <summary>
    /// O caso que motivou a anotação própria: <c>[Required]</c> não detecta um
    /// <see cref="Guid"/> ausente, porque a ausência chega como <see cref="Guid.Empty"/>.
    /// </summary>
    [Fact]
    public async Task PostLivro_SemAutorId_DeveRetornar400ApontandoOCampo()
    {
        var genero = await CadastrarGeneroAsync();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Dom Casmurro", 1899, Guid.Empty, genero.Id));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problema!.Errors.ShouldContainKey("AutorId");
    }

    [Fact]
    public async Task PostLivro_SemGeneroId_DeveRetornar400ApontandoOCampo()
    {
        var autor = await CadastrarAutorAsync();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Dom Casmurro", 1899, autor.Id, Guid.Empty));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problema!.Errors.ShouldContainKey("GeneroId");
    }

    [Fact]
    public async Task PostLivro_ComAnoAnteriorAoMinimo_DeveSerBarradoPelaAnotacao()
    {
        var autor = await CadastrarAutorAsync();
        var genero = await CadastrarGeneroAsync();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/livros",
            new CriarLivroRequest("Manuscrito", AnoDePublicacao.AnoMinimo - 1, autor.Id, genero.Id));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problema!.Errors.ShouldContainKey("AnoDePublicacao");
    }

    /// <summary>
    /// O limite superior do ano não cabe em anotação, porque depende do ano corrente.
    /// Esta é a divisão de trabalho: mínimo na anotação, "não pode ser futuro" no domínio.
    /// </summary>
    [Fact]
    public async Task PostLivro_ComAnoNoFuturo_DeveSerBarradoPeloDominio()
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
}
