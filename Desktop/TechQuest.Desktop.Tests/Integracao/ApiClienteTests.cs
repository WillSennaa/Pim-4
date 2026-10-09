using System.Net;
using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Integracao;
using TechQuest.Desktop.Integracao.Gateways;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Integracao;

/// <summary>
/// O ApiCliente e o ponto onde HTTP vira linguagem da aplicacao. Estes
/// testes travam o CONTRATO com a API real: formato do JSON, rota, cabecalho
/// e o significado de cada status.
/// </summary>
public class ApiClienteTests
{
    // Formato exato que a API devolve hoje, inclusive campos que o desktop ignora.
    private const string LoginAdminJson =
        "{\"token\":\"tok\",\"expiraEm\":\"2099-01-01T00:00:00Z\"," +
        "\"usuario\":{\"id\":1,\"nome\":\"Admin Geral\",\"email\":\"a@a.com\"," +
        "\"iniciais\":\"AG\",\"papel\":3,\"idEstudante\":null,\"xp\":0,\"nivel\":1}}";

    private static (ApiCliente api, HandlerFalso handler, SessaoAdmin sessao) Montar(
        HttpStatusCode status, string corpo)
    {
        var handler = new HandlerFalso(status, corpo);
        var sessao = new SessaoAdmin();
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.teste/") };
        return (new ApiCliente(http, sessao), handler, sessao);
    }

    [Fact]
    public async Task Login_le_papel_numerico_da_api_como_Admin()
    {
        var (api, _, _) = Montar(HttpStatusCode.OK, LoginAdminJson);

        var resposta = await new AutenticacaoGateway(api)
            .EntrarAsync(new CredenciaisLogin("a@a.com", "x"));

        Assert.Equal(PapelUsuario.Admin, resposta.Usuario.Papel);
        Assert.Equal("tok", resposta.Token);
    }

    [Fact]
    public async Task Login_envia_para_a_rota_certa_em_camelCase_e_sem_token()
    {
        var (api, handler, _) = Montar(HttpStatusCode.OK, LoginAdminJson);

        await new AutenticacaoGateway(api).EntrarAsync(new CredenciaisLogin("a@a.com", "x"));

        Assert.Equal("https://api.teste/api/auth/login", handler.Ultima!.RequestUri!.ToString());
        Assert.Equal("{\"email\":\"a@a.com\",\"senha\":\"x\"}", handler.CorpoEnviado);
        Assert.Null(handler.Ultima.Headers.Authorization);
    }

    [Fact]
    public async Task Resumo_le_o_JSON_da_API_na_rota_certa()
    {
        const string json =
            "{\"totalUsuarios\":10,\"usuariosAtivos\":8,\"estudantes\":7,\"tutores\":2," +
            "\"cursosPublicados\":4,\"cursosPendentes\":1,\"totalMatriculas\":15," +
            "\"certificadosEmitidos\":3,\"chamadosAbertos\":2}";
        var (api, handler, _) = Montar(HttpStatusCode.OK, json);

        var r = await new PainelGateway(api).ObterResumoAsync();

        Assert.Equal("https://api.teste/api/admin/resumo", handler.Ultima!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, handler.Ultima.Method);
        Assert.Equal(new ResumoAdmin(10, 8, 7, 2, 4, 1, 15, 3, 2), r);
    }

    [Fact]
    public async Task Fila_de_cursos_le_a_lista_e_escapa_o_status_na_URL()
    {
        const string json =
            "[{\"idCurso\":5,\"nome\":\"SQL\",\"descricao\":null,\"categoria\":\"Dados\"," +
            "\"nivel\":\"Iniciante\",\"tutor\":\"Ana\",\"status\":\"Pendente\"," +
            "\"totalAulas\":2,\"temProva\":true,\"totalQuestoes\":4}]";
        var (api, handler, _) = Montar(HttpStatusCode.OK, json);
        var gateway = new CursosAdminGateway(api);

        var lista = await gateway.ListarAsync(StatusCurso.Pendente);
        Assert.Equal("https://api.teste/api/admin/cursos?status=Pendente", handler.Ultima!.RequestUri!.ToString());
        Assert.Equal("SQL", Assert.Single(lista).Nome);

        await gateway.ListarAsync("Em revisao&x");
        Assert.Equal("?status=Em%20revisao%26x", handler.Ultima.RequestUri!.Query);
    }

