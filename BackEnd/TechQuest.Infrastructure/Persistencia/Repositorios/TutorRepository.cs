using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class TutorRepository : ITutorRepository
{
    private readonly TechQuestDbContext _db;
    public TutorRepository(TechQuestDbContext db) => _db = db;

    /// <summary>
    /// Alunos dos cursos do tutor, com progresso e melhor nota.
    ///
    /// Sao QUATRO consultas agregadas no banco, combinadas em memoria, em vez de
    /// uma consulta por aluno. Com 50 alunos a versao ingenua faria mais de
    /// 150 idas ao banco — o classico problema N+1, que no Azure SQL custa
    /// latencia de rede a cada ida.
    /// </summary>
    public async Task<IReadOnlyList<AlunoDoTutorDto>> ListarAlunosAsync(
        int idTutor, int? idCurso, CancellationToken ct = default)
    {
        var matriculas = await _db.Historicos
            .Include(h => h.Curso)
            .Include(h => h.Estudante).ThenInclude(e => e.Usuario)
            .Where(h => h.Curso.IdTutorCriou == idTutor
                        && (idCurso == null || h.IdCurso == idCurso))
            .AsNoTracking()
            .ToListAsync(ct);

        if (matriculas.Count == 0) return Array.Empty<AlunoDoTutorDto>();

        var idsCursos = matriculas.Select(m => m.IdCurso).Distinct().ToList();

        var totalAulas = await _db.Materiais
            .Where(m => idsCursos.Contains(m.IdCurso))
            .GroupBy(m => m.IdCurso)
            .Select(g => new { IdCurso = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.IdCurso, x => x.Total, ct);

        var concluidas = await _db.Progressos
            .Where(p => p.Concluido && idsCursos.Contains(p.Material.IdCurso))
            .GroupBy(p => new { p.IdEstudante, p.Material.IdCurso })
            .Select(g => new { g.Key.IdEstudante, g.Key.IdCurso, Total = g.Count() })
            .ToListAsync(ct);

        var provaPorCurso = await _db.Cursos
            .Where(c => idsCursos.Contains(c.IdCurso) && c.IdProva != null)
            .Select(c => new { c.IdCurso, IdProva = c.IdProva!.Value })
            .ToDictionaryAsync(x => x.IdCurso, x => x.IdProva, ct);

        var idsProvas = provaPorCurso.Values.Distinct().ToList();

        var desempenhos = await _db.Desempenhos
            .Where(d => idsProvas.Contains(d.IdProva))
            .GroupBy(d => new { d.IdEstudante, d.IdProva })
            .Select(g => new
            {
                g.Key.IdEstudante,
                g.Key.IdProva,
                Melhor = g.Max(x => x.Nota),
                Tentativas = g.Count()
            })
            .ToListAsync(ct);

        return matriculas.Select(h =>
        {
            var total = totalAulas.GetValueOrDefault(h.IdCurso);
            var feitas = concluidas
                .FirstOrDefault(c => c.IdEstudante == h.IdEstudante && c.IdCurso == h.IdCurso)?.Total ?? 0;

            decimal? melhor = null;
            var tentativas = 0;
            if (provaPorCurso.TryGetValue(h.IdCurso, out var idProva))
            {
                var d = desempenhos.FirstOrDefault(
                    x => x.IdEstudante == h.IdEstudante && x.IdProva == idProva);
                melhor = d?.Melhor;
                tentativas = d?.Tentativas ?? 0;
            }

            return new AlunoDoTutorDto(
                h.IdEstudante, h.Estudante.Usuario.Nome, h.Estudante.Usuario.Email,
                h.IdCurso, h.Curso.Nome, h.StatusConclusao,
                feitas, total,
                total == 0 ? 0 : (int)Math.Round(feitas * 100.0 / total),
                melhor, tentativas);
        })
        .OrderBy(a => a.Curso).ThenBy(a => a.Nome)
        .ToList();
    }

    public Task<bool> EstudanteExisteAsync(int idEstudante, CancellationToken ct = default)
        => _db.Estudantes.AnyAsync(e => e.IdEstudante == idEstudante, ct);
}
