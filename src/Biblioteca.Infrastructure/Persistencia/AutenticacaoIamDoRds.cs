using Amazon.RDS.Util;
using Npgsql;

namespace Biblioteca.Infrastructure.Persistencia;

/// <summary>
/// Login no Aurora com token IAM no lugar da senha.
/// <para>
/// O token é uma assinatura gerada localmente com a credencial da AWS que estiver
/// disponível — na EC2, a role da instância, lida do metadata service — e vale
/// 15 minutos. Ele só é conferido ao abrir a conexão: uma conexão aberta continua
/// valendo depois que o token vence. Por isso basta que o pool sempre tenha um
/// token recente para as conexões novas.
/// </para>
/// </summary>
internal static class AutenticacaoIamDoRds
{
    // Folga de 5 minutos sobre a validade de 15: uma renovação que falhe ainda tem
    // tempo de ser repetida antes de o token atual vencer.
    private static readonly TimeSpan IntervaloDeRenovacao = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan IntervaloAposFalha = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Faz o pool do Npgsql pedir um token novo a cada <see cref="IntervaloDeRenovacao"/>.
    /// <para>
    /// Host, porta e usuário vêm da própria connection string: o token só vale para
    /// essa combinação. A região sai de <c>AWS_REGION</c>, e a credencial, da cadeia
    /// padrão do SDK.
    /// </para>
    /// </summary>
    public static NpgsqlDataSourceBuilder UsarTokenIamDoRds(this NpgsqlDataSourceBuilder builder) =>
        builder.UsePeriodicPasswordProvider(
            async (conexao, _) => await RDSAuthTokenGenerator.GenerateAuthTokenAsync(
                conexao.Host, conexao.Port, conexao.Username),
            IntervaloDeRenovacao,
            IntervaloAposFalha);
}
