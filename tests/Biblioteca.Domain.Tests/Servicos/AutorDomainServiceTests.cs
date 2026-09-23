using Biblioteca.Domain.Common;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services;
using NSubstitute;

namespace Biblioteca.Domain.Tests.Servicos;

public sealed class AutorDomainServiceTests
{
    private readonly ILivroRepository _livroRepository = Substitute.For<ILivroRepository>();

    [Fact]
    public async Task GarantirRemocaoPermitida_SemLivrosVinculados_DevePermitir()
    {
        var autorId = Guid.CreateVersion7();
        _livroRepository.ContarPorAutorAsync(autorId, Arg.Any<CancellationToken>()).Returns(0);

        var resultado = await new AutorDomainService(_livroRepository)
            .GarantirRemocaoPermitidaAsync(autorId);

        resultado.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task GarantirRemocaoPermitida_ComLivrosVinculados_DeveBloquearComConflito()
    {
        var autorId = Guid.CreateVersion7();
        _livroRepository.ContarPorAutorAsync(autorId, Arg.Any<CancellationToken>()).Returns(3);

        var resultado = await new AutorDomainService(_livroRepository)
            .GarantirRemocaoPermitidaAsync(autorId);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Type.ShouldBe(ErrorType.Conflict);
        resultado.Error.Code.ShouldBe("autor.possui_livros_vinculados");
        // A quantidade precisa chegar na mensagem: é o que torna o erro acionável.
        resultado.Error.Message.ShouldContain("3");
    }
}
