using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Implementação do contrato declarado no domínio.
/// <para>
/// A classe é <c>internal</c>: nada fora desta camada precisa conhecê-la, pois o
/// registro no contêiner de DI acontece aqui mesmo, contra a interface.
/// </para>
/// </summary>
internal sealed class AutorRepository(BibliotecaDbContext contexto) : IAutorRepository
{
    public async Task<Autor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await contexto.Autores.FirstOrDefaultAsync(autor => autor.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Autor>> ListarAsync(CancellationToken cancellationToken = default) =>
        await contexto.Autores
            // Leitura pura: sem rastreamento o EF não precisa guardar snapshot das entidades.
            .AsNoTracking()
            .OrderBy(autor => autor.Nome)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Autor>> ListarPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default) =>
        await contexto.Autores
            .AsNoTracking()
            // Traduzido pelo Npgsql para "id = ANY(@ids)": uma consulta só,
            // independentemente de quantos ids forem pedidos.
            .Where(autor => ids.Contains(autor.Id))
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Autores.AnyAsync(autor => autor.Id == id, cancellationToken);

    public async Task AdicionarAsync(Autor autor, CancellationToken cancellationToken = default) =>
        await contexto.Autores.AddAsync(autor, cancellationToken);

    public void Atualizar(Autor autor) => contexto.Autores.Update(autor);

    public void Remover(Autor autor) => contexto.Autores.Remove(autor);
}
