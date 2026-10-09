using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Casos de uso da moderacao de cursos (RF04): listar a fila, abrir o curso
/// completo, aprovar e rejeitar.
/// </summary>
public sealed class AvaliacaoCursosService
{
    /// <summary>
    /// Motivo minimo para rejeitar.
    ///
    /// DIFERENCA CONSCIENTE EM RELACAO AO WEB: la o motivo e opcional, e sem
    /// ele o tutor recebe um texto generico ("Revise o conteudo e submeta
    /// novamente"). Rejeitar sem dizer o que corrigir faz o curso voltar
    /// igual, e a fila gira sem progresso. Dez caracteres barram o campo
    /// vazio e o "nao" sem obrigar a um texto longo.
    /// </summary>
    public const int TamanhoMinimoMotivo = 10;
    public const int TamanhoMaximoMotivo = 1000;

    private readonly ICursosAdminGateway _gateway;
    private readonly ContadorDePendencias _contador;

    public AvaliacaoCursosService(ICursosAdminGateway gateway, ContadorDePendencias contador)
    {
        _gateway = gateway;
        _contador = contador;
    }

    /// <summary>Fila de avaliacao. Atualiza o selo do menu de passagem.</summary>
    public async Task<IReadOnlyList<SolicitacaoCurso>> ListarPendentesAsync(CancellationToken ct = default)
    {
        var lista = await _gateway.ListarAsync(StatusCurso.Pendente, ct);
        _contador.Informar(lista.Count);
        return lista;
    }

    /// <summary>
    /// Todos os cursos da plataforma, nos quatro estados.
    ///
    /// A API so lista por estado (GET /api/admin/cursos?status=), entao sao
    /// quatro chamadas, disparadas EM PARALELO com Task.WhenAll: o tempo total
    /// e o da mais lenta, e nao a soma das quatro. Mesma estrategia do web
    /// (TQ.carregarAdmin), que tambem busca os quatro estados.
    ///
    /// ALTERNATIVA REJEITADA: criar GET /api/admin/cursos sem filtro. Seria
    /// uma chamada so, mas mudaria a API para atender um cliente, e o estado
    /// do projeto registra que nenhum endpoint novo deveria ser necessario.
    /// </summary>
    public async Task<IReadOnlyList<SolicitacaoCurso>> ListarTodosAsync(CancellationToken ct = default)
    {
        var estados = new[] { StatusCurso.Pendente, StatusCurso.Publicado, StatusCurso.Rascunho, StatusCurso.Rejeitado };
        var listas = await Task.WhenAll(estados.Select(s => _gateway.ListarAsync(s, ct)));

        _contador.Informar(listas[0].Count); // de passagem, o selo de pendentes
        return listas.SelectMany(l => l).ToList();
    }

    public Task<CursoRevisao> ObterRevisaoAsync(int idCurso, CancellationToken ct = default)
        => _gateway.ObterParaRevisaoAsync(idCurso, ct);

    /// <summary>
    /// Publica o curso. Recusas do servidor (curso ja avaliado por outro
    /// administrador, por exemplo) sobem como OperacaoRecusadaException, com
    /// a mensagem do proprio servidor.
    /// </summary>
    public Task AprovarAsync(int idCurso, CancellationToken ct = default)
        => _gateway.AprovarAsync(idCurso, ct);

    public async Task<ResultadoOperacao> RejeitarAsync(int idCurso, string? motivo, CancellationToken ct = default)
    {
        var validacao = ValidarMotivo(motivo);
        if (!validacao.Sucesso) return validacao;

        await _gateway.RejeitarAsync(idCurso, motivo!.Trim(), ct);
        return ResultadoOperacao.Ok();
    }

    /// <summary>
    /// Publica para o presenter poder conferir o motivo ANTES de pedir
    /// confirmacao. A regra continua num lugar so: RejeitarAsync usa este
    /// mesmo metodo, entao nao ha como as duas checagens divergirem.
    /// </summary>
    public static ResultadoOperacao ValidarMotivo(string? motivo)
    {
        var texto = motivo?.Trim() ?? string.Empty;

        if (texto.Length < TamanhoMinimoMotivo)
            return ResultadoOperacao.Falha(
                $"Explique ao tutor o que precisa ser corrigido (mínimo de {TamanhoMinimoMotivo} caracteres).");

        if (texto.Length > TamanhoMaximoMotivo)
            return ResultadoOperacao.Falha(
                $"O motivo pode ter no máximo {TamanhoMaximoMotivo} caracteres.");

        return ResultadoOperacao.Ok();
    }
}
