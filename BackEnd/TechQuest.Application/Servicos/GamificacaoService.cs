using TechQuest.Application.Interfaces;
using TechQuest.Domain.Regras;

namespace TechQuest.Application.Servicos;

/// <summary>
/// Ponto unico de calculo de XP. Antes o login calculava por conta propria;
/// centralizar evita que login, perfil e conclusao de aula divirjam.
/// </summary>
public class GamificacaoService
{
    private readonly IGamificacaoRepository _repo;
    public GamificacaoService(IGamificacaoRepository repo) => _repo = repo;

    public record ResumoXp(
        int Xp, int Nivel, int XpProximoNivel, int ProgressoNivel,
        int MateriaisConcluidos, int ProvasAprovadas, int NotasMaximas, int Medalhas);

    public async Task<ResumoXp> ObterAsync(int idEstudante, CancellationToken ct = default)
    {
        var c = await _repo.ObterContagensAsync(idEstudante, ct);

        var xp = RegrasGamificacao.CalcularXp(
            c.MateriaisConcluidos, c.ProvasAprovadas, c.NotasMaximas, c.Medalhas);

        return new ResumoXp(
            xp,
            RegrasGamificacao.CalcularNivel(xp),
            RegrasGamificacao.XpDoProximoNivel(xp),
            RegrasGamificacao.ProgressoNoNivel(xp),
            c.MateriaisConcluidos, c.ProvasAprovadas, c.NotasMaximas, c.Medalhas);
    }
}
