namespace Biblioteca.Domain.Repositories;

/// <summary>
/// Confirma, em uma única transação, todas as alterações acumuladas pelos repositórios.
/// <para>
/// Separar a gravação dos repositórios evita o problema clássico de um caso de uso
/// que altera duas entidades e grava só a primeira antes de falhar.
/// </para>
/// </summary>
public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
