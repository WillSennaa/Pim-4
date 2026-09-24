using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class AdminRepository : IAdminRepository
{
    private readonly TechQuestDbContext _db;
    public AdminRepository(TechQuestDbContext db) => _db = db;

    public async Task<ResumoAdminDto> ResumoAsync(CancellationToken ct = default)
        => new(
            await _db.Usuarios.CountAsync(ct),
            await _db.Usuarios.CountAsync(u => u.Ativo, ct),
            await _db.Estudantes.CountAsync(ct),
            await _db.Tutores.CountAsync(ct),
            await _db.Cursos.CountAsync(c => c.Status == "Publicado", ct),
            await _db.Cursos.CountAsync(c => c.Status == "Pendente", ct),
            await _db.Historicos.CountAsync(ct),
            await _db.Certificados.CountAsync(ct),
            await _db.Chamados.CountAsync(c => c.Status == "Aberto", ct));

    public async Task<IReadOnlyList<LogAuditoria>> ListarLogsAsync(
        int limite, CancellationToken ct = default)
        => await _db.LogsAuditoria
                    .Include(l => l.Adm).ThenInclude(a => a.Usuario)
                    .OrderByDescending(l => l.DataAcao)
                    .Take(limite)
                    .AsNoTracking()
                    .ToListAsync(ct);

    public void RegistrarLog(LogAuditoria log) => _db.LogsAuditoria.Add(log);
}
