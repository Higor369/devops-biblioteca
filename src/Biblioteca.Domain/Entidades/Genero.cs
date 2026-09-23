using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Entidades;

/// <summary>
/// Gênero literário.
/// <para>
/// É entidade, e não objeto de valor, porque tem ciclo de vida próprio: pode ser criado,
/// renomeado e excluído por conta própria, e outros registros o referenciam pelo id.
/// Quem é objeto de valor aqui é o <see cref="NomeDeGenero"/> que ele carrega.
/// </para>
/// <para>
/// Diferente de autor e livro, o nome é único no acervo — permitir "Ficção" duas vezes
/// tornaria a classificação inútil. A unicidade em si não é verificada aqui: a entidade
/// não enxerga as demais. Quem cuida disso é o serviço de domínio.
/// </para>
/// </summary>
public sealed class Genero : Entity
{
    /// <summary>Exigido pelo EF Core para materializar a entidade vinda do banco.</summary>
    private Genero()
    {
    }

    private Genero(Guid id, NomeDeGenero nome)
        : base(id) => Nome = nome;

    public NomeDeGenero Nome { get; private set; } = null!;

    public static Result<Genero> Criar(string nome)
    {
        var nomeValidado = NomeDeGenero.Criar(nome);

        return nomeValidado.IsFailure
            ? Result.Failure<Genero>(nomeValidado.Error)
            : Result.Success(new Genero(Guid.CreateVersion7(), nomeValidado.Value));
    }

    public Result AlterarNome(string nome)
    {
        var nomeValidado = NomeDeGenero.Criar(nome);
        if (nomeValidado.IsFailure)
        {
            return Result.Failure(nomeValidado.Error);
        }

        Nome = nomeValidado.Value;
        return Result.Success();
    }
}
