namespace Biblioteca.Domain.Common;

/// <summary>
/// Classifica a natureza de uma falha de negócio.
/// O domínio não conhece HTTP: a tradução para status code acontece na camada de API.
/// </summary>
public enum ErrorType
{
    /// <summary>Dado informado é inválido. Mapeado para 400 na API.</summary>
    Validation,

    /// <summary>Recurso referenciado não existe. Mapeado para 404 na API.</summary>
    NotFound,

    /// <summary>Operação conflita com o estado atual dos dados. Mapeado para 409 na API.</summary>
    Conflict
}

/// <summary>
/// Descreve uma falha de negócio. O <paramref name="Code"/> é estável e legível por máquina,
/// para que um consumidor da API possa reagir a ele sem depender do texto da mensagem.
/// </summary>
public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error Validation(string code, string message) => new(ErrorType.Validation, code, message);

    public static Error NotFound(string code, string message) => new(ErrorType.NotFound, code, message);

    public static Error Conflict(string code, string message) => new(ErrorType.Conflict, code, message);
}
