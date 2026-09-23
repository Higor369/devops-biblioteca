using Biblioteca.Domain.Common;
using Biblioteca.Domain.Erros;

namespace Biblioteca.Domain.Entidades.ValueObjects;

/// <summary>
/// Nome de um autor, já validado e normalizado.
/// <para>
/// Tipar o nome em vez de usar <c>string</c> solta traz duas garantias: a validação
/// acontece uma vez só, no ponto de criação, e fica impossível passar um título de livro
/// onde se espera um nome de autor — o compilador recusa.
/// </para>
/// </summary>
public sealed class NomeDeAutor : ValueObject
{
    public const int TamanhoMinimo = 2;
    public const int TamanhoMaximo = 200;

    private NomeDeAutor(string valor) => Valor = valor;

    public string Valor { get; }

    public static Result<NomeDeAutor> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return Result.Failure<NomeDeAutor>(ErrosDeAutor.NomeObrigatorio);
        }

        var normalizado = valor.Trim();

        return normalizado.Length is < TamanhoMinimo or > TamanhoMaximo
            ? Result.Failure<NomeDeAutor>(ErrosDeAutor.NomeForaDoTamanhoPermitido)
            : Result.Success(new NomeDeAutor(normalizado));
    }

    /// <summary>
    /// Reconstrói o objeto a partir de um valor já gravado no banco, sem revalidar.
    /// <para>
    /// É o que o DDD chama de reconstituição: o dado passou pela validação quando entrou,
    /// e revalidar na leitura só criaria o risco de um registro antigo se tornar
    /// irrecuperável ao apertarmos uma regra. Visível apenas para a camada de persistência.
    /// </para>
    /// </summary>
    internal static NomeDeAutor Reconstituir(string valor) => new(valor);

    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}
