using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Domain.Erros;

/// <summary>
/// Catálogo das falhas de negócio relacionadas a autor.
/// Concentrar os erros aqui evita mensagens duplicadas e divergentes espalhadas pelo código.
/// </summary>
public static class ErrosDeAutor
{
    public static readonly Error NomeObrigatorio =
        Error.Validation("autor.nome_obrigatorio", "O nome do autor é obrigatório.");

    public static readonly Error NomeForaDoTamanhoPermitido =
        Error.Validation(
            "autor.nome_tamanho_invalido",
            $"O nome do autor deve ter entre {NomeDeAutor.TamanhoMinimo} e {NomeDeAutor.TamanhoMaximo} caracteres.");

    public static Error NaoEncontrado(Guid id) =>
        Error.NotFound("autor.nao_encontrado", $"Não existe autor com o id {id}.");

    public static Error PossuiLivrosVinculados(int quantidadeDeLivros) =>
        Error.Conflict(
            "autor.possui_livros_vinculados",
            $"Não é possível excluir o autor: existem {quantidadeDeLivros} livro(s) vinculado(s) a ele.");
}
