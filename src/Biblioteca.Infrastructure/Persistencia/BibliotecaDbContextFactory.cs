using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Biblioteca.Infrastructure.Persistencia;

/// <summary>
/// Usada apenas pelas ferramentas de linha de comando do EF Core ao gerar ou aplicar
/// migrations.
/// <para>
/// Sem ela, o <c>dotnet ef</c> precisaria subir o host completo da API só para descobrir
/// como construir o contexto — o que amarraria a geração de migrations à configuração
/// de execução da aplicação. Em tempo de execução esta classe nunca é usada.
/// </para>
/// </summary>
internal sealed class BibliotecaDbContextFactory : IDesignTimeDbContextFactory<BibliotecaDbContext>
{
    private const string ConexaoPadraoDeDesenvolvimento =
        "Host=localhost;Port=5432;Database=biblioteca;Username=biblioteca;Password=biblioteca";

    public BibliotecaDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Biblioteca")
            ?? ConexaoPadraoDeDesenvolvimento;

        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BibliotecaDbContext(options);
    }
}
