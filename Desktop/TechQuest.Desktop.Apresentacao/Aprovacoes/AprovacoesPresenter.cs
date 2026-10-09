using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Aprovacoes;

/// <summary>
/// Fila de cursos pendentes. A avaliacao em si acontece na janela de
/// revisao (RevisaoCursoPresenter); esta tela lista e abre.
/// </summary>
public sealed class AprovacoesPresenter : PresenterBase
{
    private readonly IAprovacoesView _view;
    private readonly AvaliacaoCursosService _servico;

    public AprovacoesPresenter(IAprovacoesView view, AvaliacaoCursosService servico, SessaoAdmin sessao)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
    }

    public async Task CarregarAsync()
    {
        _view.OcultarErro();

        IReadOnlyList<SolicitacaoCurso>? fila = null;
        if (!await ExecutarAsync(async () => fila = await _servico.ListarPendentesAsync())) return;

        _view.ExibirFila(fila!.Select(Linha).ToList(), Resumo(fila!.Count));
    }

    /// <summary>
    /// Abre a revisao e, ao fechar, RECARREGA a fila sempre, tenha o admin
    /// decidido ou nao. Enquanto a janela estava aberta, outro administrador
    /// pode ter avaliado um curso da lista; a recarga e barata e evita
    /// mostrar como pendente o que ja nao esta.
    /// </summary>
    public async Task RevisarSelecionadoAsync()
    {
        if (_view.CursoSelecionado is not int id) return;
        _view.AbrirRevisao(id);
        await CarregarAsync();
    }

    internal static LinhaSolicitacao Linha(SolicitacaoCurso c) => new(
        c.IdCurso,
        c.Nome,
        c.Tutor ?? "não identificado",
        c.Categoria ?? "-",
        c.Nivel ?? "-",
        FormatacaoCurso.Aulas(c.TotalAulas),
        FormatacaoCurso.Prova(c.TemProva, c.TotalQuestoes),
        Incompleto: FormatacaoCurso.Incompleto(c));

    internal static string Resumo(int total) => total switch
    {
        0 => "Nenhum curso aguardando avaliação.",
        1 => "1 curso aguardando avaliação.",
        _ => $"{total} cursos aguardando avaliação."
    };
}
