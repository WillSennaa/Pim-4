using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly TechQuestDbContext _db;
    public UsuarioRepository(TechQuestDbContext db) => _db = db;

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default)
        => Completo().AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<Usuario?> ObterPorIdAsync(int idUsuario, CancellationToken ct = default)
        => Completo().AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

    /// <summary>Sem AsNoTracking: o EF precisa acompanhar para gerar o UPDATE.</summary>
    public Task<Usuario?> ObterParaEdicaoAsync(int idUsuario, CancellationToken ct = default)
        => _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default)
        => await Completo().OrderBy(u => u.Nome).AsNoTracking().ToListAsync(ct);

    public Task<bool> EmailExisteAsync(string email, CancellationToken ct = default)
        => _db.Usuarios.AnyAsync(u => u.Email == email, ct);

    public void Adicionar(Usuario usuario) => _db.Usuarios.Add(usuario);
    public void Adicionar(Adm adm) => _db.Adms.Add(adm);
    public void Adicionar(Tutor tutor) => _db.Tutores.Add(tutor);
    public void Adicionar(Estudante estudante) => _db.Estudantes.Add(estudante);

    // As tres especializacoes vem juntas porque e delas que sai o papel.
    private IQueryable<Usuario> Completo()
        => _db.Usuarios.Include(u => u.Adm).Include(u => u.Tutor).Include(u => u.Estudante);
}
