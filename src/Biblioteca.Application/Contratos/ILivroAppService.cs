using Biblioteca.Application.Dtos.Livros;
using Biblioteca.Domain.Common;

namespace Biblioteca.Application.Contratos;

/// <inheritdoc cref="IAutorAppService"/>
public interface ILivroAppService
{
    Task<Result<LivroResponse>> CriarAsync(CriarLivroRequest request, CancellationToken cancellationToken = default);

    Task<Result<LivroResponse>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LivroResponse>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Result<LivroResponse>> AtualizarAsync(
        Guid id,
        AtualizarLivroRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
