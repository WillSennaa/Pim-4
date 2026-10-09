using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Integracao.Gateways;

/// <summary>
/// Implementacao HTTP de IAutenticacaoGateway: POST api/auth/login.
///
/// O 401 de credencial errada ja sai do ApiCliente como
/// NaoAutenticadoException, que e exatamente o que o contrato promete;
/// por isso esta classe e uma linha. Gateways de outras areas seguem o
/// mesmo formato: conhecem a ROTA e o MODELO, e nada mais.
/// </summary>
public sealed class AutenticacaoGateway : IAutenticacaoGateway
{
    private readonly ApiCliente _api;
    public AutenticacaoGateway(ApiCliente api) => _api = api;

    public Task<RespostaLogin> EntrarAsync(CredenciaisLogin credenciais, CancellationToken ct = default)
        => _api.PostAsync<RespostaLogin>("api/auth/login", credenciais, ct);
}
