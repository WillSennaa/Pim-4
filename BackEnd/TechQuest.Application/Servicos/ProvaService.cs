using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;
using TechQuest.Domain.Regras;

namespace TechQuest.Application.Servicos;

public class ProvaService
{
    private readonly IProvaRepository _provas;
    private readonly IDesempenhoRepository _desempenhos;
    private readonly ConquistaService _conquistas;
    private readonly ConclusaoCursoService _conclusao;
    private readonly IUnidadeDeTrabalho _uow;

    public ProvaService(
        IProvaRepository provas, IDesempenhoRepository desempenhos,
        ConquistaService conquistas, ConclusaoCursoService conclusao,
        IUnidadeDeTrabalho uow)
    {
        _provas = provas; _desempenhos = desempenhos;
        _conquistas = conquistas; _conclusao = conclusao; _uow = uow;
    }

    /// <summary>Prova para responder: sem gabarito em nenhum nivel do JSON.</summary>
    public async Task<ProvaDto?> ObterParaResponderAsync(int idProva, CancellationToken ct = default)
    {
        var prova = await _provas.ObterComQuestoesAsync(idProva, ct);
        if (prova is null) return null;

        var questoes = prova.Questoes
            .OrderBy(q => q.Ordem)
            .Select(q => new QuestaoDto(
                q.IdQuestao, q.Ordem, q.Enunciado, q.CodigoExemplo,
                q.Alternativas
                    .OrderBy(a => a.Letra)
                    .Select(a => new AlternativaDto(a.IdAlternativa, a.Letra, a.Texto))
                    .ToList()))
            .ToList();

        return new ProvaDto(
            prova.IdProva, prova.Titulo, prova.NotaMinima, prova.TempoMinutos,
            questoes.Count, questoes);
    }

    /// <summary>
    /// Correcao no servidor. A nota reproduz a formula do PIM III
    /// (acertos / total * 10) para manter o comportamento identico ao
    /// front-end estatico, e a aprovacao usa a nota minima vinda do banco.
    /// Gravacao da tentativa e concessao de medalhas numa transacao so.
    /// </summary>
    public async Task<ResultadoTentativaDto?> CorrigirAsync(
        int idProva, int idEstudante, TentativaRequest req, CancellationToken ct = default)
    {
        var prova = await _provas.ObterComQuestoesAsync(idProva, ct);
        if (prova is null) return null;

        var marcadas = req.Respostas.ToDictionary(r => r.IdQuestao, r => r.Letra);
        var correcao = new List<CorrecaoQuestaoDto>();
        var acertos = 0;

        foreach (var q in prova.Questoes.OrderBy(q => q.Ordem))
        {
            var letraCorreta = q.AlternativaCorreta()?.Letra;
            marcadas.TryGetValue(q.IdQuestao, out var letraMarcada);

            var acertou = letraMarcada is not null
                          && letraCorreta is not null
                          && string.Equals(letraMarcada, letraCorreta, StringComparison.OrdinalIgnoreCase);

            if (acertou) acertos++;
            correcao.Add(new CorrecaoQuestaoDto(q.IdQuestao, q.Ordem, letraMarcada, letraCorreta, acertou));
        }

        var total = prova.Questoes.Count;
        var nota = total == 0 ? 0m : Math.Round(acertos * 10m / total, 2);
        var aprovado = prova.Aprovado(nota);

        var numeroTentativa = await _desempenhos.ContarTentativasAsync(idEstudante, idProva, ct) + 1;
        IReadOnlyList<string> medalhasNovas = Array.Empty<string>();

        await _uow.ExecutarEmTransacaoAsync(async () =>
        {
            _desempenhos.Registrar(new Desempenho
            {
                IdEstudante = idEstudante,
                IdProva = idProva,
                Nota = nota,
                DataRealizacao = DateTime.UtcNow,
                Tentativas = numeroTentativa
            });
            await _uow.SalvarAsync(ct);

            // Medalhas so depois da tentativa gravada: as regras contam o que
            // esta no banco, nao o que esta em memoria.
            medalhasNovas = await _conquistas.AvaliarAsync(idEstudante, ct);
            await _uow.SalvarAsync(ct);

            // SEGUNDO GATILHO DA CONCLUSAO DO CURSO.
            // Passar na prova pode ter sido a ultima condicao que faltava --
            // o aluno que conclui todos os materiais ANTES de fazer a prova
            // chega aqui com tudo pronto menos a aprovacao. Sem esta chamada o
            // certificado nunca era emitido nessa ordem, que e a ordem natural
            // de quem estuda antes de ser avaliado.
            //
            // Dentro da mesma transacao de proposito: tentativa, medalhas,
            // conclusao e certificado valem juntos ou nao valem.
            if (aprovado) await _conclusao.AvaliarPorProvaAsync(idEstudante, idProva, ct);
            await _uow.SalvarAsync(ct);
        }, ct);

        var xpGanho = aprovado
            ? RegrasGamificacao.XpPorProvaAprovada + (nota >= 10m ? RegrasGamificacao.BonusNotaMaxima : 0)
            : 0;

        return new ResultadoTentativaDto(
            idProva, acertos, total, nota, prova.NotaMinima, aprovado,
            numeroTentativa, xpGanho, medalhasNovas, correcao);
    }
}
