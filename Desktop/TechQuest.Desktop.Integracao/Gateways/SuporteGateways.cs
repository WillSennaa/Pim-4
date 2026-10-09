using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Integracao.Gateways;

public sealed class ChamadosGateway : IChamadosGateway
{
    private readonly ApiCliente _api;
    public ChamadosGateway(ApiCliente api) => _api = api;

    public Task<IReadOnlyList<Chamado>> ListarTecnicosAsync(CancellationToken ct = default)
        => _api.GetAsync<IReadOnlyList<Chamado>>("api/admin/chamados", ct);

    public Task AlterarStatusAsync(int idChamado, string status, CancellationToken ct = default)
        => _api.PatchAsync($"api/chamados/{idChamado}/status", new AlteracaoStatusChamado(status), ct);

    public Task ResponderAsync(int idChamado, string texto, CancellationToken ct = default)
        => _api.PostAsync($"api/chamados/{idChamado}/responder", new RespostaChamado(texto), ct);
}

public sealed class MedalhasGateway : IMedalhasGateway
{
    private readonly ApiCliente _api;
    public MedalhasGateway(ApiCliente api) => _api = api;

    public Task<IReadOnlyList<MedalhaAdmin>> ListarAsync(CancellationToken ct = default)
        => _api.GetAsync<IReadOnlyList<MedalhaAdmin>>("api/admin/medalhas", ct);

    public Task<MedalhaAdmin> CriarAsync(DadosMedalha dados, CancellationToken ct = default)
        => _api.PostAsync<MedalhaAdmin>("api/admin/medalhas", dados, ct);

    public Task<MedalhaAdmin> AtualizarAsync(int idMedalha, DadosMedalha dados, CancellationToken ct = default)
        => _api.PutAsync<MedalhaAdmin>($"api/admin/medalhas/{idMedalha}", dados, ct);
}

public sealed class ContaGateway : IContaGateway
{
    private readonly ApiCliente _api;
    public ContaGateway(ApiCliente api) => _api = api;

    public Task<PerfilUsuario> ObterPerfilAsync(CancellationToken ct = default)
        => _api.GetAsync<PerfilUsuario>("api/perfil", ct);

    public Task TrocarSenhaAsync(TrocaSenha troca, CancellationToken ct = default)
        => _api.PutSemRetornoAsync("api/perfil/senha", troca, ct);

    public async Task<string> TrocarEmailAsync(TrocaEmail troca, CancellationToken ct = default)
        => (await _api.PutAsync<EmailAtualizado>("api/perfil/email", troca, ct)).Email;
}
