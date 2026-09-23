namespace Biblioteca.Domain.Common;

/// <summary>
/// Representa o desfecho de uma operação que pode falhar por razões de negócio previstas.
/// <para>
/// Falha de negócio não é exceção: título inválido ou autor inexistente são desfechos
/// esperados do fluxo, não defeitos. Exceções ficam reservadas ao que é realmente
/// excepcional (banco fora do ar, bug de programação).
/// </para>
/// </summary>
public class Result
{
    private readonly Error? _error;

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("Um resultado de sucesso não pode carregar um erro.");
        }

        if (!isSuccess && error is null)
        {
            throw new InvalidOperationException("Um resultado de falha precisa carregar um erro.");
        }

        IsSuccess = isSuccess;
        _error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    /// <summary>Erro da falha. Acessar em um resultado de sucesso é erro de programação.</summary>
    public Error Error => _error
        ?? throw new InvalidOperationException("Um resultado de sucesso não possui erro.");

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>Resultado que carrega um valor em caso de sucesso.</summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
        => _value = value;

    /// <summary>Valor produzido. Acessar em um resultado de falha é erro de programação.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Um resultado de falha não possui valor.");
}
