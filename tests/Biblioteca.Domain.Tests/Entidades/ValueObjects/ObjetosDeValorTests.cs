using Biblioteca.Domain.Entidades.ValueObjects;
using Biblioteca.Domain.Tests.Suporte;

namespace Biblioteca.Domain.Tests.Entidades.ValueObjects;

/// <summary>
/// Validação e normalização migraram das entidades para os objetos de valor: é aqui que
/// elas passam a ser testadas.
/// </summary>
public sealed class ObjetosDeValorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NomeDeAutor_SemConteudo_DeveFalhar(string? valor)
    {
        var resultado = NomeDeAutor.Criar(valor!);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("autor.nome_obrigatorio");
    }

    [Fact]
    public void NomeDeAutor_DeveDescartarEspacosDasBordas()
    {
        NomeDeAutor.Criar("  Machado de Assis  ").Value.Valor.ShouldBe("Machado de Assis");
    }

    [Fact]
    public void NomeDeAutor_ForaDoTamanhoPermitido_DeveFalhar()
    {
        NomeDeAutor.Criar("M").IsFailure.ShouldBeTrue();
        NomeDeAutor.Criar(new string('a', NomeDeAutor.TamanhoMaximo + 1)).IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// A normalização acontecer dentro do objeto de valor é o que garante que
    /// "  Romance  " e "Romance" sejam o mesmo valor em qualquer ponto do sistema.
    /// </summary>
    [Fact]
    public void NomeDeGenero_ComEspacosNasBordas_DeveSerIgualAoNormalizado()
    {
        var comEspacos = NomeDeGenero.Criar("  Romance  ").Value;
        var semEspacos = NomeDeGenero.Criar("Romance").Value;

        (comEspacos == semEspacos).ShouldBeTrue();
    }

    [Fact]
    public void NomeDeGenero_DiferindoApenasNoCaixa_SaoValoresDistintosNoDominio()
    {
        var minusculo = NomeDeGenero.Criar("romance").Value;
        var maiusculo = NomeDeGenero.Criar("ROMANCE").Value;

        // O domínio compara texto exato. Tratar os dois como o mesmo gênero é decisão
        // do banco, via coluna citext — e é lá que isso é testado.
        (minusculo == maiusculo).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void TituloDeLivro_SemConteudo_DeveFalhar(string? valor)
    {
        var resultado = TituloDeLivro.Criar(valor!);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("livro.titulo_obrigatorio");
    }

    [Fact]
    public void AnoDePublicacao_AnteriorAImprensa_DeveFalhar()
    {
        var resultado = AnoDePublicacao.Criar(AnoDePublicacao.AnoMinimo - 1, new RelogioFixo(2026));

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("livro.ano_publicacao_invalido");
    }

    [Fact]
    public void AnoDePublicacao_NoFuturo_DeveFalhar()
    {
        var relogio = new RelogioFixo(2026);

        AnoDePublicacao.Criar(relogio.Ano + 1, relogio).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void AnoDePublicacao_NoAnoCorrente_DeveSerAceito()
    {
        var relogio = new RelogioFixo(2026);

        AnoDePublicacao.Criar(relogio.Ano, relogio).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A mensagem de erro precisa citar o ano do relógio injetado, e não o ano real da
    /// máquina — é o que prova que a regra não depende de quando o teste roda.
    /// </summary>
    [Fact]
    public void AnoDePublicacao_DeveReportarOLimiteVindoDoRelogioInjetado()
    {
        var resultado = AnoDePublicacao.Criar(3000, new RelogioFixo(1999));

        resultado.Error.Message.ShouldContain("1999");
    }
}
