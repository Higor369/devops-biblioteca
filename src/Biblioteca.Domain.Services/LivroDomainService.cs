using Biblioteca.Domain.Common;
using Biblioteca.Domain.Erros;
using Biblioteca.Domain.Repositories;
using Biblioteca.Domain.Services.Contratos;

namespace Biblioteca.Domain.Services;

/// <inheritdoc cref="ILivroDomainService"/>
public sealed class LivroDomainService(
    IAutorRepository autorRepository,
    IGeneroRepository generoRepository) : ILivroDomainService
{
    public async Task<Result> GarantirReferenciasValidasAsync(
        Guid autorId,
        Guid generoId,
        CancellationToken cancellationToken = default)
    {
        if (!await autorRepository.ExisteAsync(autorId, cancellationToken))
        {
            return Result.Failure(ErrosDeAutor.NaoEncontrado(autorId));
        }

        if (!await generoRepository.ExisteAsync(generoId, cancellationToken))
        {
            return Result.Failure(ErrosDeGenero.NaoEncontrado(generoId));
        }

        return Result.Success();
    }
}
