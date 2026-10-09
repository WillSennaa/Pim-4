using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class ChamadoRepository : IChamadoRepository
{
    private readonly TechQuestDbContext _db;
    public ChamadoRepository(TechQuestDbContext db) => _db = db;

    /// <summary>Caixa do usuario: o que ele enviou e o que foi enviado a ele.</summary>
    public async Task<IReadOnlyList<Chamado>> ListarDoUsuarioAsync(
        int idUsuario, CancellationToken ct = default)
        => await Completo()
                    .Where(c => c.IdRemetente == idUsuario || c.IdDestinatario == idUsuario)
                    .OrderByDescending(c => c.DataAbertura)
                    .AsNoTracking()
                    .ToListAsync(ct);

    public Task<Chamado?> ObterAsync(int idChamado, CancellationToken ct = default)
        => _db.Chamados
              .Include(c => c.Remetente).Include(c => c.Destinatario).Include(c => c.Curso)
              .FirstOrDefaultAsync(c => c.IdChamado == idChamado, ct);

    public void Adicionar(Chamado chamado) => _db.Chamados.Add(chamado);

    public async Task<IReadOnlyList<Chamado>> ListarPorTipoAsync(
        string tipo, CancellationToken ct = default)
        => await Completo().Where(c => c.Tipo == tipo)
                 .OrderByDescending(c => c.DataAbertura).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Chamado>> ListarDoDestinatarioAsync(
        int idUsuario, CancellationToken ct = default)
        => await Completo().Where(c => c.IdDestinatario == idUsuario)
                 .OrderByDescending(c => c.DataAbertura).AsNoTracking().ToListAsync(ct);

    // O curso entra nas consultas porque a duvida educacional e sempre sobre
    // um curso, e o tutor precisa do contexto para responder.
    private IQueryable<Chamado> Completo()
        => _db.Chamados
              .Include(c => c.Remetente).Include(c => c.Destinatario).Include(c => c.Curso);

}
