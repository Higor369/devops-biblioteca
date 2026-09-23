using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services;
using NSubstitute;

namespace Biblioteca.Domain.Tests.Servicos;

public sealed class GeneroDomainServiceTests
{
    private readonly IGeneroRepository _generoRepository = Substitute.For<IGeneroRepository>();
    private readonly ILivroRepository _livroRepository = Substitute.For<ILivroRepository>();

    private static readonly NomeDeGenero Romance = NomeDeGenero.Criar("Romance").Value;

    private GeneroDomainService CriarServico() => new(_generoRepository, _livroRepository);

    [Fact]
    public async Task GarantirNomeDisponivel_QuandoNomeLivre_DevePermitir()
    {
        _generoRepository
            .ExisteComNomeAsync(Romance, null, Arg.Any<CancellationToken>())
            .Returns(false);

        var resultado = await CriarServico().GarantirNomeDisponivelAsync(Romance);

        resultado.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task GarantirNomeDisponivel_QuandoNomeJaExiste_DeveBloquearComConflito()
    {
        _generoRepository
            .ExisteComNomeAsync(Romance, null, Arg.Any<CancellationToken>())
            .Returns(true);

        var resultado = await CriarServico().GarantirNomeDisponivelAsync(Romance);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.Conflict);
        resultado.Error.Code.ShouldBe("genero.nome_ja_cadastrado");
    }

    /// <summary>
    /// O nome chega ao repositório como objeto de valor, e o NSubstitute só casa o
    /// argumento porque <see cref="NomeDeGenero"/> compara por conteúdo. Ou seja: este
    /// teste também prova que a igualdade do objeto de valor funciona.
    /// </summary>
    [Fact]
    public async Task GarantirNomeDisponivel_DeveConsultarComOMesmoNomeRecebido()
    {
        _generoRepository
            .ExisteComNomeAsync(Arg.Any<NomeDeGenero>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await CriarServico().GarantirNomeDisponivelAsync(NomeDeGenero.Criar("  Romance  ").Value);

        await _generoRepository.Received(1)
            .ExisteComNomeAsync(Romance, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GarantirNomeDisponivel_NaEdicao_NaoDeveColidirComOProprioGenero()
    {
        var generoId = Guid.CreateVersion7();
        _generoRepository
            .ExisteComNomeAsync(Romance, generoId, Arg.Any<CancellationToken>())
            .Returns(false);

        var resultado = await CriarServico().GarantirNomeDisponivelAsync(Romance, generoId);

        resultado.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task GarantirRemocaoPermitida_ComLivrosVinculados_DeveBloquearComConflito()
    {
        var generoId = Guid.CreateVersion7();
        _livroRepository.ContarPorGeneroAsync(generoId, Arg.Any<CancellationToken>()).Returns(2);

        var resultado = await CriarServico().GarantirRemocaoPermitidaAsync(generoId);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("genero.possui_livros_vinculados");
        resultado.Error.Message.ShouldContain("2");
    }

    [Fact]
    public async Task GarantirRemocaoPermitida_SemLivrosVinculados_DevePermitir()
    {
        var generoId = Guid.CreateVersion7();
        _livroRepository.ContarPorGeneroAsync(generoId, Arg.Any<CancellationToken>()).Returns(0);

        var resultado = await CriarServico().GarantirRemocaoPermitidaAsync(generoId);

        resultado.IsSuccess.ShouldBeTrue();
    }
}
