using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Painel;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Apresentacao;

public class PainelPresenterTests
{
    private static ResumoAdmin Resumo(int total = 1234, int ativos = 925, int pendentes = 3, int chamados = 7)
        => new(total, ativos, 1180, 50, 12, pendentes, 2048, 310, chamados);

    private static (PainelPresenter presenter, PainelViewFalsa tela, NavegadorFalso navegador, SessaoAdmin sessao) Montar(
        Func<ResumoAdmin> resposta)
    {
        var sessao = new SessaoAdmin();
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin));
        var tela = new PainelViewFalsa();
        var navegador = new NavegadorFalso();
        var presenter = new PainelPresenter(
            tela, new PainelService(new PainelGatewayFalso(resposta), new ContadorDePendencias()), navegador, sessao, new RelogioFalso());
        return (presenter, tela, navegador, sessao);
    }

    [Fact]
    public async Task Exibe_os_numeros_com_separador_de_milhar_brasileiro()
    {
        var (presenter, tela, _, _) = Montar(() => Resumo());

        await presenter.CarregarAsync();

        var d = tela.Exibido!;
        Assert.Equal("1.234", d.TotalUsuarios);
        Assert.Equal("2.048", d.Matriculas);
        Assert.Equal("7", d.ChamadosAbertos);
        Assert.Equal(new[] { true, false }, tela.Ocupado);
    }

    [Theory]
    [InlineData(925, 1234, "75% da base")]   // 74,96 -> 75
    [InlineData(1, 8, "13% da base")]        // 12,5 -> 13, como o Math.round do web
    [InlineData(5, 5, "100% da base")]
    [InlineData(0, 0, "sem usuários cadastrados")]
    public async Task Percentual_de_ativos_arredonda_como_o_web(int ativos, int total, string esperado)
    {
        var (presenter, tela, _, _) = Montar(() => Resumo(total, ativos));

        await presenter.CarregarAsync();

        Assert.Equal(esperado, tela.Exibido!.DetalheAtivos);
    }

    [Theory]
    [InlineData(0, false, "Nenhuma aprovação pendente")]
    [InlineData(1, true, "1 curso aguardando avaliação")]
    [InlineData(3, true, "3 cursos aguardando avaliação")]
    public async Task Atalho_de_aprovacoes_usa_singular_e_plural(int pendentes, bool alerta, string titulo)
    {
        var (presenter, tela, _, _) = Montar(() => Resumo(pendentes: pendentes));

        await presenter.CarregarAsync();

        Assert.Equal(titulo, tela.Exibido!.TituloAprovacoes);
        Assert.Equal(alerta, tela.Exibido.HaAprovacoesPendentes);
    }

    [Fact]
    public async Task Mostra_a_hora_da_atualizacao()
    {
        var (presenter, tela, _, _) = Montar(() => Resumo());

        await presenter.CarregarAsync();

        Assert.StartsWith("Atualizado às ", tela.Exibido!.AtualizadoEm);
    }

    [Fact]
    public async Task Falha_mostra_o_motivo_e_nao_apaga_os_numeros_anteriores()
    {
        var falhar = false;
        var (presenter, tela, _, _) = Montar(() => falhar
            ? throw new ServidorIndisponivelException("Servidor fora.")
            : Resumo());

        await presenter.CarregarAsync();
        falhar = true;
        await presenter.CarregarAsync();

        Assert.Equal("Servidor fora.", tela.Erro);
        Assert.Equal(1, tela.VezesExibido);          // nao exibiu de novo
        Assert.Equal("1.234", tela.Exibido!.TotalUsuarios); // continua o ultimo valor bom
    }

    [Fact]
    public async Task Nova_tentativa_esconde_o_erro_anterior()
    {
        var (presenter, tela, _, _) = Montar(() => Resumo());

        await presenter.CarregarAsync();

        Assert.Equal(1, tela.ErrosOcultados);
        Assert.Null(tela.Erro);
    }

    [Fact]
    public async Task Token_recusado_pela_API_expira_a_sessao()
    {
        var (presenter, tela, _, sessao) = Montar(() => throw new NaoAutenticadoException("x"));

        await presenter.CarregarAsync();

        Assert.False(sessao.Ativa);
        Assert.Null(tela.Erro); // quem avisa e a janela principal, nao o painel
    }

    [Fact]
    public async Task Resumo_informa_o_contador_do_selo_do_menu()
    {
        var contador = new ContadorDePendencias();
        var sessao = new SessaoAdmin();
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin));
        var presenter = new PainelPresenter(new PainelViewFalsa(),
            new PainelService(new PainelGatewayFalso(() => Resumo(pendentes: 4)), contador),
            new NavegadorFalso(), sessao);

        await presenter.CarregarAsync();

        Assert.Equal(4, contador.CursosPendentes);
    }

    [Fact]
    public void Atalho_leva_a_secao_de_aprovacoes()
    {
        var (presenter, _, navegador, _) = Montar(() => Resumo());

        presenter.IrParaAprovacoes();

        Assert.Equal(new[] { SecaoAdmin.Aprovacoes }, navegador.Pedidos);
    }
}
