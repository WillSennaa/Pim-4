using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Cursos;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.UI.Estilo;
using TechQuest.Desktop.UI.Formularios;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>View do catalogo de cursos: filtro por estado, busca e abertura da revisao.</summary>
public partial class TelaCursos : UserControl, ICursosView
{
    private readonly CursosPresenter _presenter;
    private readonly AvaliacaoCursosService _servico;
    private readonly SessaoAdmin _sessao;

    /// <summary>
    /// Trava os eventos de filtro enquanto a propria tela reescreve a lista de
    /// opcoes: sem isso, preencher o ComboBox dispararia SelectedIndexChanged e
    /// o presenter filtraria no meio da atualizacao.
    /// </summary>
    private bool _atualizandoFiltros;

    public TelaCursos(AvaliacaoCursosService servico, SessaoAdmin sessao)
    {
        InitializeComponent();
        _servico = servico;
        _sessao = sessao;
        _presenter = new CursosPresenter(this, servico, sessao);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- ICursosView ----------------

    public string? StatusFiltro => (cmbStatus.SelectedItem as OpcaoFiltro)?.Valor;
    public string TermoBusca => txtBusca.Text;
    public int? CursoSelecionado => dgvCursos.Selecionado<LinhaCurso>()?.IdCurso;

    public void ExibirFiltros(IReadOnlyList<OpcaoFiltro> opcoes)
    {
        if (IsDisposed) return;
        var anterior = StatusFiltro;

        _atualizandoFiltros = true;
        try
        {
            cmbStatus.DisplayMember = nameof(OpcaoFiltro.Rotulo);
            cmbStatus.DataSource = opcoes.ToList();
            // Mantem o estado escolhido depois de recarregar (as contagens mudam,
            // a escolha do admin nao).
            cmbStatus.SelectedItem = opcoes.FirstOrDefault(o => o.Valor == anterior) ?? opcoes[0];
        }
        finally
        {
            _atualizandoFiltros = false;
        }
    }

    public void ExibirCursos(IReadOnlyList<LinhaCurso> linhas, string resumo)
    {
        if (IsDisposed) return;
        var selecionado = CursoSelecionado;
        dgvCursos.DataSource = linhas.ToList();
        if (selecionado is int id) dgvCursos.SelecionarOnde<LinhaCurso>(l => l.IdCurso == id);
        lblResumo.Text = resumo;
        btnAbrir.Enabled = linhas.Count > 0;
    }

    public void AbrirRevisao(int idCurso)
    {
        using var revisao = new FormRevisaoCurso(_servico, _sessao, idCurso);
        revisao.ShowDialog(FindForm());
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnAtualizar.Enabled = !ocupado;
        btnAtualizar.Text = ocupado ? "Atualizando..." : "Atualizar";
        faixaErro.BotaoHabilitado = !ocupado;
    }

    public void MostrarErro(string mensagem) { if (!IsDisposed) faixaErro.Mostrar(mensagem); }
    public void OcultarErro() { if (!IsDisposed) faixaErro.Ocultar(); }

    // ---------------- eventos ----------------

    private async void btnAtualizar_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void faixaErro_TentarNovamente(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void btnAbrir_Click(object? sender, EventArgs e) => await _presenter.AbrirSelecionadoAsync();

    private void cmbStatus_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_atualizandoFiltros) _presenter.Filtrar();
    }

    private void txtBusca_TextChanged(object? sender, EventArgs e) => _presenter.Filtrar();

    private void dgvCursos_SelectionChanged(object? sender, EventArgs e)
        => btnAbrir.Enabled = CursoSelecionado is not null;

    private async void dgvCursos_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0) await _presenter.AbrirSelecionadoAsync();
    }

    private async void dgvCursos_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.Handled = true;
        await _presenter.AbrirSelecionadoAsync();
    }

    /// <summary>Estado com a cor do selo do web; incompletos em alerta.</summary>
    private void dgvCursos_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewRow linha in dgvCursos.Rows)
        {
            if (linha.DataBoundItem is not LinhaCurso c) continue;

            var celula = linha.Cells[colStatus.Index];
            celula.Style.ForeColor = c.Status switch
            {
                StatusCurso.Pendente => Tema.Alerta,
                StatusCurso.Publicado => Tema.Sucesso,
                StatusCurso.Rejeitado => Tema.Perigo,
                _ => Tema.Neutro
            };
            celula.Style.Font = Fontes.PequenaNegrito;

            if (c.Incompleto)
            {
                linha.Cells[colAulas.Index].Style.ForeColor = Tema.Alerta;
                linha.Cells[colProva.Index].Style.ForeColor = Tema.Alerta;
            }
        }
    }
}
