using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Usuarios;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.UI.Estilo;
using TechQuest.Desktop.UI.Formularios;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>View da gestao de usuarios: filtros, busca, ativar/desativar e novo.</summary>
public partial class TelaUsuarios : UserControl, IUsuariosView
{
    private readonly UsuariosPresenter _presenter;
    private readonly UsuariosService _servico;
    private readonly SessaoAdmin _sessao;

    public TelaUsuarios(UsuariosService servico, SessaoAdmin sessao)
    {
        InitializeComponent();
        _servico = servico;
        _sessao = sessao;

        // Opcoes fixas: o valor interno e o texto do servidor ("Admin"), o
        // rotulo e o que o admin le ("Administradores").
        cmbPapel.DisplayMember = nameof(OpcaoFiltro.Rotulo);
        cmbPapel.DataSource = new List<OpcaoFiltro>
        {
            new(null, "Todos"),
            new(nameof(PapelUsuario.Estudante), "Estudantes"),
            new(nameof(PapelUsuario.Tutor), "Tutores"),
            new(nameof(PapelUsuario.Admin), "Administradores")
        };
        cmbSituacao.DisplayMember = nameof(OpcaoFiltro.Rotulo);
        cmbSituacao.DataSource = new List<OpcaoFiltro>
        {
            new(null, "Todas"),
            new("ativo", "Ativos"),
            new("inativo", "Inativos")
        };

        _presenter = new UsuariosPresenter(this, servico, sessao);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- IUsuariosView ----------------

    public string? PapelFiltro => (cmbPapel.SelectedItem as OpcaoFiltro)?.Valor;

    public bool? SituacaoFiltro => (cmbSituacao.SelectedItem as OpcaoFiltro)?.Valor switch
    {
        "ativo" => true,
        "inativo" => false,
        _ => null
    };

    public string TermoBusca => txtBusca.Text;
    public int? UsuarioSelecionado => dgvUsuarios.Selecionado<LinhaUsuario>()?.Id;

    public void ExibirUsuarios(IReadOnlyList<LinhaUsuario> linhas, string resumo)
    {
        if (IsDisposed) return;
        var selecionado = UsuarioSelecionado;
        dgvUsuarios.DataSource = linhas.ToList();
        if (selecionado is int id) dgvUsuarios.SelecionarOnde<LinhaUsuario>(l => l.Id == id);
        lblResumo.Text = resumo;
    }

    public void Selecionar(int idUsuario)
    {
        if (!IsDisposed) dgvUsuarios.SelecionarOnde<LinhaUsuario>(l => l.Id == idUsuario);
    }

    public void DefinirAcaoStatus(string texto, bool habilitada, string? motivoBloqueio)
    {
        if (IsDisposed) return;
        btnStatus.Text = texto;
        btnStatus.Enabled = habilitada;
        // Desativar em vermelho, reativar em verde: a cor antecipa o efeito.
        btnStatus.ForeColor = !habilitada ? Tema.TextoSuave
                            : texto.StartsWith("Desativar") ? Tema.Perigo : Tema.Sucesso;
        lblBloqueio.Text = motivoBloqueio ?? "";
    }

    public bool Confirmar(string mensagem, string titulo)
        => MessageBox.Show(FindForm(), mensagem, titulo, MessageBoxButtons.YesNo,
               MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public UsuarioAdmin? AbrirCadastro()
    {
        using var cadastro = new FormCadastroUsuario(_servico, _sessao);
        return cadastro.ShowDialog(FindForm()) == DialogResult.OK ? cadastro.Criado : null;
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnAtualizar.Enabled = !ocupado;
        btnNovo.Enabled = !ocupado;
        btnAtualizar.Text = ocupado ? "Atualizando..." : "Atualizar";
        faixaErro.BotaoHabilitado = !ocupado;
        if (ocupado) btnStatus.Enabled = false;
        // Ao terminar, quem reabilita o btnStatus e o presenter (SelecaoMudou),
        // porque depende de QUEM esta selecionado.
    }

    public void MostrarErro(string mensagem) { if (!IsDisposed) faixaErro.Mostrar(mensagem); }
    public void OcultarErro() { if (!IsDisposed) faixaErro.Ocultar(); }

    // ---------------- eventos ----------------

    private async void btnAtualizar_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void faixaErro_TentarNovamente(object? sender, EventArgs e) => await _presenter.CarregarAsync();
    private async void btnNovo_Click(object? sender, EventArgs e) => await _presenter.NovoUsuarioAsync();
    private async void btnStatus_Click(object? sender, EventArgs e) => await _presenter.AlternarStatusSelecionadoAsync();

    private void filtro_Changed(object? sender, EventArgs e) => _presenter?.Filtrar();
    private void txtBusca_TextChanged(object? sender, EventArgs e) => _presenter.Filtrar();
    private void dgvUsuarios_SelectionChanged(object? sender, EventArgs e) => _presenter?.SelecaoMudou();

    /// <summary>Contas inativas em cinza; a propria conta em negrito.</summary>
    private void dgvUsuarios_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        foreach (DataGridViewRow linha in dgvUsuarios.Rows)
        {
            if (linha.DataBoundItem is not LinhaUsuario u) continue;

            if (!u.Ativo) linha.DefaultCellStyle.ForeColor = Tema.TextoSuave;
            if (u.EhVoce) linha.DefaultCellStyle.Font = Fontes.Negrito;

            var situacao = linha.Cells[colSituacao.Index];
            situacao.Style.ForeColor = u.Ativo ? Tema.Sucesso : Tema.Neutro;
            situacao.Style.Font = Fontes.PequenaNegrito;
        }
    }
}
