using Biblioteca.Domain.Common;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services;
using NSubstitute;

namespace Biblioteca.Domain.Tests.Servicos;

public sealed class LivroDomainServiceTests
{
    private readonly IAutorRepository _autorRepository = Substitute.For<IAutorRepository>();
    private readonly IGeneroRepository _generoRepository = Substitute.For<IGeneroRepository>();

    private readonly Guid _autorId = Guid.CreateVersion7();
    private readonly Guid _generoId = Guid.CreateVersion7();

    private LivroDomainService CriarServico() => new(_autorRepository, _generoRepository);

    [Fact]
    public async Task GarantirReferenciasValidas_ComAutorEGeneroExistentes_DevePermitir()
    {
        _autorRepository.ExisteAsync(_autorId, Arg.Any<CancellationToken>()).Returns(true);
        _generoRepository.ExisteAsync(_generoId, Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CriarServico().GarantirReferenciasValidasAsync(_autorId, _generoId);

        resultado.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task GarantirReferenciasValidas_ComAutorInexistente_DeveFalharComNaoEncontrado()
    {
        _autorRepository.ExisteAsync(_autorId, Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await CriarServico().GarantirReferenciasValidasAsync(_autorId, _generoId);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.NotFound);
        resultado.Error.Code.ShouldBe("autor.nao_encontrado");
    }

    [Fact]
    public async Task GarantirReferenciasValidas_ComAutorInexistente_NemDeveConsultarOGenero()
    {
        _autorRepository.ExisteAsync(_autorId, Arg.Any<CancellationToken>()).Returns(false);

        await CriarServico().GarantirReferenciasValidasAsync(_autorId, _generoId);

        await _generoRepository.DidNotReceive().ExisteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GarantirReferenciasValidas_ComGeneroInexistente_DeveFalharComNaoEncontrado()
    {
        _autorRepository.ExisteAsync(_autorId, Arg.Any<CancellationToken>()).Returns(true);
        _generoRepository.ExisteAsync(_generoId, Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await CriarServico().GarantirReferenciasValidasAsync(_autorId, _generoId);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.NotFound);
        resultado.Error.Code.ShouldBe("genero.nao_encontrado");
    }
}
