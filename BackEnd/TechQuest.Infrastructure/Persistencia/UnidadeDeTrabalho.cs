using Microsoft.EntityFrameworkCore;
using TechQuest.Application.Interfaces;

namespace TechQuest.Infrastructure.Persistencia;

/// <summary>
/// Implementacao do Unit of Work sobre o DbContext.
///
/// DETALHE CRITICO: com EnableRetryOnFailure ligado (obrigatorio no Azure SQL
/// serverless), o EF Core PROIBE transacao manual iniciada direto pelo
/// BeginTransaction — ele nao sabe re-executar uma transacao ja aberta apos
/// uma falha transitoria. A forma correta e pedir a estrategia de execucao e
/// deixar que ELA abra e repita a transacao inteira.
/// </summary>
public class UnidadeDeTrabalho : IUnidadeDeTrabalho
{
    private readonly TechQuestDbContext _db;
    public UnidadeDeTrabalho(TechQuestDbContext db) => _db = db;

    public Task SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async Task ExecutarEmTransacaoAsync(Func<Task> acao, CancellationToken ct = default)
    {
        var estrategia = _db.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transacao = await _db.Database.BeginTransactionAsync(ct);
            await acao();
            await _db.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        });
    }
}
