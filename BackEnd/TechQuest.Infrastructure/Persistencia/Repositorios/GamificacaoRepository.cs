using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Fornece apenas CONTAGENS ao calculo de XP. A regra (quanto vale cada coisa)
/// fica no dominio; aqui so se conta o que aconteceu.
/// </summary>
public class GamificacaoRepository : IGamificacaoRepository
{
    private readonly TechQuestDbContext _db;
    public GamificacaoRepository(TechQuestDbContext db) => _db = db;

    public async Task<(int MateriaisConcluidos, int ProvasAprovadas, int NotasMaximas, int Medalhas)>
        ObterContagensAsync(int idEstudante, CancellationToken ct = default)
    {
        var materiais = await _db.Progressos
            .CountAsync(p => p.IdEstudante == idEstudante && p.Concluido, ct);

        // Uma prova aprovada conta uma vez, mesmo com varias tentativas
        // aprovadas: XP nao pode ser farmado repetindo a mesma prova.
        var provasAprovadas = await _db.Desempenhos
            .Where(d => d.IdEstudante == idEstudante
                        && d.Nota != null
                        && d.Nota >= d.Prova.NotaMinima)
            .Select(d => d.IdProva)
            .Distinct()
            .CountAsync(ct);

        var notasMaximas = await _db.Desempenhos
            .Where(d => d.IdEstudante == idEstudante && d.Nota >= 10m)
            .Select(d => d.IdProva)
            .Distinct()
            .CountAsync(ct);

        var medalhas = await _db.Conquistas
            .CountAsync(c => c.IdEstudante == idEstudante, ct);

        return (materiais, provasAprovadas, notasMaximas, medalhas);
    }
}
