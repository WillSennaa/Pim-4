using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.UI.Telas;

namespace TechQuest.Desktop.UI.Navegacao;

/// <summary>
/// Cria a tela (UserControl) de cada secao com as dependencias de que ela
/// precisa.
///
/// POR QUE UMA FABRICA: a janela principal nao deve conhecer os servicos de
/// cada tela (o de usuarios, o de cursos...). Ela pede "a tela de Usuarios"
/// e recebe pronta. Cada entrega nova acrescenta um servico ao construtor e
/// uma linha ao switch, sem tocar na janela principal.
///
/// O navegador chega por parametro, e nao pelo construtor, porque so existe
/// depois que a janela principal e criada, e a fabrica e criada antes dela,
/// em Program.cs.
/// </summary>
public sealed class FabricaDeTelas
{
    private readonly SessaoAdmin _sessao;
    private readonly PainelService _painel;
    private readonly AvaliacaoCursosService _avaliacao;
    private readonly UsuariosService _usuarios;
    private readonly AuditoriaService _auditoria;
    private readonly SuporteService _suporte;
    private readonly MedalhasService _medalhas;
    private readonly ContaService _conta;

    public FabricaDeTelas(
        SessaoAdmin sessao, PainelService painel, AvaliacaoCursosService avaliacao,
        UsuariosService usuarios, AuditoriaService auditoria,
        SuporteService suporte, MedalhasService medalhas, ContaService conta)
    {
        _suporte = suporte;
        _medalhas = medalhas;
        _conta = conta;
        _sessao = sessao;
        _painel = painel;
        _avaliacao = avaliacao;
        _usuarios = usuarios;
        _auditoria = auditoria;
    }

    public Control Criar(SecaoAdmin secao, INavegador navegador) => secao switch
    {
        SecaoAdmin.Painel => new TelaPainel(_painel, navegador, _sessao),
        SecaoAdmin.Aprovacoes => new TelaAprovacoes(_avaliacao, _sessao),
        SecaoAdmin.Cursos => new TelaCursos(_avaliacao, _sessao),
        SecaoAdmin.Usuarios => new TelaUsuarios(_usuarios, _sessao),
        SecaoAdmin.Logs => new TelaLogs(_auditoria, _sessao),
        SecaoAdmin.Chamados => new TelaChamados(_suporte, _sessao),
        SecaoAdmin.Medalhas => new TelaMedalhas(_medalhas, _sessao),
        SecaoAdmin.MinhaConta => new TelaMinhaConta(_conta, _sessao),

        // Todas as secoes de hoje tem tela. O marcador fica como rede de
        // seguranca: uma secao nova no enum aparece no menu (CriarMenu le o
        // enum) e mostra "em construcao" em vez de derrubar o programa.
        _ => new TelaEmConstrucao(secao.Titulo())
    };
}
