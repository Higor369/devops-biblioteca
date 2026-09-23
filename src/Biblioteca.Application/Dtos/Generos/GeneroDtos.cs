using System.ComponentModel.DataAnnotations;
using Biblioteca.Domain.Entidades.ValueObjects;

namespace Biblioteca.Application.Dtos.Generos;

/// <inheritdoc cref="Biblioteca.Application.Dtos.Autores.CriarAutorRequest"/>
public sealed record CriarGeneroRequest(
    [Required(ErrorMessage = "O nome do gênero é obrigatório.")]
    [StringLength(
        NomeDeGenero.TamanhoMaximo,
        MinimumLength = NomeDeGenero.TamanhoMinimo,
        ErrorMessage = "O nome do gênero deve ter entre {2} e {1} caracteres.")]
    string Nome);

/// <summary>Dados de entrada para alterar um gênero existente.</summary>
public sealed record AtualizarGeneroRequest(
    [Required(ErrorMessage = "O nome do gênero é obrigatório.")]
    [StringLength(
        NomeDeGenero.TamanhoMaximo,
        MinimumLength = NomeDeGenero.TamanhoMinimo,
        ErrorMessage = "O nome do gênero deve ter entre {2} e {1} caracteres.")]
    string Nome);

/// <inheritdoc cref="Biblioteca.Application.Dtos.Autores.AutorResponse"/>
public sealed record GeneroResponse(
    Guid Id,
    string Nome,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm);
