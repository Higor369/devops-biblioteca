using Biblioteca.Application.Dtos.Livros;
using Biblioteca.Application.Servicos;
using Biblioteca.Application.Tests.Suporte;
using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;
using NSubstitute;

namespace Biblioteca.Application.Tests.Servicos;

public sealed class LivroAppServiceTests
{
    private readonly ILivroRepository _livroRepository = Substitute.For<ILivroRepository>();
    private readonly IAutorRepository _autorRepository = Substitute.For<IAutorRepository>();
    private readonly IGeneroRepository _generoRepository = Substitute.For<IGeneroRepository>();
    private readonly ILivroDomainService _livroDomainService = Substitute.For<ILivroDomainService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RelogioFixo _relogio = new(ano: 2026);

    private readonly Autor _autor = Autor.Criar("Machado de Assis").Value;
    private readonly Genero _genero = Genero.Criar("Romance").Value;

    public LivroAppServiceTests()
    {
        _autorRepository.ObterPorIdAsync(_autor.Id, Arg.Any<CancellationToken>()).Returns(_autor);
        _generoRepository.ObterPorIdAsync(_genero.Id, Arg.Any<CancellationToken>()).Returns(_genero);
        _livroDomainService
            .GarantirReferenciasValidasAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    private LivroAppService CriarServico() => new(
        _livroRepository,
        _autorRepository,
        _generoRepository,
        _livroDomainService,
        _unitOfWork,
        _relogio);

    private CriarLivroRequest RequisicaoValida() =>
        new("Dom Casmurro", 1899, _autor.Id, _genero.Id);

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveDevolverLivroComAutorEGeneroExpandidos()
    {
        _livroRepository
            .ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => Livro.Criar("Dom Casmurro", 1899, _autor.Id, _genero.Id, _relogio).Value);

        var resultado = await CriarServico().CriarAsync(RequisicaoValida());

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.Titulo.ShouldBe("Dom Casmurro");
        resultado.Value.Autor.Nome.ShouldBe("Machado de Assis");
        resultado.Value.Genero.Nome.ShouldBe("Romance");
        await _livroRepository.Received(1).AdicionarAsync(Arg.Any<Livro>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_ComTituloInvalido_NemDeveConsultarAsReferencias()
    {
        var resultado = await CriarServico()
            .CriarAsync(RequisicaoValida() with { Titulo = "" });

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.Validation);
        await _livroDomainService.DidNotReceive().GarantirReferenciasValidasAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_ComAutorInexistente_DeveFalharSemPersistir()
    {
        _livroDomainService
            .GarantirReferenciasValidasAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.NotFound("autor.nao_encontrado", "sem autor")));

        var resultado = await CriarServico().CriarAsync(RequisicaoValida());

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.NotFound);
        await _livroRepository.DidNotReceive().AdicionarAsync(Arg.Any<Livro>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoExiste_DeveFalharComNaoEncontrado()
    {
        _livroRepository
            .ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Livro?)null);

        var resultado = await CriarServico().ObterPorIdAsync(Guid.CreateVersion7());

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.NotFound);
    }

    /// <summary>
    /// Protege contra a regressão mais provável desta camada: voltar a buscar autor e
    /// gênero dentro do laço, transformando uma listagem em N+1 consultas.
    /// </summary>
    [Fact]
    public async Task ListarAsync_ComVariosLivros_DeveCarregarAutoresEGenerosEmUmaConsultaCadaUm()
    {
        var outroAutor = Autor.Criar("Clarice Lispector").Value;
        var livros = new[]
        {
            Livro.Criar("Dom Casmurro", 1899, _autor.Id, _genero.Id, _relogio).Value,
            Livro.Criar("Quincas Borba", 1891, _autor.Id, _genero.Id, _relogio).Value,
            Livro.Criar("A Hora da Estrela", 1977, outroAutor.Id, _genero.Id, _relogio).Value
        };

        _livroRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns(livros);
        _autorRepository
            .ListarPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([_autor, outroAutor]);
        _generoRepository
            .ListarPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([_genero]);

        var resposta = await CriarServico().ListarAsync();

        resposta.Count.ShouldBe(3);
        resposta[2].Autor.Nome.ShouldBe("Clarice Lispector");

        await _autorRepository.Received(1)
            .ListarPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _generoRepository.Received(1)
            .ListarPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _autorRepository.DidNotReceive()
            .ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListarAsync_SemLivros_NaoDeveConsultarAutoresNemGeneros()
    {
        _livroRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns([]);

        var resposta = await CriarServico().ListarAsync();

        resposta.ShouldBeEmpty();
        await _autorRepository.DidNotReceive()
            .ListarPorIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }
}
