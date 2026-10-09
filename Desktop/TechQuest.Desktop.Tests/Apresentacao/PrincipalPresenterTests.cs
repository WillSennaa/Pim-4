using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Apresentacao;

public class PrincipalPresenterTests
{
    private static (PrincipalPresenter presenter, PrincipalViewFalsa tela, SessaoAdmin sessao) Montar()
        => Montar(new ContadorDePendencias());

    private static (PrincipalPresenter presenter, PrincipalViewFalsa tela, SessaoAdmin sessao) Montar(
        ContadorDePendencias contador)
    {
        var sessao = new SessaoAdmin();
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin));
        var autenticacao = new AutenticacaoService(
            new AutenticacaoGatewayFalso(() => Fabrica.Resposta(PapelUsuario.Admin)), sessao);
        var tela = new PrincipalViewFalsa();
        return (new PrincipalPresenter(tela, sessao, autenticacao, contador), tela, sessao);
    }

    [Fact]
    public void Abre_no_Painel_com_as_iniciais_do_nome()
    {
        var (presenter, tela, _) = Montar();

        presenter.Iniciar();

        Assert.Equal("AS", tela.Iniciais); // "Ana Souza", API sem iniciais
        Assert.Equal(new[] { SecaoAdmin.Painel }, tela.Exibidas);
    }

    [Fact]
    public void Clicar_na_secao_ja_aberta_nao_recarrega()
    {
        var (presenter, tela, _) = Montar();
        presenter.Iniciar();

        presenter.Navegar(SecaoAdmin.Usuarios);
        presenter.Navegar(SecaoAdmin.Usuarios);

        Assert.Equal(new[] { SecaoAdmin.Painel, SecaoAdmin.Usuarios }, tela.Exibidas);
        Assert.Equal("Usuários", tela.Titulo);
    }

    [Fact]
    public void Sessao_expirada_avisa_e_volta_ao_login()
    {
        var (presenter, tela, sessao) = Montar();
        presenter.Iniciar();

        sessao.Expirar();

        Assert.Equal(1, tela.AvisosDeExpiracao);
        Assert.Equal(SaidaPrincipal.VoltarAoLogin, tela.Saida);
    }

    [Fact]
    public void Sair_cancelado_mantem_a_sessao()
    {
        var (presenter, tela, sessao) = Montar();
        tela.RespostaConfirmacao = false;

        presenter.Sair();

        Assert.True(sessao.Ativa);
        Assert.Null(tela.Saida);
    }

    [Fact]
    public void Sair_confirmado_encerra_a_sessao_e_volta_ao_login()
    {
        var (presenter, tela, sessao) = Montar();

        presenter.Sair();

        Assert.False(sessao.Ativa);
        Assert.Equal(SaidaPrincipal.VoltarAoLogin, tela.Saida);
    }

    [Fact]
    public void Presenter_descartado_deixa_de_ouvir_a_sessao()
    {
        var (presenter, tela, sessao) = Montar();

        presenter.Dispose();
        sessao.Expirar();

        Assert.Equal(0, tela.AvisosDeExpiracao);
    }

    [Fact]
    public void Selo_de_aprovacoes_acompanha_o_contador()
    {
        var contador = new ContadorDePendencias();
        var (presenter, tela, _) = Montar(contador);
        presenter.Iniciar();
        Assert.Null(tela.Selo); // ninguem leu ainda: sem numero

        contador.Informar(3);
        Assert.Equal(3, tela.Selo);

        contador.Informar(0);
        Assert.Equal(0, tela.Selo);
    }

    [Fact]
    public void Presenter_descartado_deixa_de_atualizar_o_selo()
    {
        var contador = new ContadorDePendencias();
        var (presenter, tela, _) = Montar(contador);
        presenter.Iniciar();

        presenter.Dispose();
        contador.Informar(5);

        Assert.Null(tela.Selo);
    }
}
