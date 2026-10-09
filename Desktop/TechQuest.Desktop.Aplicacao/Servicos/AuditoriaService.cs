using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Consulta da trilha de auditoria (RF11). Somente leitura: o log e escrito
/// pela API (avaliacao de curso) e pela trigger do banco (status de usuario),
/// nunca pelo cliente. Um cliente que pudesse gravar no log poderia forjar
/// o proprio rastro.
/// </summary>
public sealed class AuditoriaService
{
    /// <summary>
    /// Quantidades oferecidas na tela. A lista e fechada para nao virar um
    /// "traga tudo" acidental: o log so cresce, e a API devolve o que for
    /// pedido, sem paginacao.
    /// </summary>
    public static readonly IReadOnlyList<int> LimitesPermitidos = new[] { 50, 100, 200, 500 };

    private readonly IAuditoriaGateway _gateway;
    public AuditoriaService(IAuditoriaGateway gateway) => _gateway = gateway;

    public Task<IReadOnlyList<LogAuditoria>> ListarAsync(int limite, CancellationToken ct = default)
    {
        if (!LimitesPermitidos.Contains(limite))
            throw new ArgumentOutOfRangeException(nameof(limite), limite, "Limite fora das opções da tela.");
        return _gateway.ListarAsync(limite, ct);
    }
}
