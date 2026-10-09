using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Integracao.Gateways;

public sealed class PainelGateway : IPainelGateway
{
    private readonly ApiCliente _api;
    public PainelGateway(ApiCliente api) => _api = api;

    public Task<ResumoAdmin> ObterResumoAsync(CancellationToken ct = default)
        => _api.GetAsync<ResumoAdmin>("api/admin/resumo", ct);
}
