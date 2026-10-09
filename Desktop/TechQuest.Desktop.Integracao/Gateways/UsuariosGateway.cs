using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Integracao.Gateways;

public sealed class UsuariosGateway : IUsuariosGateway
{
    private readonly ApiCliente _api;
    public UsuariosGateway(ApiCliente api) => _api = api;

    public Task<IReadOnlyList<UsuarioAdmin>> ListarAsync(CancellationToken ct = default)
        => _api.GetAsync<IReadOnlyList<UsuarioAdmin>>("api/admin/usuarios", ct);

    public Task<UsuarioAdmin> CriarAsync(NovoUsuario novo, CancellationToken ct = default)
        => _api.PostAsync<UsuarioAdmin>("api/admin/usuarios", novo, ct);

    public Task AlterarStatusAsync(int idUsuario, bool ativo, CancellationToken ct = default)
        => _api.PatchAsync($"api/admin/usuarios/{idUsuario}/status", new AlteracaoStatus(ativo), ct);
}

public sealed class AuditoriaGateway : IAuditoriaGateway
{
    private readonly ApiCliente _api;
    public AuditoriaGateway(ApiCliente api) => _api = api;

    public Task<IReadOnlyList<LogAuditoria>> ListarAsync(int limite, CancellationToken ct = default)
        => _api.GetAsync<IReadOnlyList<LogAuditoria>>($"api/admin/logs?limite={limite}", ct);
}
