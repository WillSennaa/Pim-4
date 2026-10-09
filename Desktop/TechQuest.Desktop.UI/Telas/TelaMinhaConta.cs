using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Conta;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>View de Minha conta: dados do admin e os formularios de troca de senha e e-mail.</summary>
public partial class TelaMinhaConta : UserControl, IMinhaContaView
{
    private readonly MinhaContaPresenter _presenter;

    public TelaMinhaConta(ContaService servico, SessaoAdmin sessao)
    {
        InitializeComponent();
        _presenter = new MinhaContaPresenter(this, servico, sessao);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- IMinhaContaView ----------------

    public string SenhaAtual => txtSenhaAtual.Text;
    public string SenhaNova => txtSenhaNova.Text;
    public string ConfirmacaoSenha => txtConfirmacao.Text;
    public string EmailNovo => txtEmailNovo.Text;
    public string SenhaParaEmail => txtSenhaEmail.Text;

    public void ExibirPerfil(PerfilExibicao p)
    {
        if (IsDisposed) return;
        lblNome.Text = p.Nome;
        lblIniciais.Text = PrincipalPresenter.IniciaisDe(p.Nome);
        lblEmail.Text = p.Email;
        lblDetalhes.Text = $"{p.Perfil}   ·   {p.Cadastro}   ·   {p.Contato}";
    }

    /// <summary>
    /// Os campos de senha sao limpos depois do sucesso: senha nao deve ficar
    /// visivel numa tela que pode passar o dia aberta.
    /// </summary>
    public void LimparCamposSenha()
    {
        txtSenhaAtual.Clear();
        txtSenhaNova.Clear();
        txtConfirmacao.Clear();
    }

    public void LimparCamposEmail()
    {
        txtEmailNovo.Clear();
        txtSenhaEmail.Clear();
    }

    public void Informar(string mensagem)
    {
        if (IsDisposed) return;
        lblMensagem.ForeColor = Tema.Sucesso;
        lblMensagem.Text = mensagem;
    }

    public void MostrarErro(string mensagem)
    {
        if (IsDisposed) return;
        lblMensagem.ForeColor = Tema.Perigo;
        lblMensagem.Text = mensagem;
    }

    public void OcultarMensagem()
    {
        if (!IsDisposed) lblMensagem.Text = "";
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        grpSenha.Enabled = !ocupado;
        grpEmail.Enabled = !ocupado;
    }

    // ---------------- eventos ----------------

    private async void btnTrocarSenha_Click(object? sender, EventArgs e) => await _presenter.TrocarSenhaAsync();
    private async void btnTrocarEmail_Click(object? sender, EventArgs e) => await _presenter.TrocarEmailAsync();
}
