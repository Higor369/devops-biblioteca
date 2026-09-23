using Biblioteca.Domain.Common;

namespace Biblioteca.Domain.Services.Contratos;

/// <inheritdoc cref="IAutorDomainService"/>
public interface ILivroDomainService
{
    /// <summary>
    /// Garante que o autor e o gênero informados existem de fato no acervo.
    /// É a regra que impede um livro órfão: a entidade só sabe exigir que os ids
    /// tenham sido preenchidos, não que eles correspondam a registros reais.
    /// </summary>
    Task<Result> GarantirReferenciasValidasAsync(
        Guid autorId,
        Guid generoId,
        CancellationToken cancellationToken = default);
}
