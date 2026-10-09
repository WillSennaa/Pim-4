using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Caso de uso "ver o painel".
///
/// Alem de repassar o resumo, informa ao ContadorDePendencias quantos
/// cursos aguardam avaliacao: o Painel abre logo depois do login, entao o
/// selo do menu ja nasce preenchido sem requisicao extra.
///
/// O presenter fala com este servico e nunca com o gateway direto. Mesmo
/// quando o servico e fino, a regra das camadas vale sem excecao.
/// </summary>
public sealed class PainelService
{
    private readonly IPainelGateway _gateway;
    private readonly ContadorDePendencias _contador;

    public PainelService(IPainelGateway gateway, ContadorDePendencias contador)
    {
        _gateway = gateway;
        _contador = contador;
    }

    public async Task<ResumoAdmin> ObterResumoAsync(CancellationToken ct = default)
    {
        var resumo = await _gateway.ObterResumoAsync(ct);
        _contador.Informar(resumo.CursosPendentes);
        return resumo;
    }
}
