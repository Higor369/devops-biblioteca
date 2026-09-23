using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;
using Biblioteca.Domain.Erros;

namespace Biblioteca.Domain.Entidades;

/// <summary>
/// Livro do acervo.
/// <para>
/// Não há restrição de unicidade: dois livros com o mesmo título, autor e gênero são
/// registros distintos, diferenciados apenas pelo <c>Id</c> herdado de <see cref="Entity"/>.
/// Isso é intencional — um acervo pode ter vários exemplares da mesma obra, e é
/// justamente o tipo de caso em que a identidade importa mais que o conteúdo.
/// </para>
/// <para>
/// Todo livro nasce apontando para um autor e um gênero. A entidade garante que os ids
/// foram informados; se eles de fato existem no acervo é responsabilidade do serviço de
/// domínio, que é quem enxerga os outros agregados.
/// </para>
/// </summary>
public sealed class Livro : Entity
{
    /// <summary>Exigido pelo EF Core para materializar a entidade vinda do banco.</summary>
    private Livro()
    {
    }

    private Livro(Guid id, TituloDeLivro titulo, AnoDePublicacao ano, Guid autorId, Guid generoId)
        : base(id)
    {
        Titulo = titulo;
        AnoDePublicacao = ano;
        AutorId = autorId;
        GeneroId = generoId;
    }

    public TituloDeLivro Titulo { get; private set; } = null!;

    public AnoDePublicacao AnoDePublicacao { get; private set; } = null!;

    public Guid AutorId { get; private set; }

    public Guid GeneroId { get; private set; }

    /// <param name="relogio">
    /// O ano atual entra por injeção em vez de <c>DateTime.Now</c>: o domínio não deve
    /// depender do relógio da máquina, senão o teste de "ano no futuro" passa a depender
    /// de quando ele roda.
    /// </param>
    public static Result<Livro> Criar(
        string titulo,
        int anoDePublicacao,
        Guid autorId,
        Guid generoId,
        TimeProvider relogio)
    {
        var campos = Validar(titulo, anoDePublicacao, autorId, generoId, relogio);

        return campos.IsFailure
            ? Result.Failure<Livro>(campos.Error)
            : Result.Success(new Livro(
                Guid.CreateVersion7(),
                campos.Value.Titulo,
                campos.Value.Ano,
                autorId,
                generoId));
    }

    public Result Atualizar(
        string titulo,
        int anoDePublicacao,
        Guid autorId,
        Guid generoId,
        TimeProvider relogio)
    {
        var campos = Validar(titulo, anoDePublicacao, autorId, generoId, relogio);
        if (campos.IsFailure)
        {
            return Result.Failure(campos.Error);
        }

        Titulo = campos.Value.Titulo;
        AnoDePublicacao = campos.Value.Ano;
        AutorId = autorId;
        GeneroId = generoId;

        return Result.Success();
    }

    /// <summary>
    /// Campos já convertidos em objetos de valor. O tipo existe para que a validação
    /// aconteça num método estático, compartilhado por <see cref="Criar"/> e
    /// <see cref="Atualizar"/> sem duplicação.
    /// </summary>
    private sealed record CamposValidados(TituloDeLivro Titulo, AnoDePublicacao Ano);

    private static Result<CamposValidados> Validar(
        string titulo,
        int anoDePublicacao,
        Guid autorId,
        Guid generoId,
        TimeProvider relogio)
    {
        var tituloValidado = TituloDeLivro.Criar(titulo);
        if (tituloValidado.IsFailure)
        {
            return Result.Failure<CamposValidados>(tituloValidado.Error);
        }

        if (autorId == Guid.Empty)
        {
            return Result.Failure<CamposValidados>(ErrosDeLivro.AutorObrigatorio);
        }

        if (generoId == Guid.Empty)
        {
            return Result.Failure<CamposValidados>(ErrosDeLivro.GeneroObrigatorio);
        }

        var anoValidado = AnoDePublicacao.Criar(anoDePublicacao, relogio);

        return anoValidado.IsFailure
            ? Result.Failure<CamposValidados>(anoValidado.Error)
            : Result.Success(new CamposValidados(tituloValidado.Value, anoValidado.Value));
    }
}
