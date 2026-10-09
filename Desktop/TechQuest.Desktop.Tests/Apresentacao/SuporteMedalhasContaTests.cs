using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Apresentacao.Conta;
using TechQuest.Desktop.Apresentacao.Medalhas;
using TechQuest.Desktop.Apresentacao.Suporte;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Apresentacao;

public class SuportePresenterTests
{
    private static readonly DateTime Base = new(2026, 10, 9, 12, 0, 0);

    private static (SuportePresenter presenter, SuporteViewFalsa tela, ChamadosGatewayFalso api) Montar()
    {
        var api = new ChamadosGatewayFalso
        {
            Chamados = new()
            {
                ChamadosGatewayFalso.Novo(1, StatusChamado.Resolvido, Base.AddDays(-3)),
                ChamadosGatewayFalso.Novo(2, StatusChamado.Aberto, Base.AddHours(-1), "Video nao carrega"),
                ChamadosGatewayFalso.Novo(3, StatusChamado.EmAndamento, Base.AddDays(-1), "Certificado", "Bruno"),
                ChamadosGatewayFalso.Novo(4, StatusChamado.Aberto, Base.AddDays(-2), "Prova travou")
            }
        };
        var tela = new SuporteViewFalsa();
        return (new SuportePresenter(tela, new SuporteService(api), Sessoes.Admin(), Fusos.Brasilia), tela, api);
    }

    [Fact]
    public async Task Fila_padrao_mostra_abertos_primeiro_do_mais_antigo_e_esconde_resolvidos()
    {
        var (presenter, tela, _) = Montar();

        await presenter.CarregarAsync();

        Assert.Equal(new[] { 4, 2, 3 }, tela.Linhas!.Select(l => l.Id));
        Assert.Equal("2 abertos · 1 em andamento · exibindo 3 de 4", tela.Resumo);
    }

    [Fact]
    public async Task Busca_encontra_pelo_numero_do_chamado()
    {
        var (presenter, tela, _) = Montar();
        await presenter.CarregarAsync();

        tela.FiltroSituacao = null;
        tela.TermoBusca = "#1";
        presenter.Filtrar();

        Assert.Equal(1, Assert.Single(tela.Linhas!).Id);
    }

    [Fact]
    public async Task Detalhe_libera_assumir_so_para_chamado_aberto()
    {
        var (presenter, tela, _) = Montar();
        await presenter.CarregarAsync();

        tela.ChamadoSelecionado = 2; presenter.SelecaoMudou();
        Assert.True(tela.Detalhe!.PodeAssumir);
        Assert.Equal("09/10/2026 08:00", tela.Detalhe.AbertoEm); // 11:00 UTC

        tela.ChamadoSelecionado = 3; presenter.SelecaoMudou();
        Assert.False(tela.Detalhe!.PodeAssumir);
        Assert.True(tela.Detalhe.PodeResponder);

        tela.ChamadoSelecionado = null; presenter.SelecaoMudou();
        Assert.Null(tela.Detalhe);
    }

    [Fact]
    public async Task Assumir_marca_em_andamento_e_mantem_o_chamado_selecionado()
    {
        var (presenter, tela, api) = Montar();
        await presenter.CarregarAsync();
        tela.ChamadoSelecionado = 2;

        await presenter.AssumirAsync();

        Assert.Equal((2, StatusChamado.EmAndamento), api.MudancasDeStatus.Single());
        Assert.Equal(StatusChamado.EmAndamento, tela.Detalhe!.Situacao);
    }

    [Fact]
    public async Task Resposta_curta_e_barrada_antes_da_confirmacao()
    {
        var (presenter, tela, api) = Montar();
        await presenter.CarregarAsync();
        tela.ChamadoSelecionado = 2;
        tela.Resposta = "ok";

        await presenter.ResponderAsync();

        Assert.Empty(tela.Confirmacoes);
        Assert.True(tela.RespostaFocada);
        Assert.Empty(api.Respostas);
    }

    [Fact]
    public async Task Responder_envia_resolve_e_limpa_o_campo()
    {
        var (presenter, tela, api) = Montar();
        await presenter.CarregarAsync();
        tela.ChamadoSelecionado = 4;
        tela.Resposta = "  Limpe o cache do navegador e tente de novo.  ";

        await presenter.ResponderAsync();

        Assert.Equal((4, "Limpe o cache do navegador e tente de novo."), api.Respostas.Single());
        Assert.True(tela.RespostaLimpa);
        Assert.NotNull(tela.Informacao);
        Assert.DoesNotContain(tela.Linhas!, l => l.Id == 4); // saiu da fila de abertos
    }
}

