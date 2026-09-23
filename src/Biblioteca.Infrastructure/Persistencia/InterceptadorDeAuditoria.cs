using Biblioteca.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Biblioteca.Infrastructure.Persistencia;

/// <summary>
/// Carimba <c>CriadoEm</c> e <c>AtualizadoEm</c> em toda entidade, no momento de gravar.
/// <para>
/// Data de auditoria é rastro de persistência, não regra de negócio — por isso mora aqui
/// e não no domínio, que assim continua sem depender do relógio da máquina.
/// </para>
/// <para>
/// O ganho prático é não haver como esquecer: qualquer entidade nova, ou qualquer método
/// de alteração criado no futuro, já nasce com as datas corretas sem precisar lembrar
/// de nada. A escrita passa pelo rastreador do EF Core, então os <c>set</c> privados das
/// propriedades continuam fechados para o resto do código.
/// </para>
/// </summary>
internal sealed class InterceptadorDeAuditoria(TimeProvider relogio) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AplicarCarimbos(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AplicarCarimbos(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AplicarCarimbos(DbContext? contexto)
    {
        if (contexto is null)
        {
            return;
        }

        var agora = TruncarParaAPrecisaoDoBanco(relogio.GetUtcNow());

        foreach (var entrada in contexto.ChangeTracker.Entries<Entity>())
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    entrada.Property(nameof(Entity.CriadoEm)).CurrentValue = agora;
                    break;

                case EntityState.Modified:
                    entrada.Property(nameof(Entity.AtualizadoEm)).CurrentValue = agora;
                    // CriadoEm nunca muda depois da inserção.
                    entrada.Property(nameof(Entity.CriadoEm)).IsModified = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Descarta a fração abaixo do microssegundo.
    /// <para>
    /// <see cref="DateTimeOffset"/> guarda até 100 nanossegundos, mas o tipo
    /// <c>timestamptz</c> do PostgreSQL só grava microssegundos. Sem truncar, o valor
    /// devolvido logo após o cadastro seria ligeiramente diferente do mesmo campo lido
    /// depois — a API responderia dois valores distintos para o mesmo registro.
    /// </para>
    /// </summary>
    private static DateTimeOffset TruncarParaAPrecisaoDoBanco(DateTimeOffset instante) =>
        new(instante.Ticks - (instante.Ticks % TimeSpan.TicksPerMicrosecond), instante.Offset);
}
