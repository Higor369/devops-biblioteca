using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Tests.Entidades;

public sealed class GeneroTests
{
    [Fact]
    public void Criar_ComNomeValido_DeveProduzirGeneroValido()
    {
        var resultado = Genero.Criar("Ficção Científica");

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.Nome.Valor.ShouldBe("Ficção Científica");
        resultado.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Criar_SemNome_DeveFalhar(string? nome)
    {
        var resultado = Genero.Criar(nome!);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("genero.nome_obrigatorio");
    }

    [Fact]
    public void Criar_ComNomeLongoDemais_DeveFalhar()
    {
        var resultado = Genero.Criar(new string('a', NomeDeGenero.TamanhoMaximo + 1));

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("genero.nome_tamanho_invalido");
    }

    [Fact]
    public void AlterarNome_ComNomeInvalido_DevePreservarOValorAnterior()
    {
        var genero = Genero.Criar("Romance").Value;

        genero.AlterarNome(" ").IsFailure.ShouldBeTrue();

        genero.Nome.Valor.ShouldBe("Romance");
    }
}
