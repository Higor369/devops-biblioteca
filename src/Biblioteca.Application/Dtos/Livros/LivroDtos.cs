using System.ComponentModel.DataAnnotations;
using Biblioteca.Application.Dtos.Autores;
using Biblioteca.Application.Dtos.Generos;
using Biblioteca.Application.Dtos.Validacao;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Application.Dtos.Livros;

/// <summary>
/// Dados de entrada para cadastrar um livro.
/// <para>
/// <see cref="AutorId"/> e <see cref="GeneroId"/> são obrigatórios: nenhum livro
/// entra no acervo sem autor e gênero existentes. A anotação garante que os ids foram
/// informados; que eles correspondam a registros reais é verificado no serviço de domínio,
/// porque isso exige consultar o banco.
/// </para>
/// <para>
/// O limite superior do ano fica em 9999 e não no ano corrente: anotação só aceita
/// constante de compilação. A regra "não pode ser no futuro" continua sendo do domínio,
/// que conhece o relógio injetado.
/// </para>
/// </summary>
public sealed record CriarLivroRequest(
    [Required(ErrorMessage = "O título do livro é obrigatório.")]
    [StringLength(
        TituloDeLivro.TamanhoMaximo,
        MinimumLength = TituloDeLivro.TamanhoMinimo,
        ErrorMessage = "O título do livro deve ter entre {2} e {1} caracteres.")]
    string Titulo,

    [Range(
        AnoDePublicacao.AnoMinimo,
        9999,
        ErrorMessage = "O ano de publicação não pode ser anterior a {1}.")]
    int AnoDePublicacao,

    [GuidNaoVazio(ErrorMessage = "O livro deve referenciar um autor.")]
    Guid AutorId,

    [GuidNaoVazio(ErrorMessage = "O livro deve referenciar um gênero.")]
    Guid GeneroId);

/// <summary>Dados de entrada para alterar um livro existente.</summary>
public sealed record AtualizarLivroRequest(
    [Required(ErrorMessage = "O título do livro é obrigatório.")]
    [StringLength(
        TituloDeLivro.TamanhoMaximo,
        MinimumLength = TituloDeLivro.TamanhoMinimo,
        ErrorMessage = "O título do livro deve ter entre {2} e {1} caracteres.")]
    string Titulo,

    [Range(
        AnoDePublicacao.AnoMinimo,
        9999,
        ErrorMessage = "O ano de publicação não pode ser anterior a {1}.")]
    int AnoDePublicacao,

    [GuidNaoVazio(ErrorMessage = "O livro deve referenciar um autor.")]
    Guid AutorId,

    [GuidNaoVazio(ErrorMessage = "O livro deve referenciar um gênero.")]
    Guid GeneroId);

/// <summary>
/// Representação de um livro devolvida pela API, já com autor e gênero expandidos —
/// evita que o cliente precise de três chamadas para montar uma listagem legível.
/// </summary>
public sealed record LivroResponse(
    Guid Id,
    string Titulo,
    int AnoDePublicacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm,
    AutorResponse Autor,
    GeneroResponse Genero);
