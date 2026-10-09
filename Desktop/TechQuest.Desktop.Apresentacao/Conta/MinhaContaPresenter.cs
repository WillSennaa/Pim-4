using System.Globalization;
using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Usuarios;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Conta;

public interface IMinhaContaView : IViewBase
{
    string SenhaAtual { get; }
    string SenhaNova { get; }
    string ConfirmacaoSenha { get; }
    string EmailNovo { get; }
    string SenhaParaEmail { get; }

    void ExibirPerfil(PerfilExibicao perfil);
    void LimparCamposSenha();
    void LimparCamposEmail();

    /// <summary>Mensagem de sucesso (o erro usa MostrarErro).</summary>
    void Informar(string mensagem);
    void OcultarMensagem();
}

public sealed record PerfilExibicao(string Nome, string Email, string Perfil, string Cadastro, string Contato);

/// <summary>Minha conta: dados do admin logado, troca de senha e de e-mail.</summary>
public sealed class MinhaContaPresenter : PresenterBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IMinhaContaView _view;
    private readonly ContaService _servico;
    private readonly TimeZoneInfo? _fuso;
    private PerfilUsuario? _perfil;

    public MinhaContaPresenter(IMinhaContaView view, ContaService servico, SessaoAdmin sessao, TimeZoneInfo? fuso = null)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
        _fuso = fuso;
    }

    public async Task CarregarAsync()
    {
        PerfilUsuario? p = null;
        if (!await ExecutarAsync(async () => p = await _servico.ObterPerfilAsync())) return;
        _perfil = p!;
        _view.ExibirPerfil(Montar(_perfil));
    }

    public async Task TrocarSenhaAsync()
    {
        _view.OcultarMensagem();

        ResultadoOperacao? r = null;
        if (!await ExecutarAsync(async () =>
                r = await _servico.TrocarSenhaAsync(_view.SenhaAtual, _view.SenhaNova, _view.ConfirmacaoSenha)))
            return;

        if (!r!.Sucesso) { _view.MostrarErro(r.Mensagem!); return; }

        _view.LimparCamposSenha();
        _view.Informar("Senha alterada. Use a nova senha no próximo login, aqui e no site.");
    }

    public async Task TrocarEmailAsync()
    {
        _view.OcultarMensagem();

        ResultadoOperacao<string>? r = null;
        if (!await ExecutarAsync(async () =>
                r = await _servico.TrocarEmailAsync(_view.EmailNovo, _view.SenhaParaEmail)))
            return;

        if (!r!.Sucesso) { _view.MostrarErro(r.Mensagem!); return; }

        _view.LimparCamposEmail();
        if (_perfil is not null)
        {
            _perfil = _perfil with { Email = r.Valor };
            _view.ExibirPerfil(Montar(_perfil));
        }
        _view.Informar($"E-mail alterado para {r.Valor}. A partir de agora, o login é feito com ele.");
    }

    internal PerfilExibicao Montar(PerfilUsuario p)
    {
        var contato = string.Join(" · ", new[] { p.Telefone, p.Cidade }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return new PerfilExibicao(
            p.Nome,
            p.Email ?? "-",
            UsuariosPresenter.NomeDoPapel(p.Papel),
            "Conta criada em " + HorarioServidor.ParaLocal(p.DataCadastro, _fuso).ToString("dd/MM/yyyy", PtBr),
            contato.Length == 0 ? "Telefone e cidade não informados" : contato);
    }
}
