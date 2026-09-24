using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class ProgressoRepository : IProgressoRepository
{
    private readonly TechQuestDbContext _db;
    public ProgressoRepository(TechQuestDbContext db) => _db = db;

    /// <summary>
    /// Indexado por ID_Material para o servico cruzar com a lista de materiais
    /// em memoria, sem uma consulta por aula (problema N+1).
    /// </summary>
    public async Task<IReadOnlyDictionary<int, Progresso>> ObterPorCursoAsync(
        int idEstudante, int idCurso, CancellationToken ct = default)
    {
        var lista = await _db.Progressos
            .Where(p => p.IdEstudante == idEstudante && p.Material.IdCurso == idCurso)
            .AsNoTracking()
            .ToListAsync(ct);

        return lista
            .GroupBy(p => p.IdMaterial)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.DataVisualizacao).First());
    }

    // Rastreado: este e alterado em seguida pelo servico.
    public Task<Progresso?> ObterAsync(int idEstudante, int idMaterial, CancellationToken ct = default)
        => _db.Progressos.FirstOrDefaultAsync(
               p => p.IdEstudante == idEstudante && p.IdMaterial == idMaterial, ct);

    public void Adicionar(Progresso progresso) => _db.Progressos.Add(progresso);

    public async Task<(int Total, int Concluidas)> ContarAulasDoCursoAsync(
        int idEstudante, int idCurso, CancellationToken ct = default)
    {
        var total = await _db.Materiais.CountAsync(m => m.IdCurso == idCurso, ct);

        var concluidas = await _db.Progressos
            .Where(p => p.IdEstudante == idEstudante && p.Concluido && p.Material.IdCurso == idCurso)
            .Select(p => p.IdMaterial)
            .Distinct()
            .CountAsync(ct);

        return (total, concluidas);
    }
}
