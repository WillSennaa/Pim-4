using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class DesempenhoRepository : IDesempenhoRepository
{
    private readonly TechQuestDbContext _db;
    public DesempenhoRepository(TechQuestDbContext db) => _db = db;

    public Task<int> ContarTentativasAsync(int idEstudante, int idProva, CancellationToken ct = default)
        => _db.Desempenhos.CountAsync(d => d.IdEstudante == idEstudante && d.IdProva == idProva, ct);

    // Apenas registra a intencao; a gravacao e responsabilidade da unidade de trabalho.
    public void Registrar(Desempenho desempenho) => _db.Desempenhos.Add(desempenho);

    public Task<bool> AprovadoNaProvaAsync(int idEstudante, int idProva, CancellationToken ct = default)
        => _db.Desempenhos
              .AnyAsync(d => d.IdEstudante == idEstudante
                             && d.IdProva == idProva
                             && d.Nota != null
                             && d.Nota >= d.Prova.NotaMinima, ct);
}
