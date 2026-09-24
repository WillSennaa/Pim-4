using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;

namespace TechQuest.Application.Servicos;

public class RelatorioService
{
    private readonly IRelatorioRepository _relatorios;
    public RelatorioService(IRelatorioRepository relatorios) => _relatorios = relatorios;

    public Task<IReadOnlyList<LinhaRelatorioDesempenhoDto>> DesempenhoAsync(
        int idEstudante, CancellationToken ct = default)
        => _relatorios.DesempenhoDoEstudanteAsync(idEstudante, ct);
}
