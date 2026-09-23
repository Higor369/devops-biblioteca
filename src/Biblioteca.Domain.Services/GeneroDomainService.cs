using Biblioteca.Domain.Common;
using Biblioteca.Domain.Entidades.ValueObjects;
using Biblioteca.Domain.Erros;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;

namespace Biblioteca.Domain.Services;

/// <inheritdoc cref="IGeneroDomainService"/>
public sealed class GeneroDomainService(
    IGeneroRepository generoRepository,
    ILivroRepository livroRepository) : IGeneroDomainService
{
    public async Task<Result> GarantirNomeDisponivelAsync(
        NomeDeGenero nome,
        Guid? idIgnorado = null,
        CancellationToken cancellationToken = default)
    {
        // Não há normalização a fazer aqui: o nome chega como objeto de valor, ou seja,
        // já validado e com os espaços das bordas removidos. Receber NomeDeGenero em vez
        // de string torna impossível chamar este método com um texto cru.
        var jaExiste = await generoRepository.ExisteComNomeAsync(nome, idIgnorado, cancellationToken);

        return jaExiste
            ? Result.Failure(ErrosDeGenero.NomeJaCadastrado(nome.Valor))
            : Result.Success();
    }

    public async Task<Result> GarantirRemocaoPermitidaAsync(
        Guid generoId,
        CancellationToken cancellationToken = default)
    {
        var livrosVinculados = await livroRepository.ContarPorGeneroAsync(generoId, cancellationToken);

        return livrosVinculados > 0
            ? Result.Failure(ErrosDeGenero.PossuiLivrosVinculados(livrosVinculados))
            : Result.Success();
    }
}
