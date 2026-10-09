using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Cursos;
using TechQuest.Desktop.Apresentacao.Logs;
using TechQuest.Desktop.Apresentacao.Usuarios;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Tests.Dubles;

public sealed class UsuariosGatewayFalso : IUsuariosGateway
{
    public List<UsuarioAdmin> Usuarios { get; set; } = new();
    public Exception? FalhaAoCriar { get; set; }
    public List<NovoUsuario> Criados { get; } = new();
    public List<(int Id, bool Ativo)> MudancasDeStatus { get; } = new();

    public Task<IReadOnlyList<UsuarioAdmin>> ListarAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<UsuarioAdmin>>(Usuarios.ToList());

    public Task<UsuarioAdmin> CriarAsync(NovoUsuario novo, CancellationToken ct = default)
    {
        if (FalhaAoCriar is not null) throw FalhaAoCriar;
        Criados.Add(novo);
        var criado = new UsuarioAdmin(100 + Criados.Count, novo.Nome, novo.Email.ToLowerInvariant(),
                                      novo.Papel, true, DateTime.UtcNow);
        Usuarios.Add(criado);
        return Task.FromResult(criado);
    }

    public Task AlterarStatusAsync(int idUsuario, bool ativo, CancellationToken ct = default)
    {
        MudancasDeStatus.Add((idUsuario, ativo));
        var i = Usuarios.FindIndex(u => u.Id == idUsuario);
        if (i >= 0) Usuarios[i] = Usuarios[i] with { Ativo = ativo };
        return Task.CompletedTask;
    }
}

public sealed class AuditoriaGatewayFalso : IAuditoriaGateway
{
    public List<LogAuditoria> Logs { get; set; } = new();
    public List<int> LimitesPedidos { get; } = new();

    public Task<IReadOnlyList<LogAuditoria>> ListarAsync(int limite, CancellationToken ct = default)
    {
        LimitesPedidos.Add(limite);
        return Task.FromResult<IReadOnlyList<LogAuditoria>>(Logs.Take(limite).ToList());
    }
}

public sealed class CursosViewFalsa : ICursosView
{
    public string? StatusFiltro { get; set; }
    public string TermoBusca { get; set; } = "";
    public int? CursoSelecionado { get; set; }
    public IReadOnlyList<OpcaoFiltro>? Filtros { get; private set; }
    public IReadOnlyList<LinhaCurso>? Linhas { get; private set; }
    public string? Resumo { get; private set; }
    public List<int> RevisoesAbertas { get; } = new();

    public void ExibirFiltros(IReadOnlyList<OpcaoFiltro> opcoes) => Filtros = opcoes;
    public void ExibirCursos(IReadOnlyList<LinhaCurso> linhas, string resumo) { Linhas = linhas; Resumo = resumo; }
    public void OcultarErro() { }
    public void AbrirRevisao(int idCurso) => RevisoesAbertas.Add(idCurso);
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) { }
}

public sealed class UsuariosViewFalsa : IUsuariosView
{
    public string? PapelFiltro { get; set; }
    public bool? SituacaoFiltro { get; set; }
    public string TermoBusca { get; set; } = "";
    public int? UsuarioSelecionado { get; set; }

    public IReadOnlyList<LinhaUsuario>? Linhas { get; private set; }
    public string? Resumo { get; private set; }
    public string? Erro { get; private set; }
    public (string Texto, bool Habilitada, string? Motivo)? Acao { get; private set; }
    public bool RespostaConfirmacao { get; set; } = true;
    public List<string> Confirmacoes { get; } = new();
    public UsuarioAdmin? ResultadoCadastro { get; set; }

    public void ExibirUsuarios(IReadOnlyList<LinhaUsuario> linhas, string resumo) { Linhas = linhas; Resumo = resumo; }
    public void Selecionar(int idUsuario) => UsuarioSelecionado = idUsuario;
    public void OcultarErro() => Erro = null;
    public void DefinirAcaoStatus(string texto, bool habilitada, string? motivo) => Acao = (texto, habilitada, motivo);
    public bool Confirmar(string mensagem, string titulo) { Confirmacoes.Add(mensagem); return RespostaConfirmacao; }
    public UsuarioAdmin? AbrirCadastro() => ResultadoCadastro;
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) => Erro = mensagem;
}

public sealed class CadastroViewFalsa : ICadastroUsuarioView
{
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";
    public PapelUsuario Papel { get; set; } = PapelUsuario.Tutor;
    public string? Erro { get; private set; }
    public (UsuarioAdmin Criado, string Senha)? Conclusao { get; private set; }

    public void DefinirSenha(string senha) => Senha = senha;
    public void Concluir(UsuarioAdmin criado, string senhaInicial) => Conclusao = (criado, senhaInicial);
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) => Erro = mensagem;
}

public sealed class LogsViewFalsa : ILogsView
{
    public int Limite { get; set; } = 100;
    public string TermoBusca { get; set; } = "";
    public IReadOnlyList<LinhaLog>? Linhas { get; private set; }
    public string? Resumo { get; private set; }

    public void ExibirLogs(IReadOnlyList<LinhaLog> linhas, string resumo) { Linhas = linhas; Resumo = resumo; }
    public void OcultarErro() { }
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) { }
}

public static class Fusos
{
    /// <summary>Brasilia fixo (UTC-3, sem horario de verao desde 2019), independente da maquina do teste.</summary>
    public static readonly TimeZoneInfo Brasilia =
        TimeZoneInfo.CreateCustomTimeZone("Teste/Brasilia", TimeSpan.FromHours(-3), "Brasilia", "Brasilia");
}
