using Biblioteca.Domain.Common;
using Biblioteca.Domain.Erros;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;

namespace Biblioteca.Domain.Services;

/// <inheritdoc cref="IAutorDomainService"/>
public sealed class AutorDomainService(ILivroRepository livroRepository) : IAutorDomainService
{
    public async Task<Result> GarantirRemocaoPermitidaAsync(
        Guid autorId,
        CancellationToken cancellationToken = default)
    {
        var livrosVinculados = await livroRepository.ContarPorAutorAsync(autorId, cancellationToken);

        // A contagem entra na mensagem de erro: dizer "existem 3 livros vinculados"
        // é mais util para quem consome a API do que um "operacao nao permitida".
        return livrosVinculados > 0
            ? Result.Failure(ErrosDeAutor.PossuiLivrosVinculados(livrosVinculados))
            : Result.Success();
    }
}
