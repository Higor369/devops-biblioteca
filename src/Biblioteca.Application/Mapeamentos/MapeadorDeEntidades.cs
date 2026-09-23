using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Dtos.Generos;
using Biblioteca.Application.Dtos.Livros;
using Biblioteca.Domain.Entidades;
using Biblioteca.Domain.Entidades.ValueObjects;
using Riok.Mapperly.Abstractions;

namespace Biblioteca.Application.Mapeamentos;

/// <summary>
/// Converte entidades em DTOs de saída.
/// <para>
/// O Mapperly é um gerador de código: os corpos dos métodos <c>partial</c> abaixo são
/// escritos em tempo de compilação. Não há reflection em execução, e um campo que deixe
/// de existir vira erro de compilação em vez de exceção em produção.
/// </para>
/// <para>
/// O caminho inverso (requisição para entidade) não passa por aqui de propósito: entidade
/// só nasce pelo método de fábrica que valida e devolve <c>Result</c>. Um mapeador que
/// preenchesse as propriedades diretamente furaria exatamente a garantia que o domínio
/// existe para dar.
/// </para>
/// </summary>
[Mapper]
internal static partial class MapeadorDeEntidades
{
    public static partial AutorResponse ParaResponse(this Autor autor);

    public static partial GeneroResponse ParaResponse(this Genero genero);

    public static partial IReadOnlyList<AutorResponse> ParaResponse(this IReadOnlyList<Autor> autores);

    public static partial IReadOnlyList<GeneroResponse> ParaResponse(this IReadOnlyList<Genero> generos);

    /// <remarks>
    /// Recebe três fontes: o livro guarda apenas os ids de autor e gênero, então as
    /// entidades relacionadas entram como parâmetro e o Mapperly reaproveita os
    /// mapeamentos declarados acima para expandi-las.
    /// <para>
    /// Os dois <c>MapperIgnoreSource</c> são deliberados: <c>AutorId</c> e <c>GeneroId</c>
    /// não vão para a resposta porque os objetos completos já vão. Sem declarar isso o
    /// Mapperly recusa a compilação — ele não deixa campo de origem sumir em silêncio.
    /// </para>
    /// </remarks>
    [MapperIgnoreSource(nameof(Livro.AutorId))]
    [MapperIgnoreSource(nameof(Livro.GeneroId))]
    public static partial LivroResponse ParaResponse(
        this Livro livro,
        Autor autor,
        Genero genero);

    // Conversões dos objetos de valor. O Mapperly localiza estes métodos sozinho e os
    // aplica onde o tipo de origem e o de destino baterem.
    private static string ParaTexto(NomeDeAutor nome) => nome.Valor;

    private static string ParaTexto(NomeDeGenero nome) => nome.Valor;

    private static string ParaTexto(TituloDeLivro titulo) => titulo.Valor;

    private static int ParaNumero(AnoDePublicacao ano) => ano.Valor;
}
