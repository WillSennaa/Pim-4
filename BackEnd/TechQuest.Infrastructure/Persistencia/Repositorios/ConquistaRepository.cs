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

    // --- catalogo de medalhas ---

    public Task<Medalha?> ObterMedalhaParaEdicaoAsync(int idMedalha, CancellationToken ct = default)
        => _db.Medalhas.FirstOrDefaultAsync(m => m.IdMedalha == idMedalha, ct);

    /// <summary>
    /// O nome identifica a medalha para as regras de concessao
    /// (RegrasMedalhas casa por nome), entao duplicar nome quebraria a
    /// concessao automatica. ignorarId existe para a edicao nao acusar
    /// conflito com a propria medalha.
    /// </summary>
    public Task<bool> NomeDeMedalhaExisteAsync(
        string nome, int? ignorarId = null, CancellationToken ct = default)
        => _db.Medalhas.AnyAsync(
               m => m.Nome == nome && (ignorarId == null || m.IdMedalha != ignorarId), ct);

    /// <summary>
    /// Quantos estudantes ja receberam cada medalha. Uma consulta agregada
    /// para a lista inteira, em vez de uma por medalha.
    /// </summary>
    public async Task<IReadOnlyDictionary<int, int>> ContarConquistasPorMedalhaAsync(
        CancellationToken ct = default)
        => await _db.Conquistas
                    .GroupBy(c => c.IdMedalha)
                    .Select(g => new { IdMedalha = g.Key, Total = g.Count() })
                    .ToDictionaryAsync(x => x.IdMedalha, x => x.Total, ct);

    public void AdicionarMedalha(Medalha medalha) => _db.Medalhas.Add(medalha);
}
