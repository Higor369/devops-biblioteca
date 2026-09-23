using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Biblioteca.Infrastructure.Persistencia;

/// <summary>
/// Acervo de exemplo gravado quando o banco nasce, para a API já subir com algo a listar.
/// <para>
/// Roda dentro do <c>MigrateAsync</c>, pelo <c>UseAsyncSeeding</c> do EF Core: fica na
/// mesma transação e sob o mesmo lock das migrations. Se a gravação falhar, o schema
/// também volta atrás, e duas instâncias subindo juntas não gravam o acervo duas vezes.
/// </para>
/// <para>
/// Os registros nascem pelos métodos de fábrica do domínio, e não por SQL ou
/// <c>HasData</c>: um dado de exemplo que violasse uma regra de negócio falha aqui, em vez
/// de entrar no banco por uma porta que a API não oferece.
/// </para>
/// </summary>
internal static class DadosDeExemplo
{
    private static readonly (string Titulo, int Ano, string Autor, string Genero)[] _livros =
    [
        ("Dom Casmurro", 1899, "Machado de Assis", "Romance"),
        ("Memórias Póstumas de Brás Cubas", 1881, "Machado de Assis", "Romance"),
        ("Papéis Avulsos", 1882, "Machado de Assis", "Conto"),
        ("A Hora da Estrela", 1977, "Clarice Lispector", "Romance"),
        ("Laços de Família", 1960, "Clarice Lispector", "Conto"),
        ("Vidas Secas", 1938, "Graciliano Ramos", "Romance"),
        ("Capitães da Areia", 1937, "Jorge Amado", "Romance"),
        ("O Hobbit", 1937, "J. R. R. Tolkien", "Fantasia"),
        ("1984", 1949, "George Orwell", "Distopia"),
        ("Admirável Mundo Novo", 1932, "Aldous Huxley", "Distopia"),
    ];

    public static async Task PopularAsync(
        DbContext contexto,
        TimeProvider relogio,
        CancellationToken cancellationToken)
    {
        // O EF Core chama o seeding em todo MigrateAsync, não só no primeiro. Qualquer
        // registro já gravado significa que o banco não é novo, e dado de exemplo não se
        // mistura com dado real. Livro exige autor e gênero, então basta olhar esses dois.
        if (await contexto.Set<Autor>().AnyAsync(cancellationToken)
            || await contexto.Set<Genero>().AnyAsync(cancellationToken))
        {
            return;
        }

        // O Npgsql carregou o catálogo de tipos ao abrir a conexão, antes de a migration
        // instalar a extensão citext, e só o recarrega depois que o seeding termina. Sem
        // isto, gravar o nome do gênero (coluna citext) falha num banco recém-criado.
        // A recarga roda na conexão da migration, que já enxerga a extensão criada na
        // transação ainda aberta.
        await ((NpgsqlConnection)contexto.Database.GetDbConnection())
            .ReloadTypesAsync(cancellationToken);

        var autores = _livros
            .Select(livro => livro.Autor)
            .Distinct()
            .ToDictionary(nome => nome, nome => Validado(Autor.Criar(nome)));

        var generos = _livros
            .Select(livro => livro.Genero)
            .Distinct()
            .ToDictionary(nome => nome, nome => Validado(Genero.Criar(nome)));

        var livros = _livros.Select(livro => Validado(Livro.Criar(
            livro.Titulo,
            livro.Ano,
            autores[livro.Autor].Id,
            generos[livro.Genero].Id,
            relogio)));

        contexto.AddRange(autores.Values);
        contexto.AddRange(generos.Values);
        contexto.AddRange(livros);

        await contexto.SaveChangesAsync(cancellationToken);
    }

    private static T Validado<T>(Result<T> resultado) =>
        resultado.IsSuccess
            ? resultado.Value
            : throw new InvalidOperationException(
                $"Dado de exemplo inválido ({resultado.Error.Code}): {resultado.Error.Message}");
}
