using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;
using Biblioteca.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure.Persistencia.Repositorios;

/// <inheritdoc cref="AutorRepository"/>
internal sealed class GeneroRepository(BibliotecaDbContext contexto) : IGeneroRepository
{
    public async Task<Genero?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await contexto.Generos.FirstOrDefaultAsync(genero => genero.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Genero>> ListarAsync(CancellationToken cancellationToken = default) =>
        await contexto.Generos
            .AsNoTracking()
            .OrderBy(genero => genero.Nome)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Genero>> ListarPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default) =>
        await contexto.Generos
            .AsNoTracking()
            .Where(genero => ids.Contains(genero.Id))
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Generos.AnyAsync(genero => genero.Id == id, cancellationToken);

    public Task<bool> ExisteComNomeAsync(
        NomeDeGenero nome,
        Guid? idIgnorado = null,
        CancellationToken cancellationToken = default) =>
        contexto.Generos.AnyAsync(
            // A coluna é citext, então esta igualdade já ignora maiúsculas/minúsculas
            // no próprio PostgreSQL — sem LOWER() de ambos os lados e sem perder o índice.
            genero => genero.Nome == nome && (idIgnorado == null || genero.Id != idIgnorado),
            cancellationToken);

    public async Task AdicionarAsync(Genero genero, CancellationToken cancellationToken = default) =>
        await contexto.Generos.AddAsync(genero, cancellationToken);

    public void Atualizar(Genero genero) => contexto.Generos.Update(genero);

    public void Remover(Genero genero) => contexto.Generos.Remove(genero);
}
