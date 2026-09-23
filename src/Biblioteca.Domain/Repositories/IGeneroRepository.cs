using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Repositories;

/// <inheritdoc cref="IAutorRepository"/>
public interface IGeneroRepository
{
    Task<Genero?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Genero>> ListarAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IAutorRepository.ListarPorIdsAsync"/>
    Task<IReadOnlyList<Genero>> ListarPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <param name="idIgnorado">
    /// Usado na edição: o próprio gênero sendo alterado não pode ser contado como
    /// duplicata de si mesmo.
    /// </param>
    Task<bool> ExisteComNomeAsync(
        NomeDeGenero nome,
        Guid? idIgnorado = null,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Genero genero, CancellationToken cancellationToken = default);

    void Atualizar(Genero genero);

    void Remover(Genero genero);
}
