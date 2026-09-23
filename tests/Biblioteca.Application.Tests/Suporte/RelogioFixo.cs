namespace Biblioteca.Application.Tests.Suporte;

/// <summary>Relógio parado em um ano conhecido, para tornar a validação de ano determinística.</summary>
internal sealed class RelogioFixo(int ano) : TimeProvider
{
    public int Ano { get; } = ano;

    public override DateTimeOffset GetUtcNow() => new(Ano, 6, 15, 12, 0, 0, TimeSpan.Zero);
}
