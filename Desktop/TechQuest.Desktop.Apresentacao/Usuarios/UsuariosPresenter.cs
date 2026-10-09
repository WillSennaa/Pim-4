using System.Globalization;
using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Usuarios;

public interface IUsuariosView : IViewBase
{
    /// <summary>"Admin", "Tutor", "Estudante" ou null (todos).</summary>
    string? PapelFiltro { get; }

    /// <summary>true = so ativos, false = so inativos, null = todos.</summary>
    bool? SituacaoFiltro { get; }

    string TermoBusca { get; }
    int? UsuarioSelecionado { get; }

    void ExibirUsuarios(IReadOnlyList<LinhaUsuario> linhas, string resumo);
    void Selecionar(int idUsuario);
    void OcultarErro();

    /// <summary>Texto e disponibilidade do botao Ativar/Desativar para a linha atual.</summary>
    void DefinirAcaoStatus(string texto, bool habilitada, string? motivoBloqueio);

    bool Confirmar(string mensagem, string titulo);

    /// <summary>Abre o cadastro modal. Devolve a conta criada, ou null se cancelou.</summary>
    UsuarioAdmin? AbrirCadastro();
}

public sealed record LinhaUsuario(
    int Id, string Nome, string Email, string Papel, string Situacao,
    bool Ativo, string Cadastro, bool EhVoce);

/// <summary>Lista de contas, filtros, ativar/desativar e criacao (RF01).</summary>
public sealed class UsuariosPresenter : PresenterBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IUsuariosView _view;
    private readonly UsuariosService _servico;
    private readonly TimeZoneInfo? _fuso;
    private IReadOnlyList<UsuarioAdmin> _todos = Array.Empty<UsuarioAdmin>();

    public UsuariosPresenter(
        IUsuariosView view, UsuariosService servico, SessaoAdmin sessao, TimeZoneInfo? fuso = null)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
        _fuso = fuso;
    }

    public async Task CarregarAsync(int? selecionar = null)
    {
        _view.OcultarErro();
        var anterior = selecionar ?? _view.UsuarioSelecionado;

        IReadOnlyList<UsuarioAdmin>? usuarios = null;
        if (!await ExecutarAsync(async () => usuarios = await _servico.ListarAsync()))
        {
            SelecaoMudou(); // a tela trava o botao durante a carga; devolve o estado certo
            return;
        }

        _todos = usuarios!.OrderBy(u => u.Nome, StringComparer.Create(PtBr, ignoreCase: true)).ToList();
        Filtrar();

        // Recarregar nao deve fazer o admin perder a linha em que estava.
        if (anterior is int id && _todos.Any(u => u.Id == id)) _view.Selecionar(id);
        SelecaoMudou();
    }

    public void Filtrar()
    {
        var papel = _view.PapelFiltro;
        var situacao = _view.SituacaoFiltro;
        var termo = _view.TermoBusca;

        var visiveis = _todos
            .Where(u => papel is null || string.Equals(u.Papel, papel, StringComparison.OrdinalIgnoreCase))
            .Where(u => situacao is null || u.Ativo == situacao)
            .Where(u => TextoBusca.Contem(termo, u.Nome, u.Email))
            .ToList();

        _view.ExibirUsuarios(visiveis.Select(Linha).ToList(), Resumo(visiveis));
    }

    public void SelecaoMudou()
    {
        var alvo = Selecionado();
        if (alvo is null)
            _view.DefinirAcaoStatus("Desativar conta", false, null);
        else if (alvo.Ativo && _servico.EhOProprioAdmin(alvo))
            _view.DefinirAcaoStatus("Desativar conta", false, "Você não pode desativar a própria conta.");
        else
            _view.DefinirAcaoStatus(alvo.Ativo ? "Desativar conta" : "Reativar conta", true, null);
    }

    public async Task AlternarStatusSelecionadoAsync()
    {
        var alvo = Selecionado();
        if (alvo is null) return;

        var ativar = !alvo.Ativo;
        var confirmado = ativar
            ? _view.Confirmar($"Reativar a conta de {alvo.Nome}?\n\nA pessoa volta a conseguir entrar na plataforma.",
                              "Reativar conta")
            : _view.Confirmar($"Desativar a conta de {alvo.Nome}?\n\n" +
                              "A pessoa não conseguirá mais entrar. O histórico, os cursos e os certificados " +
                              "são mantidos, e a conta pode ser reativada a qualquer momento.",
                              "Desativar conta");
        if (!confirmado) return;

        ResultadoOperacao? r = null;
        var concluiu = await ExecutarAsync(async () => r = await _servico.AlterarStatusAsync(alvo, ativar));

        if (concluiu && r!.Sucesso)
        {
            await CarregarAsync(selecionar: alvo.Id);
            return;
        }

        if (concluiu) _view.MostrarErro(r!.Mensagem!);
        SelecaoMudou(); // botao volta a refletir a linha selecionada
    }

    public async Task NovoUsuarioAsync()
    {
        var criado = _view.AbrirCadastro();
        if (criado is null) return;
        await CarregarAsync(selecionar: criado.Id);
    }

    private UsuarioAdmin? Selecionado()
        => _view.UsuarioSelecionado is int id ? _todos.FirstOrDefault(u => u.Id == id) : null;

    internal LinhaUsuario Linha(UsuarioAdmin u)
    {
        var ehVoce = _servico.EhOProprioAdmin(u);
        return new LinhaUsuario(
            u.Id,
            ehVoce ? u.Nome + " (você)" : u.Nome,
            u.Email ?? "-",
            NomeDoPapel(u.Papel),
            u.Ativo ? "Ativo" : "Inativo",
            u.Ativo,
            HorarioServidor.ParaLocal(u.DataCadastro, _fuso).ToString("dd/MM/yyyy", PtBr),
            ehVoce);
    }

    public static string NomeDoPapel(string? papel) => papel?.ToLowerInvariant() switch
    {
        "admin" => "Administrador",
        "tutor" => "Tutor",
        "estudante" => "Estudante",
        _ => papel ?? "-"
    };

    internal string Resumo(IReadOnlyList<UsuarioAdmin> visiveis)
    {
        if (_todos.Count == 0) return "Nenhum usuário cadastrado.";
        var ativos = visiveis.Count(u => u.Ativo);
        var parte = visiveis.Count == _todos.Count
            ? (_todos.Count == 1 ? "1 usuário" : $"{_todos.Count} usuários")
            : $"Exibindo {visiveis.Count} de {_todos.Count} usuários";
        return $"{parte} · {ativos} {(ativos == 1 ? "ativo" : "ativos")}";
    }
}
