using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Seguranca;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Aplicacao;

public class HorarioServidorTests
{
    [Fact]
    public void Data_sem_fuso_vinda_da_API_e_tratada_como_UTC()
    {
        // "2026-10-09T13:00:00" no JSON: DATETIME do SQL lida pelo EF, sem o Z.
        var doServidor = new DateTime(2026, 10, 9, 13, 0, 0, DateTimeKind.Unspecified);

        var local = HorarioServidor.ParaLocal(doServidor, Fusos.Brasilia);

        Assert.Equal(new DateTime(2026, 10, 9, 10, 0, 0), local);
    }

    [Fact]
    public void Data_marcada_como_UTC_tem_o_mesmo_resultado()
    {
        var utc = new DateTime(2026, 10, 9, 2, 30, 0, DateTimeKind.Utc);

        Assert.Equal(new DateTime(2026, 10, 8, 23, 30, 0), HorarioServidor.ParaLocal(utc, Fusos.Brasilia));
    }
}

public class TextoBuscaTests
{
    [Theory]
    [InlineData("joao", "João Silva")]
    [InlineData("JOÃO", "joao silva")]
    [InlineData("acao", "Ação de auditoria")]
    [InlineData("  silva ", "João Silva")]
    [InlineData("", "qualquer coisa")]
    public void Encontra_ignorando_acento_e_maiuscula(string termo, string campo)
    {
        Assert.True(TextoBusca.Contem(termo, campo));
    }

    [Fact]
    public void Procura_em_todos_os_campos_e_aceita_campo_nulo()
    {
        Assert.True(TextoBusca.Contem("gmail", "Ana", null, "ana@gmail.com"));
        Assert.False(TextoBusca.Contem("hotmail", "Ana", null, "ana@gmail.com"));
    }
}

public class GeradorDeSenhaTests
{
    [Fact]
    public void Gera_senhas_com_letra_e_digito_sem_caracteres_ambiguos()
    {
        for (var i = 0; i < 200; i++)
        {
            var s = GeradorDeSenha.Gerar();
            Assert.Equal(GeradorDeSenha.Tamanho, s.Length);
            Assert.True(s.Any(char.IsLetter));
            Assert.True(s.Any(char.IsDigit));
            Assert.False(s.Any(c => "0O1lI".Contains(c)));
        }
    }

    [Fact]
    public void Senhas_seguidas_sao_diferentes()
    {
        var senhas = Enumerable.Range(0, 50).Select(_ => GeradorDeSenha.Gerar()).ToHashSet();
        Assert.Equal(50, senhas.Count);
    }
}

public class UsuariosServiceTests
{
    private static (UsuariosService servico, UsuariosGatewayFalso api) Montar(int idLogado = 1)
    {
        var sessao = new SessaoAdmin();
        sessao.Iniciar(new RespostaLogin("t", DateTime.UtcNow.AddHours(1),
            new UsuarioLogado(idLogado, "Admin Logado", "adm@tq.com", "AL", PapelUsuario.Admin)));
        var api = new UsuariosGatewayFalso();
        return (new UsuariosService(api, sessao), api);
    }

    [Fact]
    public async Task Cria_enviando_o_papel_como_texto_e_dados_sem_espacos()
    {
        var (servico, api) = Montar();

        var r = await servico.CriarAsync("  Ana Souza ", " ana@tq.com ", "segredo1", PapelUsuario.Tutor);

        Assert.True(r.Sucesso);
        Assert.Equal(new NovoUsuario("Ana Souza", "ana@tq.com", "segredo1", "Tutor"), api.Criados.Single());
    }

    [Theory]
    [InlineData("Al", "a@a.com", "segredo1")]       // nome curto
    [InlineData("Ana Souza", "sem-arroba", "segredo1")]
    [InlineData("Ana Souza", "a@a.com", "12345")]   // senha curta
    public async Task Dados_invalidos_nao_chegam_a_API(string nome, string email, string senha)
    {
        var (servico, api) = Montar();

        var r = await servico.CriarAsync(nome, email, senha, PapelUsuario.Estudante);

        Assert.False(r.Sucesso);
        Assert.Empty(api.Criados);
    }

    [Fact]
    public async Task Email_repetido_e_recusado_pelo_servidor()
    {
        var (servico, api) = Montar();
        api.FalhaAoCriar = new OperacaoRecusadaException("Ja existe um usuario com este e-mail.");

        await Assert.ThrowsAsync<OperacaoRecusadaException>(
            () => servico.CriarAsync("Ana Souza", "a@a.com", "segredo1", PapelUsuario.Tutor));
    }

    [Fact]
    public async Task Admin_nao_desativa_a_propria_conta()
    {
        var (servico, api) = Montar(idLogado: 1);
        var eu = new UsuarioAdmin(1, "Admin Logado", "adm@tq.com", "Admin", true, DateTime.UtcNow);

        var r = await servico.AlterarStatusAsync(eu, ativo: false);

        Assert.False(r.Sucesso);
        Assert.Empty(api.MudancasDeStatus);
    }

    [Fact]
    public async Task Desativa_outra_conta()
    {
        var (servico, api) = Montar(idLogado: 1);
        var outro = new UsuarioAdmin(5, "Tutor", "t@tq.com", "Tutor", true, DateTime.UtcNow);

        var r = await servico.AlterarStatusAsync(outro, ativo: false);

        Assert.True(r.Sucesso);
        Assert.Equal((5, false), api.MudancasDeStatus.Single());
    }
}

public class AuditoriaServiceTests
{
    [Fact]
    public async Task So_aceita_as_quantidades_da_tela()
    {
        var servico = new AuditoriaService(new AuditoriaGatewayFalso());

        await servico.ListarAsync(200);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => servico.ListarAsync(100000));
    }
}

public class ListarTodosOsCursosTests
{
    [Fact]
    public async Task Busca_os_quatro_estados_e_atualiza_o_selo_com_os_pendentes()
    {
        var api = new CursosAdminGatewayFalso
        {
            Fila = new()
            {
                CursosDeTeste.Solicitacao(1, status: StatusCurso.Pendente),
                CursosDeTeste.Solicitacao(2, status: StatusCurso.Publicado),
                CursosDeTeste.Solicitacao(3, status: StatusCurso.Rascunho),
                CursosDeTeste.Solicitacao(4, status: StatusCurso.Rejeitado),
                CursosDeTeste.Solicitacao(5, status: StatusCurso.Pendente)
            }
        };
        var contador = new ContadorDePendencias();

        var todos = await new AvaliacaoCursosService(api, contador).ListarTodosAsync();

        Assert.Equal(5, todos.Count);
        Assert.Equal(4, api.EstadosPedidos.Distinct().Count());
        Assert.Equal(2, contador.CursosPendentes);
    }
}
