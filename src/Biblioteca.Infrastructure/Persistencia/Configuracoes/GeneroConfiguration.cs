using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Infrastructure.Persistencia.Configuracoes;

/// <inheritdoc cref="AutorConfiguration"/>
internal sealed class GeneroConfiguration : ConfiguracaoDeEntidade<Genero>
{
    protected override void ConfigurarEntidade(EntityTypeBuilder<Genero> builder)
    {
        builder.ToTable("generos");

        builder.Property(genero => genero.Nome)
            .HasConversion(
                nome => nome.Valor,
                valor => NomeDeGenero.Reconstituir(valor))
            .HasColumnName("nome")
            // citext: comparação e índice ignoram maiúsculas/minúsculas.
            .HasColumnType("citext")
            .HasMaxLength(NomeDeGenero.TamanhoMaximo)
            .IsRequired();

        // Última linha de defesa da unicidade. A aplicação já checa antes para devolver
        // um 409 com mensagem legível; este índice cobre a corrida entre duas requisições
        // simultâneas, que a checagem em memória não consegue cobrir.
        builder.HasIndex(genero => genero.Nome)
            .IsUnique()
            .HasDatabaseName("ux_generos_nome");
    }
}
