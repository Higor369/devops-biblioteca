using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Domain.Common;

namespace Biblioteca.Application.Contratos;

/// <summary>
/// Casos de uso de autor. Orquestra domínio e persistência; não contém regra de negócio
/// própria — quando aparece uma, ela pertence à entidade ou a um serviço de domínio.
/// </summary>
public interface IAutorAppService
{
    Task<Result<AutorResponse>> CriarAsync(CriarAutorRequest request, CancellationToken cancellationToken = default);

    Task<Result<AutorResponse>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutorResponse>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Result<AutorResponse>> AtualizarAsync(
        Guid id,
        AtualizarAutorRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
