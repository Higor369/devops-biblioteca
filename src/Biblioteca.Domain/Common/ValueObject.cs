namespace Biblioteca.Domain.Common;

/// <summary>
/// Raiz de todo objeto de valor do domínio.
/// <para>
/// Um objeto de valor não tem identidade própria: ele <b>é</b> o seu conteúdo. Dois nomes
/// de autor com o mesmo texto são o mesmo nome, do jeito que duas notas de dez reais valem
/// a mesma coisa. Daí a igualdade ser comparada componente a componente, e não por id
/// como em <see cref="Entity"/>.
/// </para>
/// <para>
/// A outra característica que o DDD pede é <b>imutabilidade</b>: um objeto de valor nunca
/// muda depois de criado. Quem quiser outro valor cria outra instância. Por isso as
/// propriedades das classes derivadas são somente leitura e não existem métodos de alteração.
/// </para>
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Devolve, em ordem, os valores que definem este objeto. É a única coisa que a
    /// classe derivada precisa informar — igualdade, hash e operadores saem daqui.
    /// </summary>
    protected abstract IEnumerable<object?> ObterComponentesDeIgualdade();

    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType()
            && ObterComponentesDeIgualdade().SequenceEqual(other.ObterComponentesDeIgualdade());
    }

    public override bool Equals(object? obj) => obj is ValueObject outro && Equals(outro);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GetType());

        foreach (var componente in ObterComponentesDeIgualdade())
        {
            hash.Add(componente);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? esquerda, ValueObject? direita) =>
        esquerda is null ? direita is null : esquerda.Equals(direita);

    public static bool operator !=(ValueObject? esquerda, ValueObject? direita) => !(esquerda == direita);
}
