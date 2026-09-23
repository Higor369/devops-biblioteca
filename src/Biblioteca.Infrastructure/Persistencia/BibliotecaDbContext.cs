using Biblioteca.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure.Persistencia;

/// <summary>
/// Sessão com o banco. É o único ponto da solução que conhece EF Core além dos
/// repositórios — nenhuma camada acima recebe este tipo por injeção.
/// </summary>
public sealed class BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options)
    : DbContext(options)
{
    public DbSet<Autor> Autores => Set<Autor>();

    public DbSet<Genero> Generos => Set<Genero>();

    public DbSet<Livro> Livros => Set<Livro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // citext = coluna de texto case-insensitive nativa do PostgreSQL.
        // É o que torna o índice único de gênero insensível a maiúsculas no próprio banco,
        // em vez de depender só da checagem feita em memória pela aplicação.
        modelBuilder.HasPostgresExtension("citext");

        // Carrega todas as IEntityTypeConfiguration deste assembly, evitando um
        // OnModelCreating gigante que cresce a cada entidade nova.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BibliotecaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
