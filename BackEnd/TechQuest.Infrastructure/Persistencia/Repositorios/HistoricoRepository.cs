using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class HistoricoRepository : IHistoricoRepository
{
    private readonly TechQuestDbContext _db;
    public HistoricoRepository(TechQuestDbContext db) => _db = db;

    public Task<Historico?> ObterAsync(int idEstudante, int idCurso, CancellationToken ct = default)
        => _db.Historicos.FirstOrDefaultAsync(
               h => h.IdEstudante == idEstudante && h.IdCurso == idCurso, ct);

    public async Task<IReadOnlyList<Historico>> ListarPorEstudanteAsync(
        int idEstudante, CancellationToken ct = default)
        => await _db.Historicos
                    .Include(h => h.Curso)
                    .Include(h => h.Certificado)
                    .Where(h => h.IdEstudante == idEstudante)
                    .OrderByDescending(h => h.IdHistorico)
                    .AsNoTracking()
                    .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> NomesDeCursosConcluidosAsync(
        int idEstudante, CancellationToken ct = default)
        => await _db.Historicos
                    .Where(h => h.IdEstudante == idEstudante && h.StatusConclusao == "Concluido")
                    .Select(h => h.Curso.Nome)
                    .AsNoTracking()
                    .ToListAsync(ct);

    public void Adicionar(Historico historico) => _db.Historicos.Add(historico);
}
