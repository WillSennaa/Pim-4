using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Aprovacoes;
using TechQuest.Desktop.UI.Estilo;
using TechQuest.Desktop.UI.Formularios;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>View da fila de aprovacoes: grade de cursos pendentes.</summary>
public partial class TelaAprovacoes : UserControl, IAprovacoesView
{
    private readonly AprovacoesPresenter _presenter;
    private readonly AvaliacaoCursosService _servico;
    private readonly SessaoAdmin _sessao;

    public TelaAprovacoes(AvaliacaoCursosService servico, SessaoAdmin sessao)
    {
        InitializeComponent();
        _servico = servico;
        _sessao = sessao;
        _presenter = new AprovacoesPresenter(this, servico, sessao);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- IAprovacoesView ----------------

    public int? CursoSelecionado
        => dgvFila.CurrentRow?.DataBoundItem is LinhaSolicitacao linha ? linha.IdCurso : null;

    public void ExibirFila(IReadOnlyList<LinhaSolicitacao> linhas, string resumo)
    {
        if (IsDisposed) return;

        lblResumo.Text = resumo;
        dgvFila.DataSource = linhas.ToList();

        var vazia = linhas.Count == 0;
        lblVazio.Visible = vazia;
        dgvFila.Visible = !vazia;
        btnRevisar.Enabled = !vazia;
    }

    public void AbrirRevisao(int idCurso)
    {
        // A janela de revisao e montada aqui, na UI, e nao pelo presenter:
        // criar janelas e assunto de Windows Forms. O presenter so decide
        // QUANDO abrir e o que fazer depois que ela fecha.
        using var revisao = new FormRevisaoCurso(_servico, _sessao, idCurso);
        revisao.ShowDialog(FindForm());
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnAtualizar.Enabled = !ocupado;
        btnTentarNovamente.Enabled = !ocupado;
        btnRevisar.Enabled = !ocupado && dgvFila.Rows.Count > 0;
        btnAtualizar.Text = ocupado ? "Atualizando..." : "Atualizar";
    }

    public void MostrarErro(string mensagem)
    {
        if (IsDisposed) return;
        lblFalha.Text = mensagem;
        pnlFalha.Visible = true;
    }

    public void OcultarErro()
    {
        if (IsDisposed) return;
        pnlFalha.Visible = false;
    }

    // ---------------- eventos ----------------

    private async void btnAtualizar_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();

    private async void btnRevisar_Click(object? sender, EventArgs e) => await _presenter.RevisarSelecionadoAsync();

    private async void dgvFila_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return; // duplo clique no cabecalho nao e um curso
        await _presenter.RevisarSelecionadoAsync();
    }

    private async void dgvFila_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.Handled = true; // sem isto, o Enter tambem desce para a proxima linha
        await _presenter.RevisarSelecionadoAsync();
    }

    /// <summary>Curso incompleto (sem aula, sem prova) ganha a coluna Prova/Aulas em alerta.</summary>
    private void dgvFila_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewRow linha in dgvFila.Rows)
        {
            if (linha.DataBoundItem is not LinhaSolicitacao { Incompleto: true }) continue;
            linha.Cells[colProva.Index].Style.ForeColor = Tema.Alerta;
            linha.Cells[colAulas.Index].Style.ForeColor = Tema.Alerta;
        }
    }
}
