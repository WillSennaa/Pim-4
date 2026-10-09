using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Integracao.Gateways;

public sealed class CursosAdminGateway : ICursosAdminGateway
{
    private readonly ApiCliente _api;
    public CursosAdminGateway(ApiCliente api) => _api = api;

    public Task<IReadOnlyList<SolicitacaoCurso>> ListarAsync(string status, CancellationToken ct = default)
        // Uri.EscapeDataString: o status vem de constante hoje, mas valor
        // concatenado em URL sem escape e o tipo de descuido que vira falha
        // quando alguem passa um texto com espaco ou "&".
        => _api.GetAsync<IReadOnlyList<SolicitacaoCurso>>(
            "api/admin/cursos?status=" + Uri.EscapeDataString(status), ct);

    public Task<CursoRevisao> ObterParaRevisaoAsync(int idCurso, CancellationToken ct = default)
        => _api.GetAsync<CursoRevisao>($"api/admin/cursos/{idCurso}", ct);

    public Task AprovarAsync(int idCurso, CancellationToken ct = default)
        => _api.PostAsync($"api/admin/cursos/{idCurso}/aprovar", null, ct);

    public Task RejeitarAsync(int idCurso, string motivo, CancellationToken ct = default)
        => _api.PostAsync($"api/admin/cursos/{idCurso}/rejeitar", new RejeicaoCurso(motivo), ct);
}