public class MedalhasTests
{
    private static MedalhasGatewayFalso Catalogo() => new()
    {
        Medalhas = new()
        {
            new(1, "Nota Dez", "epico", "Tire 10", 2),
            new(2, "Primeiro Passo", "comum", "Conclua um material", 15),
            new(3, "Nivel 10", "lendario", null, 0),
            new(4, "Criada pela API", "Comum", null, 1) // grafia que o servidor usa quando vem vazia
        }
    };

    [Fact]
    public async Task Lista_da_mais_comum_para_a_mais_rara_reconhecendo_Comum_maiusculo()
    {
        var api = Catalogo();
        var tela = new MedalhasViewFalsa();
        var p = new MedalhasPresenter(tela, new MedalhasService(api), Sessoes.Admin());

        await p.CarregarAsync();

        Assert.Equal(new[] { "Criada pela API", "Primeiro Passo", "Nota Dez", "Nivel 10" }, tela.Linhas!.Select(l => l.Nome));
        Assert.Equal("Comum", tela.Linhas![0].Raridade);
        Assert.Equal("nenhuma ainda", tela.Linhas[3].Conquistas);
        Assert.Equal("Comuns (2)", tela.Filtros![1].Rotulo);
    }

    [Fact]
    public async Task Filtra_por_raridade()
    {
        var tela = new MedalhasViewFalsa();
        var p = new MedalhasPresenter(tela, new MedalhasService(Catalogo()), Sessoes.Admin());
        await p.CarregarAsync();

        tela.FiltroRaridade = Raridades.Epico;
        p.Filtrar();

        Assert.Equal("Nota Dez", Assert.Single(tela.Linhas!).Nome);
    }

    [Fact]
    public async Task Nova_medalha_sempre_envia_raridade_na_grafia_do_catalogo()
    {
        var api = new MedalhasGatewayFalso();
        var tela = new MedalhaEditorViewFalsa();
        var editor = new MedalhaEditorPresenter(tela, new MedalhasService(api), Sessoes.Admin(), null);
        editor.Iniciar();
        tela.Nome = "  Persistente ";
        tela.Raridade = "RARO";

        await editor.SalvarAsync();

        Assert.Equal(new DadosMedalha("Persistente", "raro", null), api.Criadas.Single());
        Assert.Empty(tela.Confirmacoes);
        Assert.NotNull(tela.Salva);
    }

    [Fact]
    public async Task Renomear_pede_confirmacao_e_cancelar_nao_salva()
    {
        var api = Catalogo();
        var tela = new MedalhaEditorViewFalsa { RespostaConfirmacao = false };
        var editor = new MedalhaEditorPresenter(tela, new MedalhasService(api), Sessoes.Admin(), api.Medalhas[1]);
        editor.Iniciar();
        tela.Nome = "Primeiros Passos";

        await editor.SalvarAsync();

        Assert.Contains("concedidas automaticamente pelo nome", tela.Confirmacoes.Single());
        Assert.Empty(api.Atualizadas);
    }

    [Fact]
    public async Task Editar_so_a_descricao_nao_pede_confirmacao()
    {
        var api = Catalogo();
        var tela = new MedalhaEditorViewFalsa();
        var editor = new MedalhaEditorPresenter(tela, new MedalhasService(api), Sessoes.Admin(), api.Medalhas[1]);
        editor.Iniciar();
        tela.Nome = "primeiro passo "; // so caixa e espaco: nao e renomear
        tela.Descricao = "Conclua o primeiro material de qualquer curso";

        await editor.SalvarAsync();

        Assert.Empty(tela.Confirmacoes);
        Assert.Equal(2, api.Atualizadas.Single().Id);
        Assert.Contains("15 estudantes", tela.Aviso);
    }

    [Fact]
    public async Task Nome_vazio_ou_longo_demais_nao_chega_a_API()
    {
        var api = new MedalhasGatewayFalso();
        var servico = new MedalhasService(api);

        Assert.False((await servico.SalvarAsync(null, "  ", Raridades.Comum, null)).Sucesso);
        Assert.False((await servico.SalvarAsync(null, new string('x', 51), Raridades.Comum, null)).Sucesso);
        Assert.False((await servico.SalvarAsync(null, "Ok", "mitica", null)).Sucesso);
        Assert.Empty(api.Criadas);
    }
}

