using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Infrastructure.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="Autor"/>.
/// <para>
/// Fica aqui, e não em atributos na entidade, para que o domínio não carregue
/// anotações de persistência.
/// </para>
/// </summary>
internal sealed class AutorConfiguration : ConfiguracaoDeEntidade<Autor>
{
    protected override void ConfigurarEntidade(EntityTypeBuilder<Autor> builder)
    {
        builder.ToTable("autores");

        // O objeto de valor vira uma coluna de texto comum: na ida usa-se o conteúdo,
        // na volta ele é reconstituído sem revalidar.
        builder.Property(autor => autor.Nome)
            .HasConversion(
                nome => nome.Valor,
                valor => NomeDeAutor.Reconstituir(valor))
            .HasColumnName("nome")
            .HasMaxLength(NomeDeAutor.TamanhoMaximo)
            .IsRequired();

        // Sem índice único: homônimos são permitidos.
        builder.HasIndex(autor => autor.Nome)
            .HasDatabaseName("ix_autores_nome");
    }
}
