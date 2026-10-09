using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Login;

namespace TechQuest.Desktop.UI.Formularios;

/// <summary>
/// View do login no MVP. Este arquivo so desenha e repassa: nenhum "if" de
/// regra de negocio mora aqui. Toda decisao esta em LoginPresenter.
/// </summary>
public partial class FormLogin : Form, ILoginView
{
    private readonly LoginPresenter _presenter;

    public FormLogin(AutenticacaoService autenticacao, SessaoAdmin sessao)
    {
        InitializeComponent();
        _presenter = new LoginPresenter(this, autenticacao, sessao);
    }

    // ---------------- ILoginView ----------------

    public string Email => txtEmail.Text;
    public string Senha => txtSenha.Text;

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;

        UseWaitCursor = ocupado;
        txtEmail.Enabled = !ocupado;
        txtSenha.Enabled = !ocupado;
        btnEntrar.Enabled = !ocupado;
        btnEntrar.Text = ocupado ? "Entrando..." : "Entrar";
        if (ocupado) lblErro.Visible = false;
    }

    public void MostrarErro(string mensagem)
    {
        lblErro.Text = mensagem;
        lblErro.Visible = true;
    }

    public void LimparSenha()
    {
        txtSenha.Clear();
        txtSenha.Focus();
    }

    public void ConcluirLogin()
    {
        DialogResult = DialogResult.OK; // Program.cs le isto e abre a principal
        Close();
    }

    // ---------------- eventos ----------------

    /// <summary>
    /// async void e o unico formato aceito por um handler de evento. Excecao
    /// que escape daqui chega ao Application.ThreadException de Program.cs.
    /// </summary>
    private async void btnEntrar_Click(object? sender, EventArgs e)
        => await _presenter.EntrarAsync();
}
