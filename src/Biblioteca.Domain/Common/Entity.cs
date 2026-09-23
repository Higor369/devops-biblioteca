namespace Biblioteca.Domain.Common;

/// <summary>
/// Raiz de toda entidade do domínio.
/// <para>
/// O que define uma entidade em DDD é a <b>identidade</b>, não os atributos: dois livros
/// com o mesmo título, autor e ano continuam sendo livros diferentes se os
/// <see cref="Id"/> diferem. Por isso a igualdade aqui é comparada pelo id, e não
/// campo a campo — o oposto de <see cref="ValueObject"/>.
/// </para>
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>Exigido pelo EF Core para materializar a entidade vinda do banco.</summary>
    protected Entity()
    {
    }

    protected Entity(Guid id) => Id = id;

    public Guid Id { get; private set; }

    /// <summary>
    /// Quando o registro entrou no banco.
    /// <para>
    /// Preenchida pelo interceptador de auditoria na infraestrutura, não pelo domínio.
    /// Data de auditoria é rastro de persistência, não regra de negócio — e delegar isso
    /// ao interceptador torna impossível alguém esquecer de preencher.
    /// </para>
    /// </summary>
    public DateTimeOffset CriadoEm { get; private set; }

    /// <summary>
    /// Quando o registro foi alterado pela última vez. Nula enquanto nunca tiver sido alterado.
    /// </summary>
    /// <inheritdoc cref="CriadoEm"/>
    public DateTimeOffset? AtualizadoEm { get; private set; }

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // Entidades de tipos diferentes nunca são iguais, mesmo compartilhando o id.
        if (GetType() != other.GetType())
        {
            return false;
        }

        // Id vazio significa entidade ainda não identificada: não é igual nem a si mesma
        // por valor, apenas por referência (já tratado acima).
        return Id != Guid.Empty && Id == other.Id;
    }

    public override bool Equals(object? obj) => obj is Entity outra && Equals(outra);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? esquerda, Entity? direita) =>
        esquerda is null ? direita is null : esquerda.Equals(direita);

    public static bool operator !=(Entity? esquerda, Entity? direita) => !(esquerda == direita);
}
