using System.Net.Http.Json;
using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Dtos.Generos;

namespace Biblioteca.Api.IntegrationTests.Suporte;

/// <summary>
/// Base das classes de teste de integração: entrega um cliente HTTP apontado para a
/// API, garante banco limpo antes de cada teste e oferece atalhos para montar o
/// cenário (um autor e um gênero já cadastrados).
/// </summary>
[Collection(ColecaoDeIntegracao.Nome)]
public abstract class TesteDeIntegracao(BibliotecaApiFactory fabrica) : IAsyncLifetime
{
    protected HttpClient Cliente { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await fabrica.LimparBancoAsync();
        Cliente = fabrica.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<AutorResponse> CadastrarAutorAsync(string nome = "Machado de Assis")
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/autores", new CriarAutorRequest(nome));
        resposta.EnsureSuccessStatusCode();

        return (await resposta.Content.ReadFromJsonAsync<AutorResponse>())!;
    }

    protected async Task<GeneroResponse> CadastrarGeneroAsync(string nome = "Romance")
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/generos", new CriarGeneroRequest(nome));
        resposta.EnsureSuccessStatusCode();

        return (await resposta.Content.ReadFromJsonAsync<GeneroResponse>())!;
    }
}
