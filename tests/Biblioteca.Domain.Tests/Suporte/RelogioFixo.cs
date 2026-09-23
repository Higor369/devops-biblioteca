namespace Biblioteca.Domain.Tests.Suporte;

/// <summary>
/// Relógio parado em um instante conhecido.
/// <para>
/// É o que permite testar "ano no futuro" sem que o teste dependa da data em que roda —
/// um teste escrito com <c>DateTime.Now</c> passa hoje e quebra na virada do ano.
/// </para>
/// </summary>
internal sealed class RelogioFixo(int ano) : TimeProvider
{
    public int Ano { get; } = ano;

    public override DateTimeOffset GetUtcNow() => new(Ano, 6, 15, 12, 0, 0, TimeSpan.Zero);
}
