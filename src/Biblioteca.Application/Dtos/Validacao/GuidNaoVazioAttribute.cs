using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.Dtos.Validacao;

/// <summary>
/// Exige que um <see cref="Guid"/> tenha sido informado de fato.
/// <para>
/// <c>[Required]</c> não serve aqui: <see cref="Guid"/> é tipo de valor, então uma
/// requisição que omita o campo chega com <see cref="Guid.Empty"/> — que para o
/// <c>[Required]</c> é um valor preenchido como qualquer outro.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class GuidNaoVazioAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is Guid identificador && identificador != Guid.Empty;

    public override string FormatErrorMessage(string name) =>
        ErrorMessage ?? $"O campo {name} deve conter um identificador válido.";
}
