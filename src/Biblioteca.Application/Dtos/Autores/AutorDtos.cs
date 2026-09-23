using System.ComponentModel.DataAnnotations;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Application.Dtos.Autores;

/// <summary>
/// Dados de entrada para cadastrar um autor.
/// <para>
/// As anotações barram a requisição malformada antes de ela chegar ao caso de uso,
/// devolvendo 400 com o campo exato que falhou. Os limites vêm das constantes do próprio
/// objeto de valor, então anotação e domínio não têm como divergir com o tempo — se
/// <see cref="NomeDeAutor.TamanhoMaximo"/> mudar, as duas mudam juntas.
/// </para>
/// </summary>
public sealed record CriarAutorRequest(
    [Required(ErrorMessage = "O nome do autor é obrigatório.")]
    [StringLength(
        NomeDeAutor.TamanhoMaximo,
        MinimumLength = NomeDeAutor.TamanhoMinimo,
        ErrorMessage = "O nome do autor deve ter entre {2} e {1} caracteres.")]
    string Nome);

/// <summary>Dados de entrada para alterar um autor existente.</summary>
public sealed record AtualizarAutorRequest(
    [Required(ErrorMessage = "O nome do autor é obrigatório.")]
    [StringLength(
        NomeDeAutor.TamanhoMaximo,
        MinimumLength = NomeDeAutor.TamanhoMinimo,
        ErrorMessage = "O nome do autor deve ter entre {2} e {1} caracteres.")]
    string Nome);

/// <summary>
/// Representação de um autor devolvida pela API.
/// <para>
/// A entidade nunca cruza a fronteira da aplicação: o DTO isola o contrato público
/// do modelo interno, para que uma mudança no domínio não quebre quem consome a API.
/// </para>
/// </summary>
public sealed record AutorResponse(
    Guid Id,
    string Nome,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm);
