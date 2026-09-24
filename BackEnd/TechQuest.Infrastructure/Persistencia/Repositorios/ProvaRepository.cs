using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

public class ProvaRepository : IProvaRepository
{
    private readonly TechQuestDbContext _db;
    public ProvaRepository(TechQuestDbContext db) => _db = db;

    /// <summary>
    /// Traz as alternativas (inclusive o gabarito) porque a correcao acontece
    /// no servidor. O recorte do que o cliente ve e feito no DTO, nao aqui.
    /// </summary>
    public Task<Prova?> ObterComQuestoesAsync(int idProva, CancellationToken ct = default)
        => _db.Provas
              .Include(p => p.Questoes).ThenInclude(q => q.Alternativas)
              .AsNoTracking()
              .FirstOrDefaultAsync(p => p.IdProva == idProva, ct);

    public Task<Prova?> ObterParaEdicaoAsync(int idProva, CancellationToken ct = default)
        => _db.Provas.FirstOrDefaultAsync(p => p.IdProva == idProva, ct);

    public void Adicionar(Prova prova) => _db.Provas.Add(prova);

    // As alternativas sao adicionadas junto pela navegacao da questao.
    public void AdicionarQuestao(Questao questao) => _db.Questoes.Add(questao);

    public Task<Questao?> ObterQuestaoAsync(int idQuestao, CancellationToken ct = default)
        => _db.Questoes.Include(q => q.Alternativas)
              .FirstOrDefaultAsync(q => q.IdQuestao == idQuestao, ct);

    public void RemoverQuestao(Questao questao) => _db.Questoes.Remove(questao);
}
