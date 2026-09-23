using Biblioteca.Domain.Common;
using Biblioteca.Domain.Erros;

namespace Biblioteca.Domain.Entidades.ValueObjects;

/// <summary>
/// Ano em que o livro foi publicado.
/// <para>
/// Diferente dos demais objetos de valor, a validação depende de um dado externo — o ano
/// corrente — porque um livro não pode ter sido publicado no futuro. Esse dado entra por
/// parâmetro, e não de <c>DateTime.Now</c>, para que a regra não dependa de quando o
/// código roda.
/// </para>
/// </summary>
public sealed class AnoDePublicacao : ValueObject
{
    /// <summary>Ano aproximado da prensa de Gutenberg: antes disso não há livro impresso.</summary>
    public const int AnoMinimo = 1450;

    private AnoDePublicacao(int valor) => Valor = valor;

    public int Valor { get; }

    public static Result<AnoDePublicacao> Criar(int valor, TimeProvider relogio)
    {
        var anoLimite = relogio.GetUtcNow().Year;

        return valor < AnoMinimo || valor > anoLimite
            ? Result.Failure<AnoDePublicacao>(ErrosDeLivro.AnoDePublicacaoInvalido(anoLimite))
            : Result.Success(new AnoDePublicacao(valor));
    }

    /// <inheritdoc cref="NomeDeAutor.Reconstituir"/>
    internal static AnoDePublicacao Reconstituir(int valor) => new(valor);

    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }

    public override string ToString() => Valor.ToString();
}
