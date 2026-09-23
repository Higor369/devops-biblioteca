using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Servicos;
using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;
using NSubstitute;

namespace Biblioteca.Application.Tests.Servicos;

public sealed class AutorAppServiceTests
{
    private readonly IAutorRepository _autorRepository = Substitute.For<IAutorRepository>();
    private readonly IAutorDomainService _autorDomainService = Substitute.For<IAutorDomainService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private AutorAppService CriarServico() => new(_autorRepository, _autorDomainService, _unitOfWork);

    [Fact]
    public async Task CriarAsync_ComNomeValido_DevePersistirEConfirmar()
    {
        var resultado = await CriarServico().CriarAsync(new CriarAutorRequest("Machado de Assis"));

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.Nome.ShouldBe("Machado de Assis");
        await _autorRepository.Received(1).AdicionarAsync(Arg.Any<Autor>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_ComNomeInvalido_NaoDeveTocarNaPersistencia()
    {
        var resultado = await CriarServico().CriarAsync(new CriarAutorRequest(""));

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.Validation);
        await _autorRepository.DidNotReceive().AdicionarAsync(Arg.Any<Autor>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoExiste_DeveFalharComNaoEncontrado()
    {
        var id = Guid.CreateVersion7();
        _autorRepository.ObterPorIdAsync(id, Arg.Any<CancellationToken>()).Returns((Autor?)null);

        var resultado = await CriarServico().ObterPorIdAsync(id);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task AtualizarAsync_ComNomeInvalido_NaoDeveConfirmarAlteracao()
    {
        var autor = Autor.Criar("Nome Original").Value;
        _autorRepository.ObterPorIdAsync(autor.Id, Arg.Any<CancellationToken>()).Returns(autor);

        var resultado = await CriarServico().AtualizarAsync(autor.Id, new AtualizarAutorRequest("x"));

        resultado.IsFailure.ShouldBeTrue();
        autor.Nome.Valor.ShouldBe("Nome Original");
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoverAsync_QuandoRegraDeDominioBloqueia_NaoDeveRemover()
    {
        var autor = Autor.Criar("Autor Com Livros").Value;
        _autorRepository.ObterPorIdAsync(autor.Id, Arg.Any<CancellationToken>()).Returns(autor);
        _autorDomainService
            .GarantirRemocaoPermitidaAsync(autor.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Conflict("autor.possui_livros_vinculados", "tem livros")));

        var resultado = await CriarServico().RemoverAsync(autor.Id);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.Conflict);
        _autorRepository.DidNotReceive().Remover(Arg.Any<Autor>());
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoverAsync_QuandoRegraDeDominioPermite_DeveRemoverEConfirmar()
    {
        var autor = Autor.Criar("Autor Sem Livros").Value;
        _autorRepository.ObterPorIdAsync(autor.Id, Arg.Any<CancellationToken>()).Returns(autor);
        _autorDomainService
            .GarantirRemocaoPermitidaAsync(autor.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var resultado = await CriarServico().RemoverAsync(autor.Id);

        resultado.IsSuccess.ShouldBeTrue();
        _autorRepository.Received(1).Remover(autor);
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
}
