using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Infrastructure.Persistencia.Configuracoes;

/// <inheritdoc cref="AutorConfiguration"/>
internal sealed class LivroConfiguration : ConfiguracaoDeEntidade<Livro>
{
    protected override void ConfigurarEntidade(EntityTypeBuilder<Livro> builder)
    {
        builder.ToTable("livros");

        builder.Property(livro => livro.Titulo)
            .HasConversion(
                titulo => titulo.Valor,
                valor => TituloDeLivro.Reconstituir(valor))
            .HasColumnName("titulo")
            .HasMaxLength(TituloDeLivro.TamanhoMaximo)
            .IsRequired();

        builder.Property(livro => livro.AnoDePublicacao)
            .HasConversion(
                ano => ano.Valor,
                valor => AnoDePublicacao.Reconstituir(valor))
            .HasColumnName("ano_publicacao")
            .IsRequired();

        builder.Property(livro => livro.AutorId)
            .HasColumnName("autor_id")
            .IsRequired();

        builder.Property(livro => livro.GeneroId)
            .HasColumnName("genero_id")
            .IsRequired();

        // HasOne<T>() sem lambda: a chave estrangeira existe no banco, mas o Livro nao
        // carrega propriedade de navegacao. O vinculo entre agregados e o id, e so ele.
        // Restrict espelha no banco a mesma regra que o servico de dominio aplica:
        // apagar autor ou genero com livros vinculados e proibido.
        builder.HasOne<Autor>()
            .WithMany()
            .HasForeignKey(livro => livro.AutorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Genero>()
            .WithMany()
            .HasForeignKey(livro => livro.GeneroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(livro => livro.AutorId).HasDatabaseName("ix_livros_autor_id");
        builder.HasIndex(livro => livro.GeneroId).HasDatabaseName("ix_livros_genero_id");

        // Nenhum indice unico sobre titulo/autor/genero: livros repetidos sao permitidos.
    }
}
