using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.Integracao;
using TechQuest.Desktop.Integracao.Gateways;
using TechQuest.Desktop.UI.Configuracao;
using TechQuest.Desktop.UI.Formularios;
using TechQuest.Desktop.UI.Navegacao;

namespace TechQuest.Desktop.UI;

/// <summary>
/// RAIZ DE COMPOSICAO. E o unico lugar do desktop que conhece todas as
/// camadas ao mesmo tempo: aqui cada interface recebe sua implementacao e
/// cada objeto recebe, pelo construtor, aquilo de que depende. E o mesmo
/// papel do Program.cs da API, so que montado a mao.
///
/// POR QUE INJECAO MANUAL E NAO UM CONTAINER (Microsoft.Extensions.
/// DependencyInjection): no ASP.NET o container ja vem com o framework e
/// gerencia o escopo de cada requisicao. No desktop existe UMA sessao e UM
/// usuario; o grafo de objetos cabe em poucas linhas e fica legivel de cima
/// a baixo, sem pacote extra. Se o numero de servicos crescer a ponto de
/// estas linhas ficarem confusas, o container passa a compensar.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        ConfigurarTratamentoGlobalDeErros();

        ConfiguracaoDesktop configuracao;
        try
        {
            configuracao = ConfiguracaoDesktop.Carregar();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Configuração", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // ---- objetos que vivem o programa inteiro ----
        using var http = new HttpClient
        {
            BaseAddress = configuracao.UrlBaseApi,
            Timeout = configuracao.Timeout
        };

        var sessao = new SessaoAdmin();
        var pendencias = new ContadorDePendencias();
        var api = new ApiCliente(http, sessao);

        var autenticacao = new AutenticacaoService(new AutenticacaoGateway(api), sessao);
        var painel = new PainelService(new PainelGateway(api), pendencias);
        var avaliacao = new AvaliacaoCursosService(new CursosAdminGateway(api), pendencias);
        var usuarios = new UsuariosService(new UsuariosGateway(api), sessao);
        var auditoria = new AuditoriaService(new AuditoriaGateway(api));
        var suporte = new SuporteService(new ChamadosGateway(api));
        var medalhas = new MedalhasService(new MedalhasGateway(api));
        var conta = new ContaService(new ContaGateway(api), sessao);

        var fabrica = new FabricaDeTelas(
            sessao, painel, avaliacao, usuarios, auditoria, suporte, medalhas, conta);

        // ---- ciclo login -> principal -> (logout) -> login ----
        // Sair da conta volta ao login sem fechar o programa; fechar a
        // janela principal encerra.
        while (true)
        {
            using (var login = new FormLogin(autenticacao, sessao))
            {
                if (login.ShowDialog() != DialogResult.OK) return;
            }

            using var principal = new FormPrincipal(autenticacao, sessao, pendencias, fabrica);
            Application.Run(principal);

            if (principal.Saida != SaidaPrincipal.VoltarAoLogin) return;
        }
    }

    /// <summary>
    /// Ultima rede de seguranca. Erros previstos (rede, 4xx) sao tratados nos
    /// presenters e nunca chegam aqui; o que chega e defeito de programa. Em
    /// vez de o Windows fechar o aplicativo com a janela generica de "parou de
    /// funcionar", o admin ve a mensagem e pode continuar.
    /// </summary>
    private static void ConfigurarTratamentoGlobalDeErros()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        Application.ThreadException += (_, e) =>
            MessageBox.Show(
                "Ocorreu um erro inesperado:\n\n" + e.Exception.Message,
                "Tech Quest", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
