using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Fila de suporte tecnico (RF09/RF10). Chamado tecnico nasce sem
/// destinatario: qualquer administrador assume. "Assumir" so muda a situacao
/// para Em andamento, sinalizando aos outros admins que alguem esta cuidando.
/// </summary>
public sealed class SuporteService
{
    public const int TamanhoMinimoResposta = 10;
    public const int TamanhoMaximoResposta = 2000;

    private readonly IChamadosGateway _gateway;
    public SuporteService(IChamadosGateway gateway) => _gateway = gateway;

    public Task<IReadOnlyList<Chamado>> ListarAsync(CancellationToken ct = default)
        => _gateway.ListarTecnicosAsync(ct);

    public async Task<ResultadoOperacao> AssumirAsync(Chamado chamado, CancellationToken ct = default)
    {
        if (chamado.Status != StatusChamado.Aberto)
            return ResultadoOperacao.Falha("Só é possível assumir um chamado que ainda está aberto.");

        await _gateway.AlterarStatusAsync(chamado.Id, StatusChamado.EmAndamento, ct);
        return ResultadoOperacao.Ok();
    }

    /// <summary>
    /// Responde e resolve numa operacao so: no servidor, a resposta vira um
    /// novo chamado do tipo Resposta enderecado a quem abriu, e o original e
    /// marcado como Resolvido na mesma transacao.
    /// </summary>
    public async Task<ResultadoOperacao> ResponderAsync(Chamado chamado, string? texto, CancellationToken ct = default)
    {
        var validacao = ValidarResposta(texto);
        if (!validacao.Sucesso) return validacao;

        if (chamado.Status == StatusChamado.Resolvido)
            return ResultadoOperacao.Falha("Este chamado já foi resolvido.");

        await _gateway.ResponderAsync(chamado.Id, texto!.Trim(), ct);
        return ResultadoOperacao.Ok();
    }

    public static ResultadoOperacao ValidarResposta(string? texto)
    {
        var t = texto?.Trim() ?? "";
        if (t.Length < TamanhoMinimoResposta)
            return ResultadoOperacao.Falha(
                $"Escreva a resposta ao usuário (mínimo de {TamanhoMinimoResposta} caracteres).");
        if (t.Length > TamanhoMaximoResposta)
            return ResultadoOperacao.Falha($"A resposta pode ter no máximo {TamanhoMaximoResposta} caracteres.");
        return ResultadoOperacao.Ok();
    }
}