    [Fact]
    public async Task Rejeitar_envia_o_motivo_no_corpo()
    {
        var (api, handler, _) = Montar(HttpStatusCode.OK, "\"Rejeitado\"");

        await new CursosAdminGateway(api).RejeitarAsync(5, "Falta prova.");

        Assert.Equal("https://api.teste/api/admin/cursos/5/rejeitar", handler.Ultima!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.Ultima.Method);
        Assert.Equal("{\"motivo\":\"Falta prova.\"}", handler.CorpoEnviado);
    }

    [Fact]
    public async Task Revisao_le_materiais_e_prova_com_gabarito()
    {
        const string json =
            "{\"idCurso\":5,\"nome\":\"SQL\",\"descricao\":null,\"categoria\":null,\"nivel\":null," +
            "\"duracaoHoras\":null,\"tutor\":\"Ana\",\"status\":\"Pendente\"," +
            "\"materiais\":[{\"id\":1,\"titulo\":\"Select\",\"tipo\":\"texto\",\"conteudo\":\"...\"}]," +
            "\"prova\":{\"id\":3,\"titulo\":\"Final\",\"notaMinima\":6.5,\"tempoMinutos\":20," +
            "\"questoes\":[{\"id\":9,\"ordem\":1,\"enunciado\":\"?\",\"codigoExemplo\":null," +
            "\"alternativas\":[{\"id\":1,\"letra\":\"A\",\"texto\":\"x\",\"ehCorreta\":true}]}]}}";
        var (api, _, _) = Montar(HttpStatusCode.OK, json);

        var c = await new CursosAdminGateway(api).ObterParaRevisaoAsync(5);

        Assert.Null(c.DuracaoHoras);
        Assert.Equal("Select", c.Materiais.Single().Titulo);
        Assert.Equal(6.5m, c.Prova!.NotaMinima);
        Assert.True(c.Prova.Questoes.Single().Alternativas.Single().EhCorreta);
    }

    [Fact]
    public async Task Usuarios_le_papel_como_texto_e_data_sem_fuso()
    {
        const string json =
            "[{\"id\":3,\"nome\":\"Ana\",\"email\":\"ana@tq.com\",\"papel\":\"Tutor\"," +
            "\"ativo\":false,\"dataCadastro\":\"2026-10-09T13:00:00\"}]";
        var (api, _, _) = Montar(HttpStatusCode.OK, json);

        var u = (await new UsuariosGateway(api).ListarAsync()).Single();

        Assert.Equal(PapelUsuario.Tutor, u.PapelDe());
        Assert.False(u.Ativo);
        Assert.Equal(DateTimeKind.Unspecified, u.DataCadastro.Kind); // por isso existe HorarioServidor
    }

    [Fact]
    public async Task Criar_usuario_envia_o_papel_como_texto()
    {
        const string resposta =
            "{\"id\":9,\"nome\":\"Ana\",\"email\":\"ana@tq.com\",\"papel\":\"Admin\"," +
            "\"ativo\":true,\"dataCadastro\":\"2026-10-09T13:00:00Z\"}";
        var (api, handler, _) = Montar(HttpStatusCode.OK, resposta);

        var criado = await new UsuariosGateway(api).CriarAsync(new NovoUsuario("Ana", "ana@tq.com", "segredo1", "Admin"));

        Assert.Equal("{\"nome\":\"Ana\",\"email\":\"ana@tq.com\",\"senha\":\"segredo1\",\"papel\":\"Admin\"}",
                     handler.CorpoEnviado);
        Assert.Equal(9, criado.Id);
    }

