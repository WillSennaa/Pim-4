using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class ConquistaRepository : IConquistaRepository
{
    private readonly TechQuestDbContext _db;
    public ConquistaRepository(TechQuestDbContext db) => _db = db;

    public async Task<IReadOnlyList<Medalha>> ListarMedalhasAsync(CancellationToken ct = default)
        => await _db.Medalhas.OrderBy(m => m.IdMedalha).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Conquista>> ListarDoEstudanteAsync(
        int idEstudante, CancellationToken ct = default)
        => await _db.Conquistas
                    .Include(c => c.Medalha)
                    .Where(c => c.IdEstudante == idEstudante)
                    .AsNoTracking()
                    .ToListAsync(ct);

    public async Task<IReadOnlyList<Medalha>> ObterMedalhasPorNomeAsync(
        IEnumerable<string> nomes, CancellationToken ct = default)
    {
        var lista = nomes.ToList();
        return await _db.Medalhas.Where(m => lista.Contains(m.Nome)).AsNoTracking().ToListAsync(ct);
    }

    public Task<Medalha?> ObterMedalhaAsync(int idMedalha, CancellationToken ct = default)
        => _db.Medalhas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMedalha == idMedalha, ct);

    public void Adicionar(Conquista conquista) => _db.Conquistas.Add(conquista);
}
