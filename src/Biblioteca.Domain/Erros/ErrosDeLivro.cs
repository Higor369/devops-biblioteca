using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Erros;

/// <summary>Catálogo das falhas de negócio relacionadas a livro.</summary>
public static class ErrosDeLivro
{
    public static readonly Error TituloObrigatorio =
        Error.Validation("livro.titulo_obrigatorio", "O título do livro é obrigatório.");

    public static readonly Error TituloForaDoTamanhoPermitido =
        Error.Validation(
            "livro.titulo_tamanho_invalido",
            $"O título do livro deve ter entre {TituloDeLivro.TamanhoMinimo} e {TituloDeLivro.TamanhoMaximo} caracteres.");

    public static readonly Error AutorObrigatorio =
        Error.Validation("livro.autor_obrigatorio", "O livro deve referenciar um autor.");

    public static readonly Error GeneroObrigatorio =
        Error.Validation("livro.genero_obrigatorio", "O livro deve referenciar um gênero.");

    public static Error AnoDePublicacaoInvalido(int anoLimite) =>
        Error.Validation(
            "livro.ano_publicacao_invalido",
            $"O ano de publicação deve estar entre {AnoDePublicacao.AnoMinimo} e {anoLimite}.");

    public static Error NaoEncontrado(Guid id) =>
        Error.NotFound("livro.nao_encontrado", $"Não existe livro com o id {id}.");
}
