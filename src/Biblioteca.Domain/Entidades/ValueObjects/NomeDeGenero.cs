using Biblioteca.Domain.Common;
using Biblioteca.Domain.Erros;

namespace Biblioteca.Domain.Entidades.ValueObjects;

/// <inheritdoc cref="NomeDeAutor"/>
public sealed class NomeDeGenero : ValueObject
{
    public const int TamanhoMinimo = 2;
    public const int TamanhoMaximo = 100;

    private NomeDeGenero(string valor) => Valor = valor;

    public string Valor { get; }

    public static Result<NomeDeGenero> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return Result.Failure<NomeDeGenero>(ErrosDeGenero.NomeObrigatorio);
        }

        var normalizado = valor.Trim();

        return normalizado.Length is < TamanhoMinimo or > TamanhoMaximo
            ? Result.Failure<NomeDeGenero>(ErrosDeGenero.NomeForaDoTamanhoPermitido)
            : Result.Success(new NomeDeGenero(normalizado));
    }

    /// <inheritdoc cref="NomeDeAutor.Reconstituir"/>
    internal static NomeDeGenero Reconstituir(string valor) => new(valor);

    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}
