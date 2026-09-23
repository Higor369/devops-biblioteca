using Biblioteca.Domain.Repositories;

namespace Biblioteca.Infrastructure.Persistencia;

/// <summary>
/// Confirma no banco tudo o que os repositórios acumularam no rastreador do EF Core.
/// <para>
/// Como todos os repositórios recebem a mesma instância de <see cref="BibliotecaDbContext"/>
/// (registrada com tempo de vida <c>Scoped</c>), uma única chamada grava as alterações
/// de todos eles dentro da mesma transação.
/// </para>
/// </summary>
internal sealed class UnitOfWork(BibliotecaDbContext contexto) : IUnitOfWork
{
    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default) =>
        contexto.SaveChangesAsync(cancellationToken);
}
