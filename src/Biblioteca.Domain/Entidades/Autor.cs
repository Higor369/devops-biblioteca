using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Entidades;

/// <summary>
/// Autor de um ou mais livros.
/// <para>
/// Entidade: tem identidade própria e ciclo de vida independente. Dois autores homônimos
/// são registros distintos, e é por isso que autor não poderia ser um objeto de valor.
/// </para>
/// <para>
/// Não existe construtor público: a única porta de entrada é <see cref="Criar"/>, que valida
/// antes de instanciar. Com isso um <see cref="Autor"/> em memória está sempre válido,
/// e nenhuma camada acima precisa reconferir isso.
/// </para>
/// </summary>
public sealed class Autor : Entity
{
    /// <summary>Exigido pelo EF Core para materializar a entidade vinda do banco.</summary>
    private Autor()
    {
    }

    private Autor(Guid id, NomeDeAutor nome)
        : base(id) => Nome = nome;

    public NomeDeAutor Nome { get; private set; } = null!;

    public static Result<Autor> Criar(string nome)
    {
        var nomeValidado = NomeDeAutor.Criar(nome);

        return nomeValidado.IsFailure
            ? Result.Failure<Autor>(nomeValidado.Error)
            : Result.Success(new Autor(Guid.CreateVersion7(), nomeValidado.Value));
    }

    public Result AlterarNome(string nome)
    {
        var nomeValidado = NomeDeAutor.Criar(nome);
        if (nomeValidado.IsFailure)
        {
            return Result.Failure(nomeValidado.Error);
        }

        Nome = nomeValidado.Value;
        return Result.Success();
    }
}
