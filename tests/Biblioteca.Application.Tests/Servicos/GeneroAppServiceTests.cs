using Biblioteca.Application.Dtos.Generos;
using Biblioteca.Application.Servicos;
using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;
using NSubstitute;

namespace Biblioteca.Application.Tests.Servicos;

public sealed class GeneroAppServiceTests
{
    private readonly IGeneroRepository _generoRepository = Substitute.For<IGeneroRepository>();
    private readonly IGeneroDomainService _generoDomainService = Substitute.For<IGeneroDomainService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public GeneroAppServiceTests() =>
        // Por padrão o nome está livre; cada teste que precisa do contrário sobrescreve.
        _generoDomainService
            .GarantirNomeDisponivelAsync(Arg.Any<NomeDeGenero>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

    private GeneroAppService CriarServico() => new(_generoRepository, _generoDomainService, _unitOfWork);

    [Fact]
    public async Task CriarAsync_ComNomeDisponivel_DevePersistir()
    {
        var resultado = await CriarServico().CriarAsync(new CriarGeneroRequest("Romance"));

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.Nome.ShouldBe("Romance");
        await _generoRepository.Received(1).AdicionarAsync(Arg.Any<Genero>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_ComNomeJaCadastrado_DeveFalharSemPersistir()
    {
        _generoDomainService
            .GarantirNomeDisponivelAsync(NomeDeGenero.Criar("Romance").Value, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Conflict("genero.nome_ja_cadastrado", "duplicado")));

        var resultado = await CriarServico().CriarAsync(new CriarGeneroRequest("Romance"));

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.Conflict);
        await _generoRepository.DidNotReceive().AdicionarAsync(Arg.Any<Genero>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_ComNomeInvalido_NemDeveConsultarDuplicidade()
    {
        var resultado = await CriarServico().CriarAsync(new CriarGeneroRequest(" "));

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.Validation);
        // Validar antes de consultar evita uma ida ao banco que já se sabe inútil.
        await _generoDomainService.DidNotReceive()
            .GarantirNomeDisponivelAsync(Arg.Any<NomeDeGenero>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_DeveIgnorarOProprioGeneroNaChecagemDeDuplicidade()
    {
        var genero = Genero.Criar("Romance").Value;
        _generoRepository.ObterPorIdAsync(genero.Id, Arg.Any<CancellationToken>()).Returns(genero);

        var resultado = await CriarServico().AtualizarAsync(genero.Id, new AtualizarGeneroRequest("Romance Histórico"));

        resultado.IsSuccess.ShouldBeTrue();
        await _generoDomainService.Received(1).GarantirNomeDisponivelAsync(
            NomeDeGenero.Criar("Romance Histórico").Value,
            genero.Id,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoverAsync_QuandoRegraDeDominioBloqueia_NaoDeveRemover()
    {
        var genero = Genero.Criar("Romance").Value;
        _generoRepository.ObterPorIdAsync(genero.Id, Arg.Any<CancellationToken>()).Returns(genero);
        _generoDomainService
            .GarantirRemocaoPermitidaAsync(genero.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Conflict("genero.possui_livros_vinculados", "tem livros")));

        var resultado = await CriarServico().RemoverAsync(genero.Id);

        resultado.IsFailure.ShouldBeTrue();
        _generoRepository.DidNotReceive().Remover(Arg.Any<Genero>());
    }
}
