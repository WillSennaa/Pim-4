using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Suporte;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>View da fila de suporte tecnico: lista a esquerda, chamado selecionado a direita.</summary>
public partial class TelaChamados : UserControl, ISuporteView
{
    private readonly SuportePresenter _presenter;

    public TelaChamados(SuporteService servico, SessaoAdmin sessao)
    {
        InitializeComponent();
        cmbSituacao.DisplayMember = nameof(OpcaoFiltro.Rotulo);
        cmbSituacao.DataSource = new List<OpcaoFiltro>
        {
            new(SuportePresenter.FiltroAbertos, "Abertos e em andamento"),
            new(SuportePresenter.FiltroResolvidos, "Resolvidos"),
            new(null, "Todos")
        };
        _presenter = new SuportePresenter(this, servico, sessao);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- ISuporteView ----------------

    public string? FiltroSituacao => (cmbSituacao.SelectedItem as OpcaoFiltro)?.Valor;
    public string TermoBusca => txtBusca.Text;
    public int? ChamadoSelecionado => dgvChamados.Selecionado<LinhaChamado>()?.Id;
    public string Resposta => txtResposta.Text;

    public void ExibirChamados(IReadOnlyList<LinhaChamado> linhas, string resumo)
    {
        if (IsDisposed) return;
        var selecionado = ChamadoSelecionado;
        dgvChamados.DataSource = linhas.ToList();
        if (selecionado is int id) dgvChamados.SelecionarOnde<LinhaChamado>(l => l.Id == id);
        lblResumo.Text = resumo;
    }

    public void Selecionar(int idChamado)
    {
        if (!IsDisposed) dgvChamados.SelecionarOnde<LinhaChamado>(l => l.Id == idChamado);
    }

    public void ExibirDetalhe(DetalheChamado? d)
    {
        if (IsDisposed) return;

        pnlConteudoDetalhe.Visible = d is not null;
        lblSemSelecao.Visible = d is null;
        if (d is null) return;

        lblTitulo.Text = d.Titulo;
        lblMeta.Text = $"Aberto por {d.Remetente} em {d.AbertoEm}";
        lblSituacao.Text = d.Situacao.ToUpperInvariant();
        lblSituacao.ForeColor = CorDaSituacao(d.Situacao);
        txtDescricao.Text = d.Descricao;

        btnAssumir.Visible = d.PodeAssumir;
        btnResponder.Enabled = d.PodeResponder;
        txtResposta.Enabled = d.PodeResponder;
        lblResposta.Text = d.PodeResponder ? "Resposta ao usuário" : "Chamado resolvido: não há resposta pendente.";
    }

    public void LimparResposta() => txtResposta.Clear();
    public void FocarResposta() => txtResposta.Focus();

    public bool Confirmar(string mensagem, string titulo)
        => MessageBox.Show(FindForm(), mensagem, titulo, MessageBoxButtons.YesNo,
               MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public void Informar(string mensagem)
        => MessageBox.Show(FindForm(), mensagem, "Suporte", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnAtualizar.Enabled = !ocupado;
        btnAtualizar.Text = ocupado ? "Atualizando..." : "Atualizar";
        faixaErro.BotaoHabilitado = !ocupado;
        pnlConteudoDetalhe.Enabled = !ocupado;
    }

    /// <summary>
    /// Erro de carga vai para a faixa; erro de acao (resposta curta, chamado
    /// ja resolvido por outro admin) tambem: a faixa fica no topo, visivel de
    /// qualquer ponto da tela.
    /// </summary>
    public void MostrarErro(string mensagem) { if (!IsDisposed) faixaErro.Mostrar(mensagem); }
    public void OcultarErro() { if (!IsDisposed) faixaErro.Ocultar(); }

    // ---------------- eventos ----------------

    private async void btnAtualizar_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void faixaErro_TentarNovamente(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void btnAssumir_Click(object? sender, EventArgs e) => await _presenter.AssumirAsync();
    private async void btnResponder_Click(object? sender, EventArgs e) => await _presenter.ResponderAsync();

    private void cmbSituacao_SelectedIndexChanged(object? sender, EventArgs e) => _presenter?.Filtrar();
    private void txtBusca_TextChanged(object? sender, EventArgs e) => _presenter.Filtrar();
    private void dgvChamados_SelectionChanged(object? sender, EventArgs e) => _presenter?.SelecaoMudou();

    private void dgvChamados_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewRow linha in dgvChamados.Rows)
        {
            if (linha.DataBoundItem is not LinhaChamado c) continue;
            var celula = linha.Cells[colSituacao.Index];
            celula.Style.ForeColor = CorDaSituacao(c.Situacao);
            celula.Style.Font = Fontes.PequenaNegrito;
            if (c.Situacao == StatusChamado.Resolvido) linha.DefaultCellStyle.ForeColor = Tema.TextoSuave;
        }
    }

    /// <summary>Mesmas cores do web: aberto em vermelho, em andamento em amarelo, resolvido em verde.</summary>
    private static Color CorDaSituacao(string situacao) => situacao switch
    {
        StatusChamado.Aberto => Tema.Perigo,
        StatusChamado.EmAndamento => Tema.Alerta,
        StatusChamado.Resolvido => Tema.Sucesso,
        _ => Tema.Neutro
    };
}
