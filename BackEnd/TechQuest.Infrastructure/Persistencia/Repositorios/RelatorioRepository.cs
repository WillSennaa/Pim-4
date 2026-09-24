using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Infrastructure.Persistencia.Consultas;

namespace TechQuest.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Unico ponto do sistema que executa SQL bruto.
///
/// POR QUE PROCEDURE AQUI: o relatorio cruza quatro tabelas e e uma consulta
/// de leitura pura, sem regra de negocio. Mantida no banco, ela pode ser
/// reaproveitada por qualquer cliente (a aplicacao desktop, uma ferramenta de
/// BI) sem passar pela API, e o plano de execucao fica em cache no servidor.
/// O CRUD do dia a dia continua no EF Core, que da seguranca de tipos e
/// legibilidade — usar procedure para tudo devolveria o projeto aos anos 2000.
///
/// SEGURANCA: FromSqlInterpolated parametriza o valor interpolado
/// automaticamente (vira @p0). Concatenar a string a mao abriria SQL Injection.
/// </summary>
public class RelatorioRepository : IRelatorioRepository
{
    private readonly TechQuestDbContext _db;
    public RelatorioRepository(TechQuestDbContext db) => _db = db;

    public async Task<IReadOnlyList<LinhaRelatorioDesempenhoDto>> DesempenhoDoEstudanteAsync(
        int idEstudante, CancellationToken ct = default)
    {
        var linhas = await _db.Set<RelatorioDesempenhoItem>()
            .FromSqlInterpolated($"EXEC SP_RelatorioDesempenhoEstudante @ID_Estudante = {idEstudante}")
            .ToListAsync(ct);

        return linhas.Select(l => new LinhaRelatorioDesempenhoDto(
            l.Estudante, l.Prova, l.Nota, l.Tentativas, l.Data_Realizacao)).ToList();
    }
}
