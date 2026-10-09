using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Medalhas;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.UI.Estilo;
using TechQuest.Desktop.UI.Formularios;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>View do catalogo de medalhas.</summary>
public partial class TelaMedalhas : UserControl, IMedalhasView
{
    private readonly MedalhasPresenter _presenter;
    private readonly MedalhasService _servico;
    private readonly SessaoAdmin _sessao;
    private bool _atualizandoFiltros;

    public TelaMedalhas(MedalhasService servico, SessaoAdmin sessao)
    {
        InitializeComponent();
        _servico = servico;
        _sessao = sessao;
        _presenter = new MedalhasPresenter(this, servico, sessao);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- IMedalhasView ----------------

    public string? FiltroRaridade => (cmbRaridade.SelectedItem as OpcaoFiltro)?.Valor;
    public string TermoBusca => txtBusca.Text;
    public int? MedalhaSelecionada => dgvMedalhas.Selecionado<LinhaMedalha>()?.Id;

    public void ExibirFiltros(IReadOnlyList<OpcaoFiltro> opcoes)
    {
        if (IsDisposed) return;
        var anterior = FiltroRaridade;
        _atualizandoFiltros = true;
        try
        {
            cmbRaridade.DisplayMember = nameof(OpcaoFiltro.Rotulo);
            cmbRaridade.DataSource = opcoes.ToList();
            cmbRaridade.SelectedItem = opcoes.FirstOrDefault(o => o.Valor == anterior) ?? opcoes[0];
        }
        finally
        {
            _atualizandoFiltros = false;
        }
    }

    public void ExibirMedalhas(IReadOnlyList<LinhaMedalha> linhas, string resumo)
    {
        if (IsDisposed) return;
        var selecionada = MedalhaSelecionada;
        dgvMedalhas.DataSource = linhas.ToList();
        if (selecionada is int id) dgvMedalhas.SelecionarOnde<LinhaMedalha>(l => l.Id == id);
        lblResumo.Text = resumo;
    }

    public void Selecionar(int idMedalha)
    {
        if (!IsDisposed) dgvMedalhas.SelecionarOnde<LinhaMedalha>(l => l.Id == idMedalha);
    }

    public void DefinirEdicaoDisponivel(bool disponivel)
    {
        if (!IsDisposed) btnEditar.Enabled = disponivel;
    }

    public MedalhaAdmin? AbrirEditor(MedalhaAdmin? existente)
    {
        using var editor = new FormMedalha(_servico, _sessao, existente);
        return editor.ShowDialog(FindForm()) == DialogResult.OK ? editor.Salva : null;
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnAtualizar.Enabled = !ocupado;
        btnNova.Enabled = !ocupado;
        btnAtualizar.Text = ocupado ? "Atualizando..." : "Atualizar";
        faixaErro.BotaoHabilitado = !ocupado;
    }

    public void MostrarErro(string mensagem) { if (!IsDisposed) faixaErro.Mostrar(mensagem); }
    public void OcultarErro() { if (!IsDisposed) faixaErro.Ocultar(); }

    // ---------------- eventos ----------------

    private async void btnAtualizar_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void faixaErro_TentarNovamente(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void btnNova_Click(object? sender, EventArgs e) => await _presenter.NovaAsync();
    private async void btnEditar_Click(object? sender, EventArgs e) => await _presenter.EditarSelecionadaAsync();

    private async void dgvMedalhas_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0) await _presenter.EditarSelecionadaAsync();
    }

    private void cmbRaridade_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_atualizandoFiltros) _presenter?.Filtrar();
    }

    private void txtBusca_TextChanged(object? sender, EventArgs e) => _presenter.Filtrar();
    private void dgvMedalhas_SelectionChanged(object? sender, EventArgs e) => _presenter?.SelecaoMudou();

    /// <summary>Raridade com cor propria, da mais comum (cinza) a mais rara (ambar).</summary>
    private void dgvMedalhas_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewRow linha in dgvMedalhas.Rows)
        {
            if (linha.DataBoundItem is not LinhaMedalha m) continue;
            var celula = linha.Cells[colRaridade.Index];
            celula.Style.Font = Fontes.PequenaNegrito;
            celula.Style.ForeColor = m.Raridade switch
            {
                "Rara" => Tema.Primaria,
                "Épica" => Color.FromArgb(124, 58, 237),   // roxo
                "Lendária" => Tema.Alerta,
                _ => Tema.Neutro
            };
        }
    }
}
