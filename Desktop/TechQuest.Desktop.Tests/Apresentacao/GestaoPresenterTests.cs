using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Cursos;
using TechQuest.Desktop.Apresentacao.Logs;
using TechQuest.Desktop.Apresentacao.Usuarios;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Apresentacao;

internal static class Sessoes
{
    public static SessaoAdmin Admin(int id = 1)
    {
        var s = new SessaoAdmin();
        s.Iniciar(new RespostaLogin("t", DateTime.UtcNow.AddHours(1),
            new UsuarioLogado(id, "Admin Logado", "adm@tq.com", "AL", PapelUsuario.Admin)));
        return s;
    }
}

public class CursosPresenterTests
{
    private static (CursosPresenter presenter, CursosViewFalsa tela, CursosAdminGatewayFalso api) Montar()
    {
        var api = new CursosAdminGatewayFalso
        {
            Fila = new()
            {
                CursosDeTeste.Solicitacao(1, status: StatusCurso.Rascunho, nome: "Banco de Dados", tutor: "Ana"),
                CursosDeTeste.Solicitacao(2, status: StatusCurso.Publicado, nome: "Introdução ao C#", tutor: "João"),
                CursosDeTeste.Solicitacao(3, status: StatusCurso.Pendente, nome: "APIs REST", tutor: "Ana"),
                CursosDeTeste.Solicitacao(4, status: StatusCurso.Publicado, nome: "Algoritmos", tutor: "Bia",
                                          prova: false, questoes: 0)
            }
        };
        var tela = new CursosViewFalsa();
        var servico = new AvaliacaoCursosService(api, new ContadorDePendencias());
        return (new CursosPresenter(tela, servico, Sessoes.Admin()), tela, api);
    }

    [Fact]
    public async Task Lista_pendentes_primeiro_e_depois_por_nome()
    {
        var (presenter, tela, _) = Montar();

        await presenter.CarregarAsync();

        Assert.Equal(new[] { "APIs REST", "Algoritmos", "Introdução ao C#", "Banco de Dados" },
                     tela.Linhas!.Select(l => l.Curso));
        Assert.True(tela.Linhas!.Single(l => l.Curso == "Algoritmos").Incompleto);
        Assert.Equal("4 cursos.", tela.Resumo);
    }

    [Fact]
    public async Task Filtros_mostram_a_contagem_de_cada_estado()
    {
        var (presenter, tela, _) = Montar();

        await presenter.CarregarAsync();

        Assert.Equal(new[] { "Todos (4)", "Pendentes (1)", "Publicados (2)", "Rejeitados (0)", "Rascunhos (1)" },
                     tela.Filtros!.Select(f => f.Rotulo));
    }

    [Fact]
    public async Task Filtra_por_estado_e_busca_sem_acento_no_nome_e_no_tutor()
    {
        var (presenter, tela, _) = Montar();
        await presenter.CarregarAsync();

        tela.StatusFiltro = StatusCurso.Publicado;
        tela.TermoBusca = "joao";
        presenter.Filtrar();

        Assert.Equal("Introdução ao C#", Assert.Single(tela.Linhas!).Curso);
        Assert.Equal("Exibindo 1 de 4 cursos.", tela.Resumo);
    }

    [Fact]
    public async Task Abrir_curso_recarrega_a_lista_ao_fechar()
    {
        var (presenter, tela, api) = Montar();
        await presenter.CarregarAsync();
        var antes = api.Listagens;
        tela.CursoSelecionado = 2;

        await presenter.AbrirSelecionadoAsync();

        Assert.Equal(new[] { 2 }, tela.RevisoesAbertas);
        Assert.Equal(antes + 4, api.Listagens); // os quatro estados de novo
    }
}

public class UsuariosPresenterTests
{
    private static readonly DateTime Cadastro = new(2026, 3, 1, 2, 0, 0, DateTimeKind.Unspecified);

