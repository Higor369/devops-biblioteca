using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;
using Biblioteca.Domain.Tests.Suporte;

namespace Biblioteca.Domain.Tests.Entidades;

public sealed class LivroTests
{
    private static readonly RelogioFixo Relogio = new(ano: 2026);
    private static readonly Guid AutorId = Guid.CreateVersion7();
    private static readonly Guid GeneroId = Guid.CreateVersion7();

    [Fact]
    public void Criar_ComDadosValidos_DeveProduzirLivroValido()
    {
        var resultado = Livro.Criar("Dom Casmurro", 1899, AutorId, GeneroId, Relogio);

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.Titulo.Valor.ShouldBe("Dom Casmurro");
        resultado.Value.AnoDePublicacao.Valor.ShouldBe(1899);
        resultado.Value.AutorId.ShouldBe(AutorId);
        resultado.Value.GeneroId.ShouldBe(GeneroId);
        resultado.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemTitulo_DeveFalhar(string? titulo)
    {
        var resultado = Livro.Criar(titulo!, 1899, AutorId, GeneroId, Relogio);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("livro.titulo_obrigatorio");
    }

    [Fact]
    public void Criar_SemAutor_DeveFalhar()
    {
        var resultado = Livro.Criar("Dom Casmurro", 1899, Guid.Empty, GeneroId, Relogio);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("livro.autor_obrigatorio");
    }

    [Fact]
    public void Criar_SemGenero_DeveFalhar()
    {
        var resultado = Livro.Criar("Dom Casmurro", 1899, AutorId, Guid.Empty, Relogio);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("livro.genero_obrigatorio");
    }

    [Fact]
    public void Criar_ComAnoAnteriorAImprensa_DeveFalhar()
    {
        var resultado = Livro.Criar(
            "Manuscrito",
            AnoDePublicacao.AnoMinimo - 1,
            AutorId,
            GeneroId,
            Relogio);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("livro.ano_publicacao_invalido");
    }

    [Fact]
    public void Criar_ComAnoNoFuturo_DeveFalhar()
    {
        var resultado = Livro.Criar("Obra Futura", Relogio.Ano + 1, AutorId, GeneroId, Relogio);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("livro.ano_publicacao_invalido");
    }

    [Fact]
    public void Criar_ComAnoCorrente_DeveSerAceito()
    {
        var resultado = Livro.Criar("Lançamento", Relogio.Ano, AutorId, GeneroId, Relogio);

        resultado.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Requisito explícito do acervo: livros repetidos são permitidos e se distinguem
    /// apenas pelo id.
    /// </summary>
    [Fact]
    public void Criar_DoisLivrosIdenticos_DevemSerRegistrosDistintos()
    {
        var primeiro = Livro.Criar("Dom Casmurro", 1899, AutorId, GeneroId, Relogio).Value;
        var segundo = Livro.Criar("Dom Casmurro", 1899, AutorId, GeneroId, Relogio).Value;

        segundo.Id.ShouldNotBe(primeiro.Id);
        segundo.Titulo.ShouldBe(primeiro.Titulo);
        segundo.AutorId.ShouldBe(primeiro.AutorId);
        segundo.GeneroId.ShouldBe(primeiro.GeneroId);
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveRefletirAsAlteracoes()
    {
        var livro = Livro.Criar("Título Antigo", 1899, AutorId, GeneroId, Relogio).Value;
        var novoAutorId = Guid.CreateVersion7();

        var resultado = livro.Atualizar("Título Novo", 1900, novoAutorId, GeneroId, Relogio);

        resultado.IsSuccess.ShouldBeTrue();
        livro.Titulo.Valor.ShouldBe("Título Novo");
        livro.AnoDePublicacao.Valor.ShouldBe(1900);
        livro.AutorId.ShouldBe(novoAutorId);
    }

    [Fact]
    public void Atualizar_ComDadosInvalidos_NaoDeveAlterarNenhumCampo()
    {
        var livro = Livro.Criar("Título Antigo", 1899, AutorId, GeneroId, Relogio).Value;

        var resultado = livro.Atualizar("", 1900, AutorId, GeneroId, Relogio);

        resultado.IsFailure.ShouldBeTrue();
        // Nenhum campo pode ter sido escrito antes da validação falhar.
        livro.Titulo.Valor.ShouldBe("Título Antigo");
        livro.AnoDePublicacao.Valor.ShouldBe(1899);
    }
}
