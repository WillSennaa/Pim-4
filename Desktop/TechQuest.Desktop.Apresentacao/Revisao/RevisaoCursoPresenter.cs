using System.Globalization;
using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Regras;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Revisao;

/// <summary>
/// Revisao completa de um curso: le, avalia e decide.
///
/// A MESMA JANELA SERVE PARA AUDITORIA: aberta para um curso que nao esta
/// Pendente (publicado, rejeitado), mostra o conteudo sem os botoes de
/// avaliacao. A secao "Cursos" da proxima entrega reaproveita esta revisao.
/// </summary>
public sealed class RevisaoCursoPresenter : PresenterBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IRevisaoCursoView _view;
    private readonly AvaliacaoCursosService _servico;
    private readonly int _idCurso;

    private CursoRevisao? _curso;
    private IReadOnlyList<string> _pontos = Array.Empty<string>();

    public RevisaoCursoPresenter(
        IRevisaoCursoView view, AvaliacaoCursosService servico, SessaoAdmin sessao, int idCurso)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
        _idCurso = idCurso;
    }

    public async Task CarregarAsync()
    {
        CursoRevisao? curso = null;
        if (!await ExecutarAsync(async () => curso = await _servico.ObterRevisaoAsync(_idCurso))) return;

        _curso = curso!;
        _pontos = AnaliseDeRevisao.PontosDeAtencao(_curso);
        _view.Exibir(Montar(_curso, _pontos));
    }

    public async Task AprovarAsync()
    {
        if (_curso is null || !StatusCurso.EhPendente(_curso.Status)) return;

        // Publicar e irreversivel (Publicado e estado final no servidor), por
        // isso a confirmacao repete as consequencias e, se houver, os pontos
        // de atencao. Quem aprova com pendencia faz isso sabendo.
        var mensagem = _pontos.Count == 0
            ? $"Publicar \"{_curso.Nome}\"?\n\n" +
              "O curso ficará visível para todos os estudantes e não poderá voltar a rascunho."
            : $"\"{_curso.Nome}\" tem {_pontos.Count} " +
              (_pontos.Count == 1 ? "ponto de atenção" : "pontos de atenção") + ":\n\n" +
              string.Join("\n", _pontos.Select(p => "• " + p)) +
              "\n\nPublicar mesmo assim? O curso não poderá voltar a rascunho.";

        if (!_view.Confirmar(mensagem, "Aprovar e publicar")) return;

        if (await ExecutarAsync(() => _servico.AprovarAsync(_curso.IdCurso)))
            _view.Concluir($"\"{_curso.Nome}\" foi publicado e já aparece para os estudantes.");
    }

    public async Task RejeitarAsync()
    {
        if (_curso is null || !StatusCurso.EhPendente(_curso.Status)) return;

        // A validacao roda ANTES da confirmacao: perguntar "tem certeza?" e
        // depois recusar o motivo seria pedir duas vezes a mesma coisa.
        var motivo = _view.MotivoRejeicao;
        var previa = AvaliacaoCursosService.ValidarMotivo(motivo);
        if (!previa.Sucesso)
        {
            _view.MostrarErro(previa.Mensagem!);
            _view.FocarMotivo();
            return;
        }

        if (!_view.Confirmar(
                $"Rejeitar \"{_curso.Nome}\"?\n\n" +
                "O curso volta ao tutor, que recebe o seu motivo como mensagem e pode corrigir e reenviar.",
                "Rejeitar curso"))
            return;

        ResultadoOperacao? resultado = null;
        if (!await ExecutarAsync(async () => resultado = await _servico.RejeitarAsync(_curso.IdCurso, motivo))) return;

        if (resultado!.Sucesso)
            _view.Concluir($"\"{_curso.Nome}\" voltou ao tutor com o seu parecer.");
        else
            _view.MostrarErro(resultado.Mensagem!);
    }

    internal static RevisaoExibicao Montar(CursoRevisao c, IReadOnlyList<string> pontos)
    {
        var materiais = c.Materiais.Select((m, i) => new MaterialExibicao(
            Titulo: $"{i + 1}. {(string.IsNullOrWhiteSpace(m.Titulo) ? "(sem título)" : m.Titulo)}",
            Tipo: string.IsNullOrWhiteSpace(m.Tipo) ? "material" : m.Tipo!,
            SemConteudo: string.IsNullOrWhiteSpace(m.Conteudo),
            Blocos: FormatadorConteudo.Blocos(m.Conteudo))).ToList();

        ProvaExibicao? prova = null;
        if (c.Prova is { } p)
        {
            var nQuestoes = p.Questoes.Count == 1 ? "1 questão" : $"{p.Questoes.Count} questões";
            prova = new ProvaExibicao(
                Titulo: string.IsNullOrWhiteSpace(p.Titulo) ? "Prova" : p.Titulo!,
                Resumo: $"Nota mínima {p.NotaMinima.ToString("0.0", PtBr)} · {p.TempoMinutos} minutos · {nQuestoes}",
                Questoes: p.Questoes.OrderBy(q => q.Ordem).Select(q => new QuestaoExibicao(
                    Cabecalho: $"Questão {q.Ordem}",
                    Enunciado: q.Enunciado,
                    Codigo: string.IsNullOrWhiteSpace(q.CodigoExemplo) ? null : q.CodigoExemplo,
                    SemGabarito: !q.Alternativas.Any(a => a.EhCorreta),
                    Alternativas: q.Alternativas.Select(a => new AlternativaExibicao(
                        a.Letra ?? "?", a.Texto ?? "", a.EhCorreta)).ToList())).ToList());
        }

        return new RevisaoExibicao(
            Nome: c.Nome,
            Status: c.Status,
            PodeAvaliar: StatusCurso.EhPendente(c.Status),
            Descricao: string.IsNullOrWhiteSpace(c.Descricao) ? "Sem descrição." : c.Descricao!,
            Tutor: c.Tutor ?? "não identificado",
            Categoria: c.Categoria ?? "-",
            Nivel: c.Nivel ?? "-",
            CargaHoraria: c.DuracaoHoras is int h ? (h == 1 ? "1 hora" : $"{h} horas") : "não informada",
            PontosDeAtencao: pontos,
            TituloAbaMateriais: $"Materiais ({c.Materiais.Count})",
            Materiais: materiais,
            TituloAbaProva: c.Prova is null ? "Prova (ausente)" : $"Prova ({c.Prova.Questoes.Count})",
            Prova: prova);
    }
}
