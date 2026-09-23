using Biblioteca.Domain.Entidades;

namespace Biblioteca.Domain.Repositories;

/// <summary>
/// Contrato de persistência de autores.
/// <para>
/// A interface vive no domínio e a implementação na infraestrutura: é isso que permite
/// ao domínio ignorar completamente a existência de EF Core e PostgreSQL.
/// </para>
/// <para>
/// Nenhum método aqui persiste de fato — a gravação é confirmada por
/// <see cref="IUnitOfWork.SalvarAlteracoesAsync"/>, para que um caso de uso que toca
/// vários repositórios continue sendo uma transação só.
/// </para>
/// </summary>
public interface IAutorRepository
{
    Task<Autor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Autor>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega vários autores de uma vez.
    /// <para>
    /// Existe para que montar uma listagem de livros custe uma consulta de autores,
    /// e não uma por livro.
    /// </para>
    /// </summary>
    Task<IReadOnlyList<Autor>> ListarPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Autor autor, CancellationToken cancellationToken = default);

    void Atualizar(Autor autor);

    void Remover(Autor autor);
}
