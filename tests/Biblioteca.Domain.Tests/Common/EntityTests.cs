using Biblioteca.Domain.Common;

namespace Biblioteca.Domain.Tests.Common;

/// <summary>
/// O que se testa aqui é o contrato de identidade do DDD: entidade é comparada pelo id,
/// nunca pelo conteúdo.
/// </summary>
public sealed class EntityTests
{
    // Entidades de teste: as reais não permitem escolher o id, e aqui é justamente isso
    // que precisa ser controlado.
    private sealed class EntidadeDeTeste(Guid id) : Entity(id);

    private sealed class OutraEntidadeDeTeste(Guid id) : Entity(id);

    [Fact]
    public void Entidades_ComOMesmoId_DevemSerIguais()
    {
        var id = Guid.CreateVersion7();

        var primeira = new EntidadeDeTeste(id);
        var segunda = new EntidadeDeTeste(id);

        primeira.Equals(segunda).ShouldBeTrue();
        (primeira == segunda).ShouldBeTrue();
        primeira.GetHashCode().ShouldBe(segunda.GetHashCode());
    }

    [Fact]
    public void Entidades_ComIdsDiferentes_NaoDevemSerIguais()
    {
        var primeira = new EntidadeDeTeste(Guid.CreateVersion7());
        var segunda = new EntidadeDeTeste(Guid.CreateVersion7());

        (primeira != segunda).ShouldBeTrue();
    }

    [Fact]
    public void Entidades_DeTiposDiferentesComOMesmoId_NaoDevemSerIguais()
    {
        var id = Guid.CreateVersion7();

        var entidade = new EntidadeDeTeste(id);
        var outra = new OutraEntidadeDeTeste(id);

        entidade.Equals(outra).ShouldBeFalse();
    }

    [Fact]
    public void Entidade_SemIdAtribuido_SoDeveSerIgualASiMesma()
    {
        var primeira = new EntidadeDeTeste(Guid.Empty);
        var segunda = new EntidadeDeTeste(Guid.Empty);

        // Duas entidades ainda não identificadas não são "a mesma coisa" só por
        // compartilharem a ausência de id.
        primeira.Equals(segunda).ShouldBeFalse();
        primeira.Equals(primeira).ShouldBeTrue();
    }

    [Fact]
    public void Entidade_ComparadaComNulo_NaoDeveSerIgual()
    {
        var entidade = new EntidadeDeTeste(Guid.CreateVersion7());

        entidade.Equals(null).ShouldBeFalse();
        (entidade == null).ShouldBeFalse();
    }
}
