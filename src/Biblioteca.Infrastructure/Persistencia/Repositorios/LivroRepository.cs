using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure.Persistencia.Repositorios;

/// <inheritdoc cref="AutorRepository"/>
internal sealed class LivroRepository(BibliotecaDbContext contexto) : ILivroRepository
{
    public async Task<Livro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await contexto.Livros.FirstOrDefaultAsync(livro => livro.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Livro>> ListarAsync(CancellationToken cancellationToken = default) =>
        await contexto.Livros
            .AsNoTracking()
            .OrderBy(livro => livro.Titulo)
            .ToListAsync(cancellationToken);

    public Task<int> ContarPorAutorAsync(Guid autorId, CancellationToken cancellationToken = default) =>
        contexto.Livros.CountAsync(livro => livro.AutorId == autorId, cancellationToken);

    public Task<int> ContarPorGeneroAsync(Guid generoId, CancellationToken cancellationToken = default) =>
        contexto.Livros.CountAsync(livro => livro.GeneroId == generoId, cancellationToken);

    public async Task AdicionarAsync(Livro livro, CancellationToken cancellationToken = default) =>
        await contexto.Livros.AddAsync(livro, cancellationToken);

    public void Atualizar(Livro livro) => contexto.Livros.Update(livro);

    public void Remover(Livro livro) => contexto.Livros.Remove(livro);
}
