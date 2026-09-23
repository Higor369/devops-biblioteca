using Biblioteca.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Infrastructure.Persistencia.Configuracoes;

/// <summary>
/// Configuração comum a toda entidade: chave primária e colunas de auditoria.
/// <para>
/// Centralizar aqui garante que nenhuma entidade nova esqueça <c>criado_em</c> ou
/// <c>atualizado_em</c> — o compilador obriga a derivar desta classe, e o mapeamento
/// vem junto.
/// </para>
/// </summary>
internal abstract class ConfiguracaoDeEntidade<TEntidade> : IEntityTypeConfiguration<TEntidade>
    where TEntidade : Entity
{
    public void Configure(EntityTypeBuilder<TEntidade> builder)
    {
        builder.HasKey(entidade => entidade.Id);

        builder.Property(entidade => entidade.Id)
            .HasColumnName("id")
            // O id nasce no domínio (Guid v7), não no banco.
            .ValueGeneratedNever();

        builder.Property(entidade => entidade.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(entidade => entidade.AtualizadoEm)
            .HasColumnName("atualizado_em");

        ConfigurarEntidade(builder);
    }

    /// <summary>O que é específico de cada entidade.</summary>
    protected abstract void ConfigurarEntidade(EntityTypeBuilder<TEntidade> builder);
}
