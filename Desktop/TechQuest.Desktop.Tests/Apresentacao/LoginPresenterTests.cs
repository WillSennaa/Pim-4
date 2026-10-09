using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Login;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Apresentacao;

/// <summary>
/// Testa a tela de login inteira sem abrir janela: o presenter recebe uma
/// tela falsa e um gateway falso, e o teste confere o que foi pedido a tela.
/// </summary>
public class LoginPresenterTests
{
    private static async Task<(LoginViewFalsa tela, SessaoAdmin sessao, AutenticacaoGatewayFalso gateway)> Entrar(
        string email, string senha, Func<RespostaLogin> respostaDaApi)
    {
        var sessao = new SessaoAdmin();
        var gateway = new AutenticacaoGatewayFalso(respostaDaApi);
        var tela = new LoginViewFalsa { Email = email, Senha = senha };

        await new LoginPresenter(tela, new AutenticacaoService(gateway, sessao), sessao).EntrarAsync();

        return (tela, sessao, gateway);
    }

    [Fact]
    public async Task Admin_entra_e_a_sessao_fica_ativa()
    {
        var (tela, sessao, _) = await Entrar("  admin123@gmail.com ", "admin123@",
            () => Fabrica.Resposta(PapelUsuario.Admin));

        Assert.True(tela.Concluiu);
        Assert.True(sessao.Ativa);
        Assert.Equal(new[] { true, false }, tela.Ocupado); // travou e destravou
    }

    [Theory]
    [InlineData(PapelUsuario.Tutor)]
    [InlineData(PapelUsuario.Estudante)]
    public async Task Quem_nao_e_admin_e_recusado_e_o_token_e_descartado(PapelUsuario papel)
    {
        var (tela, sessao, _) = await Entrar("tutor123@gmail.com", "tutor123@",
            () => Fabrica.Resposta(papel));

        Assert.False(tela.Concluiu);
        Assert.False(sessao.Ativa);
        Assert.Null(sessao.Token);
        Assert.Contains("exclusiva para administradores", tela.Erro);
    }

    [Fact]
    public async Task Senha_errada_mostra_mensagem_generica_e_limpa_a_senha()
    {
        var (tela, _, _) = await Entrar("a@a.com", "errada",
            () => throw new NaoAutenticadoException("qualquer"));

        Assert.Equal("E-mail ou senha inválidos.", tela.Erro);
        Assert.True(tela.SenhaLimpa);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a@a.com", "")]
    [InlineData("   ", "senha")]
    [InlineData("sem-arroba", "senha")]
    public async Task Entrada_invalida_nao_chega_a_chamar_a_API(string email, string senha)
    {
        var (tela, _, gateway) = await Entrar(email, senha, () => Fabrica.Resposta(PapelUsuario.Admin));

        Assert.Equal(0, gateway.Chamadas);
        Assert.False(tela.Concluiu);
        Assert.NotNull(tela.Erro);
    }

    [Fact]
    public async Task Servidor_fora_mostra_o_motivo_e_preserva_a_senha()
    {
        var (tela, _, _) = await Entrar("a@a.com", "x",
            () => throw new ServidorIndisponivelException("Servidor fora."));

        Assert.Equal("Servidor fora.", tela.Erro);
        Assert.False(tela.SenhaLimpa); // nao foi culpa da senha
        Assert.Equal(new[] { true, false }, tela.Ocupado);
    }
}
