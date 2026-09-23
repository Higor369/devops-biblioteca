using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Erros;

/// <summary>Catálogo das falhas de negócio relacionadas a gênero.</summary>
public static class ErrosDeGenero
{
    public static readonly Error NomeObrigatorio =
        Error.Validation("genero.nome_obrigatorio", "O nome do gênero é obrigatório.");

    public static readonly Error NomeForaDoTamanhoPermitido =
        Error.Validation(
            "genero.nome_tamanho_invalido",
            $"O nome do gênero deve ter entre {NomeDeGenero.TamanhoMinimo} e {NomeDeGenero.TamanhoMaximo} caracteres.");

    public static Error NaoEncontrado(Guid id) =>
        Error.NotFound("genero.nao_encontrado", $"Não existe gênero com o id {id}.");

    public static Error NomeJaCadastrado(string nome) =>
        Error.Conflict("genero.nome_ja_cadastrado", $"Já existe um gênero cadastrado com o nome '{nome}'.");

    public static Error PossuiLivrosVinculados(int quantidadeDeLivros) =>
        Error.Conflict(
            "genero.possui_livros_vinculados",
            $"Não é possível excluir o gênero: existem {quantidadeDeLivros} livro(s) vinculado(s) a ele.");
}