public class MinhaContaTests
{
    private static (MinhaContaPresenter p, MinhaContaViewFalsa tela, ContaGatewayFalso api, TechQuest.Desktop.Aplicacao.Sessao.SessaoAdmin sessao) Montar()
    {
        var api = new ContaGatewayFalso();
        var sessao = Sessoes.Admin();
        var tela = new MinhaContaViewFalsa();
        return (new MinhaContaPresenter(tela, new ContaService(api, sessao), sessao, Fusos.Brasilia), tela, api, sessao);
    }

    [Fact]
    public async Task Exibe_o_perfil_com_data_local_e_contato()
    {
        var (p, tela, _, _) = Montar();

        await p.CarregarAsync();

        Assert.Equal("Administrador", tela.Perfil!.Perfil);
        Assert.Equal("Conta criada em 09/01/2026", tela.Perfil.Cadastro); // 10/01 01h UTC
        Assert.Equal("Sao Paulo", tela.Perfil.Contato);
    }

    [Theory]
    [InlineData("", "novaSenha1", "novaSenha1")]          // sem senha atual
    [InlineData("atual123", "12345", "12345")]             // nova curta
    [InlineData("atual123", "novaSenha1", "novaSenha2")]   // confirmacao diferente
    [InlineData("atual123", "atual123", "atual123")]       // igual a atual
    public async Task Troca_de_senha_invalida_nao_chega_a_API(string atual, string nova, string confirmacao)
    {
        var (p, tela, api, _) = Montar();
        tela.SenhaAtual = atual; tela.SenhaNova = nova; tela.ConfirmacaoSenha = confirmacao;

        await p.TrocarSenhaAsync();

        Assert.NotNull(tela.Erro);
        Assert.Empty(api.TrocasDeSenha);
        Assert.False(tela.SenhasLimpas);
    }

    [Fact]
    public async Task Troca_de_senha_valida_limpa_os_campos()
    {
        var (p, tela, api, _) = Montar();
        tela.SenhaAtual = "atual123"; tela.SenhaNova = "novaSenha1"; tela.ConfirmacaoSenha = "novaSenha1";

        await p.TrocarSenhaAsync();

        Assert.Equal(new TrocaSenha("atual123", "novaSenha1"), api.TrocasDeSenha.Single());
        Assert.True(tela.SenhasLimpas);
        Assert.NotNull(tela.Informacao);
    }

    [Fact]
    public async Task Troca_de_email_atualiza_a_sessao_e_o_perfil_exibido()
    {
        var (p, tela, api, sessao) = Montar();
        await p.CarregarAsync();
        tela.EmailNovo = " Novo.Admin@TQ.com "; tela.SenhaParaEmail = "atual123";

        await p.TrocarEmailAsync();

        Assert.Equal("Novo.Admin@TQ.com", api.TrocasDeEmail.Single().EmailNovo);
        Assert.Equal("novo.admin@tq.com", sessao.Usuario!.Email);
        Assert.Equal("novo.admin@tq.com", tela.Perfil!.Email);
        Assert.True(tela.EmailLimpo);
    }

    [Fact]
    public async Task Email_igual_ao_atual_e_recusado_sem_chamar_a_API()
    {
        var (p, tela, api, _) = Montar();
        tela.EmailNovo = "ADM@tq.com"; tela.SenhaParaEmail = "atual123";

        await p.TrocarEmailAsync();

        Assert.Equal("O novo e-mail é igual ao atual.", tela.Erro);
        Assert.Empty(api.TrocasDeEmail);
    }

    [Fact]
    public async Task Senha_atual_errada_mostra_a_mensagem_do_servidor_sem_expirar_a_sessao()
    {
        var api = new ContaGatewayFalsoQueRecusa();
        var sessao = Sessoes.Admin();
        var tela = new MinhaContaViewFalsa
        {
            SenhaAtual = "errada1", SenhaNova = "novaSenha1", ConfirmacaoSenha = "novaSenha1"
        };

        await new MinhaContaPresenter(tela, new ContaService(api, sessao), sessao).TrocarSenhaAsync();

        // O servidor responde 400 (nao 401) para senha atual errada: o admin
        // continua logado e ve o motivo.
        Assert.Equal("A senha atual esta incorreta.", tela.Erro);
        Assert.True(sessao.Ativa);
    }

    private sealed class ContaGatewayFalsoQueRecusa : TechQuest.Desktop.Aplicacao.Interfaces.IContaGateway
    {
        public Task<PerfilUsuario> ObterPerfilAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task TrocarSenhaAsync(TrocaSenha troca, CancellationToken ct = default)
            => throw new OperacaoRecusadaException("A senha atual esta incorreta.");
        public Task<string> TrocarEmailAsync(TrocaEmail troca, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
