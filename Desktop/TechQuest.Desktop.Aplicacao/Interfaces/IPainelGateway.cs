using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Interfaces;

/// <summary>Contadores gerais da plataforma (GET /api/admin/resumo).</summary>
public interface IPainelGateway
{
    Task<ResumoAdmin> ObterResumoAsync(CancellationToken ct = default);
}
