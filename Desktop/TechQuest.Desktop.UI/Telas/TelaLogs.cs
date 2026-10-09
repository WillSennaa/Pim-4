using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Logs;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>View da trilha de auditoria: quantidade, busca e lista.</summary>
public partial class TelaLogs : UserControl, ILogsView
{
    private readonly LogsPresenter _presenter;
    private bool _iniciando = true;

    public TelaLogs(AuditoriaService servico, SessaoAdmin sessao)
    {
        InitializeComponent();
        cmbLimite.DataSource = AuditoriaService.LimitesPermitidos.ToList();
        _presenter = new LogsPresenter(this, servico, sessao);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // So aqui o ComboBox ja esta na janela e tem os itens da lista
        // vinculada; escolher no construtor nao teria efeito.
        cmbLimite.SelectedItem = 100; // o mesmo padrao do endpoint
        _iniciando = false;
        await _presenter.CarregarAsync();
    }

    // ---------------- ILogsView ----------------

    public int Limite => cmbLimite.SelectedItem is int n ? n : 100;
    public string TermoBusca => txtBusca.Text;

    public void ExibirLogs(IReadOnlyList<LinhaLog> linhas, string resumo)
    {
        if (IsDisposed) return;
        dgvLogs.DataSource = linhas.ToList();
        lblResumo.Text = resumo;
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnAtualizar.Enabled = !ocupado;
        cmbLimite.Enabled = !ocupado;
        btnAtualizar.Text = ocupado ? "Atualizando..." : "Atualizar";
        faixaErro.BotaoHabilitado = !ocupado;
    }

    public void MostrarErro(string mensagem) { if (!IsDisposed) faixaErro.Mostrar(mensagem); }
    public void OcultarErro() { if (!IsDisposed) faixaErro.Ocultar(); }

    // ---------------- eventos ----------------

    private async void btnAtualizar_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void faixaErro_TentarNovamente(object? sender, EventArgs e) => await _presenter.CarregarAsync();

    /// <summary>Mudar a quantidade busca de novo; a busca por texto filtra o que ja veio.</summary>
    private async void cmbLimite_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_iniciando) await _presenter.CarregarAsync();
    }

    private void txtBusca_TextChanged(object? sender, EventArgs e) => _presenter.Filtrar();
}
