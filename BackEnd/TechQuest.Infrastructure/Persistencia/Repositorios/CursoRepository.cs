using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class CursoRepository : ICursoRepository
{
    private readonly TechQuestDbContext _db;
    public CursoRepository(TechQuestDbContext db) => _db = db;

    public async Task<IReadOnlyList<Curso>> ListarPublicadosAsync(CancellationToken ct = default)
        => await _db.Cursos
                    .Include(c => c.TutorCriou!).ThenInclude(t => t.Usuario)
                    .Where(c => c.Status == "Publicado")
                    .OrderBy(c => c.Nome)
                    .AsNoTracking()
                    .ToListAsync(ct);

    public Task<Curso?> ObterComMateriaisAsync(int idCurso, CancellationToken ct = default)
        => _db.Cursos
              .Include(c => c.TutorCriou!).ThenInclude(t => t.Usuario)
              .Include(c => c.Materiais)
              .AsNoTracking()
              .FirstOrDefaultAsync(c => c.IdCurso == idCurso, ct);

    public Task<Material?> ObterMaterialAsync(int idMaterial, CancellationToken ct = default)
        => _db.Materiais.AsNoTracking().FirstOrDefaultAsync(m => m.IdMaterial == idMaterial, ct);

    public Task<int> ContarPublicadosAsync(CancellationToken ct = default)
        => _db.Cursos.CountAsync(c => c.Status == "Publicado", ct);

    // ---- escrita e gestao ----

    // Rastreado: o servico altera status, vinculo de prova e dados do curso.
    public Task<Curso?> ObterParaEdicaoAsync(int idCurso, CancellationToken ct = default)
        => _db.Cursos.FirstOrDefaultAsync(c => c.IdCurso == idCurso, ct);

    public async Task<IReadOnlyList<Curso>> ListarPorTutorAsync(
        int idTutor, CancellationToken ct = default)
        => await _db.Cursos
                    .Where(c => c.IdTutorCriou == idTutor)
                    .OrderByDescending(c => c.IdCurso)
                    .ToListAsync(ct);

    public async Task<IReadOnlyList<Curso>> ListarPorStatusAsync(
        string status, CancellationToken ct = default)
        => await _db.Cursos
                    .Include(c => c.TutorCriou!).ThenInclude(t => t.Usuario)
                    .Where(c => c.Status == status)
                    .OrderBy(c => c.IdCurso)
                    .AsNoTracking()
                    .ToListAsync(ct);

    /// <summary>Historico e a tabela de vinculo aluno-curso deste modelo.</summary>
    public Task<int> ContarMatriculadosAsync(int idCurso, CancellationToken ct = default)
        => _db.Historicos.CountAsync(h => h.IdCurso == idCurso, ct);

    public Task<int> ContarQuestoesAsync(int idProva, CancellationToken ct = default)
        => _db.Questoes.CountAsync(q => q.IdProva == idProva, ct);

    public void Adicionar(Curso curso) => _db.Cursos.Add(curso);
    public void AdicionarMaterial(Material material) => _db.Materiais.Add(material);
    public void RemoverMaterial(Material material) => _db.Materiais.Remove(material);

    public Task<Material?> ObterMaterialParaEdicaoAsync(int idMaterial, CancellationToken ct = default)
        => _db.Materiais.FirstOrDefaultAsync(m => m.IdMaterial == idMaterial, ct);

    public Task<bool> MaterialTemProgressoAsync(int idMaterial, CancellationToken ct = default)
        => _db.Progressos.AnyAsync(p => p.IdMaterial == idMaterial, ct);
}
