using Biblioteca.Application.Dtos.Generos;
using Biblioteca.Domain.Common;

namespace Biblioteca.Application.Contratos;

/// <inheritdoc cref="IAutorAppService"/>
public interface IGeneroAppService
{
    Task<Result<GeneroResponse>> CriarAsync(CriarGeneroRequest request, CancellationToken cancellationToken = default);

    Task<Result<GeneroResponse>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeneroResponse>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Result<GeneroResponse>> AtualizarAsync(
        Guid id,
        AtualizarGeneroRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
