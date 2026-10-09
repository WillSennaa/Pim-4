using System.Globalization;
using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Cursos;

public interface ICursosView : IViewBase
{
    /// <summary>Valor do filtro de estado escolhido (null = todos).</summary>
    string? StatusFiltro { get; }
    string TermoBusca { get; }
    int? CursoSelecionado { get; }

    /// <summary>Opcoes do filtro de estado, com a contagem de cada uma.</summary>
    void ExibirFiltros(IReadOnlyList<OpcaoFiltro> opcoes);

    void ExibirCursos(IReadOnlyList<LinhaCurso> linhas, string resumo);
    void OcultarErro();
    void AbrirRevisao(int idCurso);
}

public sealed record LinhaCurso(
    int IdCurso, string Curso, string Tutor, string Categoria, string Nivel,
    string Status, string Aulas, string Prova, bool Incompleto);

/// <summary>
/// Catalogo completo de cursos, nos quatro estados, para consulta e
/// auditoria. Abrir um curso usa a mesma janela de revisao das Aprovacoes:
/// se ele estiver Pendente, da para avaliar dali mesmo; nos outros estados,
/// a janela e somente leitura.
///
/// NAO HA EXCLUSAO NEM ARQUIVAMENTO aqui, e e de proposito: Publicado e
/// estado final no servidor (curso com aluno matriculado nao pode sumir,
/// RNF02), e a API nao tem rota para isso.
///
/// FILTRO E BUSCA SAO LOCAIS: a lista inteira chega numa carga, e filtrar na
/// memoria responde a cada tecla sem nova requisicao.
/// </summary>
public sealed class CursosPresenter : PresenterBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ICursosView _view;
    private readonly AvaliacaoCursosService _servico;
    private IReadOnlyList<SolicitacaoCurso> _todos = Array.Empty<SolicitacaoCurso>();

    public CursosPresenter(ICursosView view, AvaliacaoCursosService servico, SessaoAdmin sessao)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
    }

    public async Task CarregarAsync()
    {
        _view.OcultarErro();

        IReadOnlyList<SolicitacaoCurso>? cursos = null;
        if (!await ExecutarAsync(async () => cursos = await _servico.ListarTodosAsync())) return;

        _todos = cursos!
            .OrderBy(c => FormatacaoCurso.OrdemDoStatus(c.Status))
            .ThenBy(c => c.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        _view.ExibirFiltros(Filtros(_todos));
        Filtrar();
    }

    /// <summary>Reaplica estado e busca sobre a lista ja carregada.</summary>
    public void Filtrar()
    {
        var status = _view.StatusFiltro;
        var termo = _view.TermoBusca;

        var visiveis = _todos
            .Where(c => status is null || string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase))
            .Where(c => TextoBusca.Contem(termo, c.Nome, c.Tutor, c.Categoria))
            .Select(Linha)
            .ToList();

        _view.ExibirCursos(visiveis, Resumo(visiveis.Count, _todos.Count));
    }

    /// <summary>Abre a revisao e, ao fechar, recarrega: o curso pode ter mudado de estado.</summary>
    public async Task AbrirSelecionadoAsync()
    {
        if (_view.CursoSelecionado is not int id) return;
        _view.AbrirRevisao(id);
        await CarregarAsync();
    }

    internal static IReadOnlyList<OpcaoFiltro> Filtros(IReadOnlyList<SolicitacaoCurso> todos)
    {
        int Contar(string s) => todos.Count(c => string.Equals(c.Status, s, StringComparison.OrdinalIgnoreCase));
        return new[]
        {
            new OpcaoFiltro(null, $"Todos ({todos.Count})"),
            new OpcaoFiltro(StatusCurso.Pendente, $"Pendentes ({Contar(StatusCurso.Pendente)})"),
            new OpcaoFiltro(StatusCurso.Publicado, $"Publicados ({Contar(StatusCurso.Publicado)})"),
            new OpcaoFiltro(StatusCurso.Rejeitado, $"Rejeitados ({Contar(StatusCurso.Rejeitado)})"),
            new OpcaoFiltro(StatusCurso.Rascunho, $"Rascunhos ({Contar(StatusCurso.Rascunho)})")
        };
    }

    internal static LinhaCurso Linha(SolicitacaoCurso c) => new(
        c.IdCurso, c.Nome, c.Tutor ?? "não identificado", c.Categoria ?? "-", c.Nivel ?? "-",
        c.Status, FormatacaoCurso.Aulas(c.TotalAulas), FormatacaoCurso.Prova(c.TemProva, c.TotalQuestoes),
        FormatacaoCurso.Incompleto(c));

    internal static string Resumo(int visiveis, int total)
        => total == 0 ? "Nenhum curso cadastrado na plataforma."
         : visiveis == total ? (total == 1 ? "1 curso." : $"{total} cursos.")
         : $"Exibindo {visiveis} de {total} cursos.";
}