    [Fact]
    public async Task Alterar_status_usa_PATCH_na_rota_do_usuario()
    {
        var (api, handler, _) = Montar(HttpStatusCode.NoContent, "");

        await new UsuariosGateway(api).AlterarStatusAsync(4, false);

        Assert.Equal(HttpMethod.Patch, handler.Ultima!.Method);
        Assert.Equal("https://api.teste/api/admin/usuarios/4/status", handler.Ultima.RequestUri!.ToString());
        Assert.Equal("{\"ativo\":false}", handler.CorpoEnviado);
    }

    [Fact]
    public async Task Logs_pede_o_limite_na_query()
    {
        var (api, handler, _) = Montar(HttpStatusCode.OK, "[]");

        await new AuditoriaGateway(api).ListarAsync(200);

        Assert.Equal("https://api.teste/api/admin/logs?limite=200", handler.Ultima!.RequestUri!.ToString());
    }

    [Fact]
    public async Task Chamados_assumir_e_responder_usam_as_rotas_de_chamado()
    {
        var (api, handler, _) = Montar(HttpStatusCode.NoContent, "");
        var gateway = new ChamadosGateway(api);

        await gateway.AlterarStatusAsync(7, StatusChamado.EmAndamento);
        Assert.Equal(HttpMethod.Patch, handler.Ultima!.Method);
        Assert.Equal("https://api.teste/api/chamados/7/status", handler.Ultima.RequestUri!.ToString());
        Assert.Equal("{\"status\":\"Em andamento\"}", handler.CorpoEnviado);

        await gateway.ResponderAsync(7, "Resolvido.");
        Assert.Equal("https://api.teste/api/chamados/7/responder", handler.Ultima!.RequestUri!.ToString());
        Assert.Equal("{\"descricao\":\"Resolvido.\"}", handler.CorpoEnviado);
    }

    [Fact]
    public async Task Medalha_editada_usa_PUT_com_os_dados_no_corpo()
    {
        const string resposta = "{\"id\":3,\"nome\":\"Nova\",\"raridade\":\"raro\",\"descricao\":null,\"totalConquistas\":4}";
        var (api, handler, _) = Montar(HttpStatusCode.OK, resposta);

        var m = await new MedalhasGateway(api).AtualizarAsync(3, new DadosMedalha("Nova", "raro", null));

        Assert.Equal(HttpMethod.Put, handler.Ultima!.Method);
        Assert.Equal("https://api.teste/api/admin/medalhas/3", handler.Ultima.RequestUri!.ToString());
        Assert.Equal(4, m.TotalConquistas);
    }

    [Fact]
    public async Task Perfil_ignora_campos_de_estudante_e_troca_de_email_le_o_email_gravado()
    {
        const string perfil =
            "{\"id\":1,\"nome\":\"Admin\",\"email\":\"a@tq.com\",\"iniciais\":\"AD\",\"papel\":\"Admin\"," +
            "\"telefone\":null,\"dataNascimento\":\"1990-05-01\",\"cidade\":null," +
            "\"dataCadastro\":\"2026-01-10T01:00:00\",\"xp\":0,\"nivel\":1,\"xpProximoNivel\":350," +
            "\"progressoNivel\":0,\"cursosConcluidos\":0,\"aulasConcluidas\":0,\"provasAprovadas\":0,\"medalhas\":0}";
        var (api, _, _) = Montar(HttpStatusCode.OK, perfil);
        Assert.Equal("Admin", (await new ContaGateway(api).ObterPerfilAsync()).Papel);

        var (api2, handler2, _) = Montar(HttpStatusCode.OK, "{\"email\":\"novo@tq.com\"}");
        var gravado = await new ContaGateway(api2).TrocarEmailAsync(new TrocaEmail("senha1", "Novo@TQ.com"));
        Assert.Equal("novo@tq.com", gravado);
        Assert.Equal("{\"senha\":\"senha1\",\"emailNovo\":\"Novo@TQ.com\"}", handler2.CorpoEnviado);
    }

