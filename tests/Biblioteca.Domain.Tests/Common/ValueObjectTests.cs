using Biblioteca.Domain.Common;

namespace Biblioteca.Domain.Tests.Common;

/// <summary>
/// Contrato oposto ao de <see cref="Entity"/>: objeto de valor é comparado pelo conteúdo,
/// e não tem identidade nenhuma.
/// </summary>
public sealed class ValueObjectTests
{
    private sealed class Endereco(string rua, int numero) : ValueObject
    {
        protected override IEnumerable<object?> ObterComponentesDeIgualdade()
        {
            yield return rua;
            yield return numero;
        }
    }

    private sealed class Coordenada(string rua, int numero) : ValueObject
    {
        protected override IEnumerable<object?> ObterComponentesDeIgualdade()
        {
            yield return rua;
            yield return numero;
        }
    }

    [Fact]
    public void Objetos_ComOsMesmosComponentes_DevemSerIguais()
    {
        var primeiro = new Endereco("Rua das Flores", 100);
        var segundo = new Endereco("Rua das Flores", 100);

        // Instâncias diferentes, mesmo valor: para o domínio, a mesma coisa.
        ReferenceEquals(primeiro, segundo).ShouldBeFalse();
        (primeiro == segundo).ShouldBeTrue();
        primeiro.GetHashCode().ShouldBe(segundo.GetHashCode());
    }

    [Fact]
    public void Objetos_ComQualquerComponenteDiferente_NaoDevemSerIguais()
    {
        var primeiro = new Endereco("Rua das Flores", 100);

        (primeiro != new Endereco("Rua das Flores", 101)).ShouldBeTrue();
        (primeiro != new Endereco("Avenida Central", 100)).ShouldBeTrue();
    }

    [Fact]
    public void Objetos_DeTiposDiferentesComOsMesmosComponentes_NaoDevemSerIguais()
    {
        var endereco = new Endereco("Rua das Flores", 100);
        var coordenada = new Coordenada("Rua das Flores", 100);

        endereco.Equals(coordenada).ShouldBeFalse();
    }

    [Fact]
    public void Objeto_ComparadoComNulo_NaoDeveSerIgual()
    {
        var endereco = new Endereco("Rua das Flores", 100);

        endereco.Equals(null).ShouldBeFalse();
        (endereco == null).ShouldBeFalse();
    }
}
