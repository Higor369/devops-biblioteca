using Biblioteca.Domain.Entidades;

namespace Biblioteca.Domain.Repositories;

/// <inheritdoc cref="IAutorRepository"/>
public interface ILivroRepository
{
    Task<Livro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Livro>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>Quantos livros apontam para este autor. Usado na regra de exclusão.</summary>
    Task<int> ContarPorAutorAsync(Guid autorId, CancellationToken cancellationToken = default);

    /// <summary>Quantos livros apontam para este gênero. Usado na regra de exclusão.</summary>
    Task<int> ContarPorGeneroAsync(Guid generoId, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Livro livro, CancellationToken cancellationToken = default);

    void Atualizar(Livro livro);

    void Remover(Livro livro);
}