    [Fact]
    public async Task Troca_de_senha_aceita_resposta_204_sem_corpo()
    {
        var (api, handler, _) = Montar(HttpStatusCode.NoContent, "");

        await new ContaGateway(api).TrocarSenhaAsync(new TrocaSenha("a", "b"));

        Assert.Equal(HttpMethod.Put, handler.Ultima!.Method);
        Assert.Equal("https://api.teste/api/perfil/senha", handler.Ultima.RequestUri!.ToString());
    }

    [Fact]
    public async Task Com_sessao_ativa_envia_cabecalho_Bearer()
    {
        var (api, handler, sessao) = Montar(HttpStatusCode.OK, "{}");
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin));

        await api.GetAsync<Dictionary<string, object>>("api/admin/resumo");

        Assert.Equal("Bearer token-teste", handler.Ultima!.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task Status_401_sem_corpo_vira_NaoAutenticado()
    {
        var (api, _, _) = Montar(HttpStatusCode.Unauthorized, "");
        await Assert.ThrowsAsync<NaoAutenticadoException>(() => api.GetAsync<object>("api/x"));
    }

    [Fact]
    public async Task Status_400_usa_a_mensagem_do_servidor()
    {
        var (api, _, _) = Montar(HttpStatusCode.BadRequest, "{\"mensagem\":\"Curso ja publicado.\"}");

        var erro = await Assert.ThrowsAsync<OperacaoRecusadaException>(() => api.GetAsync<object>("api/x"));

        Assert.Equal("Curso ja publicado.", erro.Message);
    }

    [Fact]
    public async Task Status_409_do_middleware_vira_OperacaoRecusada()
    {
        var (api, _, _) = Montar(HttpStatusCode.Conflict, "{\"mensagem\":\"Conflito.\",\"identificador\":\"abc\"}");
        await Assert.ThrowsAsync<OperacaoRecusadaException>(() => api.GetAsync<object>("api/x"));
    }

    [Fact]
    public async Task Status_403_vira_AcessoNegado()
    {
        var (api, _, _) = Montar(HttpStatusCode.Forbidden, "");
        await Assert.ThrowsAsync<AcessoNegadoException>(() => api.GetAsync<object>("api/x"));
    }

    [Fact]
    public async Task Status_404_vira_RegistroNaoEncontrado()
    {
        var (api, _, _) = Montar(HttpStatusCode.NotFound, "");
        await Assert.ThrowsAsync<RegistroNaoEncontradoException>(() => api.GetAsync<object>("api/x"));
    }

    [Fact]
    public async Task Status_500_com_corpo_HTML_vira_ServidorIndisponivel_com_o_codigo()
    {
        var (api, _, _) = Montar(HttpStatusCode.InternalServerError, "<html>erro</html>");

        var erro = await Assert.ThrowsAsync<ServidorIndisponivelException>(() => api.GetAsync<object>("api/x"));

        Assert.Contains("500", erro.Message);
    }

    [Fact]
    public async Task Resposta_com_JSON_quebrado_vira_ServidorIndisponivel()
    {
        var (api, _, _) = Montar(HttpStatusCode.OK, "{nao e json");
        await Assert.ThrowsAsync<ServidorIndisponivelException>(() => api.GetAsync<object>("api/x"));
    }

    [Fact]
    public async Task Tempo_esgotado_vira_ServidorIndisponivel()
    {
        var http = new HttpClient(new HandlerLento())
        {
            BaseAddress = new Uri("https://api.teste/"),
            Timeout = TimeSpan.FromMilliseconds(100)
        };
        var api = new ApiCliente(http, new SessaoAdmin());

        await Assert.ThrowsAsync<ServidorIndisponivelException>(() => api.GetAsync<object>("api/x"));
    }
}
