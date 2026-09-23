using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Services.Contratos;

/// <inheritdoc cref="IAutorDomainService"/>
public interface IGeneroDomainService
{
    /// <summary>
    /// Falha com conflito se já houver outro gênero com o mesmo nome (sem diferenciar
    /// maiúsculas de minúsculas).
    /// </summary>
    /// <param name="idIgnorado">Na edição, o próprio gênero não conta como duplicata.</param>
    Task<Result> GarantirNomeDisponivelAsync(
        NomeDeGenero nome,
        Guid? idIgnorado = null,
        CancellationToken cancellationToken = default);

    /// <summary>Falha com conflito se o gênero ainda tiver livros vinculados.</summary>
    Task<Result> GarantirRemocaoPermitidaAsync(Guid generoId, CancellationToken cancellationToken = default);
}
