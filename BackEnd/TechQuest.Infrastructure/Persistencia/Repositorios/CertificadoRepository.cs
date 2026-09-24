using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class CertificadoRepository : ICertificadoRepository
{
    private readonly TechQuestDbContext _db;
    public CertificadoRepository(TechQuestDbContext db) => _db = db;

    public async Task<IReadOnlyList<Certificado>> ListarPorEstudanteAsync(
        int idEstudante, CancellationToken ct = default)
        => await ConsultaCompleta()
                 .Where(c => c.Historico.IdEstudante == idEstudante)
                 .OrderByDescending(c => c.DataEmissao)
                 .ToListAsync(ct);

    public Task<Certificado?> ObterPorCodigoAsync(Guid codigo, CancellationToken ct = default)
        => ConsultaCompleta().FirstOrDefaultAsync(c => c.CodigoAutenticacao == codigo, ct);

    public void Adicionar(Certificado certificado) => _db.Certificados.Add(certificado);

    // O certificado so faz sentido com curso e estudante juntos: o nome do
    // aluno e do curso sao impressos nele.
    private IQueryable<Certificado> ConsultaCompleta()
        => _db.Certificados
              .Include(c => c.Historico).ThenInclude(h => h.Curso)
              .Include(c => c.Historico).ThenInclude(h => h.Estudante).ThenInclude(e => e.Usuario)
              .AsNoTracking();
}