    private static (UsuariosPresenter presenter, UsuariosViewFalsa tela, UsuariosGatewayFalso api) Montar()
    {
        var api = new UsuariosGatewayFalso
        {
            Usuarios = new()
            {
                new(1, "Admin Logado", "adm@tq.com", "Admin", true, Cadastro),
                new(2, "Zé Tutor", "ze@tq.com", "Tutor", true, Cadastro),
                new(3, "Ana Aluna", "ana@tq.com", "Estudante", false, Cadastro),
                new(4, "Bruno Aluno", "bruno@tq.com", "Estudante", true, Cadastro)
            }
        };
        var sessao = Sessoes.Admin(id: 1);
        var tela = new UsuariosViewFalsa();
        var presenter = new UsuariosPresenter(tela, new UsuariosService(api, sessao), sessao, Fusos.Brasilia);
        return (presenter, tela, api);
    }

    [Fact]
    public async Task Lista_em_ordem_alfabetica_com_data_local_e_marca_a_propria_conta()
    {
        var (presenter, tela, _) = Montar();

        await presenter.CarregarAsync();

        Assert.Equal(new[] { "Admin Logado (você)", "Ana Aluna", "Bruno Aluno", "Zé Tutor" },
                     tela.Linhas!.Select(l => l.Nome));
        // 01/03 as 02h UTC ainda e 28/02 em Brasilia.
        Assert.Equal("28/02/2026", tela.Linhas![0].Cadastro);
        Assert.Equal("Administrador", tela.Linhas[0].Papel);
        Assert.Equal("4 usuários · 3 ativos", tela.Resumo);
    }

    [Fact]
    public async Task Filtra_por_perfil_situacao_e_busca()
    {
        var (presenter, tela, _) = Montar();
        await presenter.CarregarAsync();

        tela.PapelFiltro = "Estudante";
        tela.SituacaoFiltro = true;
        presenter.Filtrar();
        Assert.Equal("Bruno Aluno", Assert.Single(tela.Linhas!).Nome);

        tela.PapelFiltro = null;
        tela.SituacaoFiltro = null;
        tela.TermoBusca = "ze";
        presenter.Filtrar();
        Assert.Equal("Zé Tutor", Assert.Single(tela.Linhas!).Nome);
    }

    [Fact]
    public async Task Botao_de_status_acompanha_a_selecao()
    {
        var (presenter, tela, _) = Montar();
        await presenter.CarregarAsync();

        tela.UsuarioSelecionado = 1; presenter.SelecaoMudou();
        Assert.Equal(("Desativar conta", false, "Você não pode desativar a própria conta."), tela.Acao);

        tela.UsuarioSelecionado = 3; presenter.SelecaoMudou();
        Assert.Equal(("Reativar conta", true, (string?)null), tela.Acao);

        tela.UsuarioSelecionado = 2; presenter.SelecaoMudou();
        Assert.Equal(("Desativar conta", true, (string?)null), tela.Acao);
    }

    [Fact]
    public async Task Desativar_confirma_envia_e_mantem_a_selecao()
    {
        var (presenter, tela, api) = Montar();
        await presenter.CarregarAsync();
        tela.UsuarioSelecionado = 2;

        await presenter.AlternarStatusSelecionadoAsync();

        Assert.Contains("pode ser reativada", tela.Confirmacoes.Single());
        Assert.Equal((2, false), api.MudancasDeStatus.Single());
        Assert.Equal(2, tela.UsuarioSelecionado);
        Assert.Equal("Inativo", tela.Linhas!.Single(l => l.Id == 2).Situacao);
    }

    [Fact]
    public async Task Cancelar_a_confirmacao_nao_muda_nada()
    {
        var (presenter, tela, api) = Montar();
        await presenter.CarregarAsync();
        tela.UsuarioSelecionado = 2;
        tela.RespostaConfirmacao = false;

        await presenter.AlternarStatusSelecionadoAsync();

        Assert.Empty(api.MudancasDeStatus);
    }

    [Fact]
    public async Task Novo_usuario_recarrega_e_seleciona_a_conta_criada()
    {
        var (presenter, tela, api) = Montar();
        await presenter.CarregarAsync();
        var novo = new UsuarioAdmin(9, "Carla Nova", "carla@tq.com", "Tutor", true, DateTime.UtcNow);
        api.Usuarios.Add(novo);
        tela.ResultadoCadastro = novo;

        await presenter.NovoUsuarioAsync();

        Assert.Equal(9, tela.UsuarioSelecionado);
        Assert.Contains(tela.Linhas!, l => l.Nome == "Carla Nova");
    }
}

