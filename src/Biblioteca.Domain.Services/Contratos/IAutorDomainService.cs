using Biblioteca.Domain.Common;

namespace Biblioteca.Domain.Services.Contratos;

/// <summary>
/// Regras de negócio de autor que dependem de consultar outros agregados —
/// por isso não cabem dentro da entidade <c>Autor</c>, que não enxerga livros.
/// </summary>
public interface IAutorDomainService
{
    /// <summary>
    /// Falha com conflito se o autor ainda tiver livros vinculados.
    /// </summary>
    Task<Result> GarantirRemocaoPermitidaAsync(Guid autorId, CancellationToken cancellationToken = default);
}
