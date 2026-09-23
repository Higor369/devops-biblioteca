using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Tests.Entidades;

public sealed class AutorTests
{
    [Fact]
    public void Criar_ComNomeValido_DeveProduzirAutorValido()
    {
        var resultado = Autor.Criar("Machado de Assis");

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.Nome.Valor.ShouldBe("Machado de Assis");
        resultado.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemNome_DeveFalhar(string? nome)
    {
        var resultado = Autor.Criar(nome!);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("autor.nome_obrigatorio");
    }

    [Fact]
    public void Criar_ComNomeCurtoDemais_DeveFalhar()
    {
        var resultado = Autor.Criar("M");

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("autor.nome_tamanho_invalido");
    }

    [Fact]
    public void Criar_ComNomeLongoDemais_DeveFalhar()
    {
        var nomeExcessivo = new string('a', NomeDeAutor.TamanhoMaximo + 1);

        var resultado = Autor.Criar(nomeExcessivo);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("autor.nome_tamanho_invalido");
    }

    [Fact]
    public void Criar_DeveDescartarEspacosDasBordas()
    {
        var resultado = Autor.Criar("   Clarice Lispector   ");

        resultado.Value.Nome.Valor.ShouldBe("Clarice Lispector");
    }

    [Fact]
    public void Criar_DuasVezes_DeveGerarIdsDistintos()
    {
        var primeiro = Autor.Criar("Jorge Amado").Value;
        var segundo = Autor.Criar("Jorge Amado").Value;

        segundo.Id.ShouldNotBe(primeiro.Id);
    }

    [Fact]
    public void AlterarNome_ComNomeValido_DeveAtualizar()
    {
        var autor = Autor.Criar("Nome Antigo").Value;

        var resultado = autor.AlterarNome("Nome Novo");

        resultado.IsSuccess.ShouldBeTrue();
        autor.Nome.Valor.ShouldBe("Nome Novo");
    }

    [Fact]
    public void AlterarNome_ComNomeInvalido_DevePreservarOValorAnterior()
    {
        var autor = Autor.Criar("Nome Válido").Value;

        var resultado = autor.AlterarNome("");

        resultado.IsFailure.ShouldBeTrue();
        // O ponto do teste: falhar não pode deixar a entidade em estado intermediário.
        autor.Nome.Valor.ShouldBe("Nome Válido");
    }
}