public class CadastroUsuarioPresenterTests
{
    [Fact]
    public void Ao_abrir_ja_sugere_uma_senha_gerada()
    {
        var tela = new CadastroViewFalsa();
        var sessao = Sessoes.Admin();
        new CadastroUsuarioPresenter(tela, new UsuariosService(new UsuariosGatewayFalso(), sessao), sessao).Iniciar();

        Assert.Equal(12, tela.Senha.Length);
    }

    [Fact]
    public async Task Cria_a_conta_e_devolve_a_senha_para_a_tela_exibir()
    {
        var api = new UsuariosGatewayFalso();
        var sessao = Sessoes.Admin();
        var tela = new CadastroViewFalsa
        {
            Nome = "Carla Nova", Email = "carla@tq.com", Senha = "inicial123", Papel = PapelUsuario.Admin
        };

        await new CadastroUsuarioPresenter(tela, new UsuariosService(api, sessao), sessao).SalvarAsync();

        Assert.Equal("Admin", api.Criados.Single().Papel);
        Assert.Equal("inicial123", tela.Conclusao!.Value.Senha);
        Assert.Null(tela.Erro);
    }

    [Fact]
    public async Task Email_repetido_mostra_a_mensagem_do_servidor()
    {
        var api = new UsuariosGatewayFalso
        {
            FalhaAoCriar = new TechQuest.Desktop.Aplicacao.Erros.OperacaoRecusadaException(
                "Ja existe um usuario com este e-mail.")
        };
        var sessao = Sessoes.Admin();
        var tela = new CadastroViewFalsa { Nome = "Carla Nova", Email = "carla@tq.com", Senha = "inicial123" };

        await new CadastroUsuarioPresenter(tela, new UsuariosService(api, sessao), sessao).SalvarAsync();

        Assert.Equal("Ja existe um usuario com este e-mail.", tela.Erro);
        Assert.Null(tela.Conclusao);
    }
}

public class LogsPresenterTests
{
    private static (LogsPresenter presenter, LogsViewFalsa tela, AuditoriaGatewayFalso api) Montar(int quantidade)
    {
        var api = new AuditoriaGatewayFalso
        {
            Logs = Enumerable.Range(1, quantidade).Select(i => new LogAuditoria(
                i, 1, i % 2 == 0 ? "Ana Admin" : null, $"Ação número {i}",
                new DateTime(2026, 10, 9, 12, 0, 0).AddMinutes(i))).ToList()
        };
        var tela = new LogsViewFalsa();
        return (new LogsPresenter(tela, new AuditoriaService(api), Sessoes.Admin(), Fusos.Brasilia), tela, api);
    }

    [Fact]
    public async Task Mais_recente_primeiro_com_horario_de_Brasilia()
    {
        var (presenter, tela, _) = Montar(3);

        await presenter.CarregarAsync();

        Assert.Equal("09/10/2026 09:03", tela.Linhas![0].Data); // 12:03 UTC
        Assert.Equal("Ação número 3", tela.Linhas[0].Acao);
        Assert.Equal("administrador nº 1", tela.Linhas[0].Administrador); // sem nome na API
        Assert.Equal("Todos os 3 registros.", tela.Resumo);
    }

    [Fact]
    public async Task Pede_a_quantidade_escolhida_e_avisa_quando_ha_mais()
    {
        var (presenter, tela, api) = Montar(80);
        tela.Limite = 50;

        await presenter.CarregarAsync();

        Assert.Equal(new[] { 50 }, api.LimitesPedidos);
        Assert.Equal("Os 50 registros mais recentes.", tela.Resumo);
    }

    [Fact]
    public async Task Busca_sem_acento_filtra_o_que_ja_foi_carregado()
    {
        var (presenter, tela, api) = Montar(12);
        await presenter.CarregarAsync();

        tela.TermoBusca = "acao numero 1";
        presenter.Filtrar();

        Assert.Equal(4, tela.Linhas!.Count); // 1, 10, 11, 12
        Assert.Equal("4 de 12 registros correspondem à busca.", tela.Resumo);
        Assert.Single(api.LimitesPedidos); // nao buscou de novo
    }
}
