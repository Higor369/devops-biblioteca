using Biblioteca.Domain.Common;
using Biblioteca.Domain.Erros;

namespace Biblioteca.Domain.Entidades.ValueObjects;

/// <inheritdoc cref="NomeDeAutor"/>
public sealed class TituloDeLivro : ValueObject
{
    public const int TamanhoMinimo = 2;
    public const int TamanhoMaximo = 300;

    private TituloDeLivro(string valor) => Valor = valor;

    public string Valor { get; }

    public static Result<TituloDeLivro> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return Result.Failure<TituloDeLivro>(ErrosDeLivro.TituloObrigatorio);
        }

        var normalizado = valor.Trim();

        return normalizado.Length is < TamanhoMinimo or > TamanhoMaximo
            ? Result.Failure<TituloDeLivro>(ErrosDeLivro.TituloForaDoTamanhoPermitido)
            : Result.Success(new TituloDeLivro(normalizado));
    }

    /// <inheritdoc cref="NomeDeAutor.Reconstituir"/>
    internal static TituloDeLivro Reconstituir(string valor) => new(valor);

    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}
