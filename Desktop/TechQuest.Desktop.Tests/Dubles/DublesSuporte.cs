using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Conta;
using TechQuest.Desktop.Apresentacao.Medalhas;
using TechQuest.Desktop.Apresentacao.Suporte;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Tests.Dubles;

public sealed class ChamadosGatewayFalso : IChamadosGateway
{
    public List<Chamado> Chamados { get; set; } = new();
    public List<(int Id, string Status)> MudancasDeStatus { get; } = new();
    public List<(int Id, string Texto)> Respostas { get; } = new();

    public Task<IReadOnlyList<Chamado>> ListarTecnicosAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Chamado>>(Chamados.ToList());

    public Task AlterarStatusAsync(int idChamado, string status, CancellationToken ct = default)
    {
        MudancasDeStatus.Add((idChamado, status));
        Trocar(idChamado, status);
        return Task.CompletedTask;
    }

    public Task ResponderAsync(int idChamado, string texto, CancellationToken ct = default)
    {
        Respostas.Add((idChamado, texto));
        Trocar(idChamado, StatusChamado.Resolvido);
        return Task.CompletedTask;
    }

    private void Trocar(int id, string status)
    {
        var i = Chamados.FindIndex(c => c.Id == id);
        if (i >= 0) Chamados[i] = Chamados[i] with { Status = status };
    }

    public static Chamado Novo(int id, string status, DateTime abertura, string assunto = "Erro no login",
                               string remetente = "Ana Aluna")
        => new(id, "Tecnico", assunto, "Nao consigo entrar.", 10 + id, remetente, null, null, abertura, status, null);
}

public sealed class MedalhasGatewayFalso : IMedalhasGateway
{
    public List<MedalhaAdmin> Medalhas { get; set; } = new();
    public List<DadosMedalha> Criadas { get; } = new();
    public List<(int Id, DadosMedalha Dados)> Atualizadas { get; } = new();

    public Task<IReadOnlyList<MedalhaAdmin>> ListarAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<MedalhaAdmin>>(Medalhas.ToList());

    public Task<MedalhaAdmin> CriarAsync(DadosMedalha dados, CancellationToken ct = default)
    {
        Criadas.Add(dados);
        var m = new MedalhaAdmin(100 + Criadas.Count, dados.Nome, dados.Raridade, dados.Descricao, 0);
        Medalhas.Add(m);
        return Task.FromResult(m);
    }

    public Task<MedalhaAdmin> AtualizarAsync(int idMedalha, DadosMedalha dados, CancellationToken ct = default)
    {
        Atualizadas.Add((idMedalha, dados));
        var i = Medalhas.FindIndex(m => m.Id == idMedalha);
        var m = Medalhas[i] with { Nome = dados.Nome, Raridade = dados.Raridade, Descricao = dados.Descricao };
        Medalhas[i] = m;
        return Task.FromResult(m);
    }
}

public sealed class ContaGatewayFalso : IContaGateway
{
    public PerfilUsuario Perfil { get; set; } = new(1, "Admin Logado", "adm@tq.com", "AL", "Admin",
        null, "Sao Paulo", new DateTime(2026, 1, 10, 1, 0, 0));
    public List<TrocaSenha> TrocasDeSenha { get; } = new();
    public List<TrocaEmail> TrocasDeEmail { get; } = new();

    public Task<PerfilUsuario> ObterPerfilAsync(CancellationToken ct = default) => Task.FromResult(Perfil);

    public Task TrocarSenhaAsync(TrocaSenha troca, CancellationToken ct = default)
    {
        TrocasDeSenha.Add(troca);
        return Task.CompletedTask;
    }

    public Task<string> TrocarEmailAsync(TrocaEmail troca, CancellationToken ct = default)
    {
        TrocasDeEmail.Add(troca);
        return Task.FromResult(troca.EmailNovo.Trim().ToLowerInvariant()); // como o servidor
    }
}

public sealed class SuporteViewFalsa : ISuporteView
{
    public string? FiltroSituacao { get; set; } = "abertos";
    public string TermoBusca { get; set; } = "";
    public int? ChamadoSelecionado { get; set; }
    public string Resposta { get; set; } = "";

    public IReadOnlyList<LinhaChamado>? Linhas { get; private set; }
    public string? Resumo { get; private set; }
    public DetalheChamado? Detalhe { get; private set; }
    public string? Erro { get; private set; }
    public string? Informacao { get; private set; }
    public bool RespostaLimpa { get; private set; }
    public bool RespostaFocada { get; private set; }
    public bool RespostaConfirmacao { get; set; } = true;
    public List<string> Confirmacoes { get; } = new();

    public void ExibirChamados(IReadOnlyList<LinhaChamado> linhas, string resumo) { Linhas = linhas; Resumo = resumo; }
    public void Selecionar(int idChamado) => ChamadoSelecionado = idChamado;
    public void ExibirDetalhe(DetalheChamado? detalhe) => Detalhe = detalhe;
    public void LimparResposta() { RespostaLimpa = true; Resposta = ""; }
    public void FocarResposta() => RespostaFocada = true;
    public void OcultarErro() => Erro = null;
    public bool Confirmar(string mensagem, string titulo) { Confirmacoes.Add(mensagem); return RespostaConfirmacao; }
    public void Informar(string mensagem) => Informacao = mensagem;
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) => Erro = mensagem;
}

public sealed class MedalhasViewFalsa : IMedalhasView
{
    public string? FiltroRaridade { get; set; }
    public string TermoBusca { get; set; } = "";
    public int? MedalhaSelecionada { get; set; }
    public IReadOnlyList<OpcaoFiltro>? Filtros { get; private set; }
    public IReadOnlyList<LinhaMedalha>? Linhas { get; private set; }
    public string? Resumo { get; private set; }
    public bool? EdicaoDisponivel { get; private set; }
    public List<MedalhaAdmin?> EditoresAbertos { get; } = new();
    public Func<MedalhaAdmin?, MedalhaAdmin?> ResultadoEditor { get; set; } = _ => null;

    public void ExibirFiltros(IReadOnlyList<OpcaoFiltro> opcoes) => Filtros = opcoes;
    public void ExibirMedalhas(IReadOnlyList<LinhaMedalha> linhas, string resumo) { Linhas = linhas; Resumo = resumo; }
    public void Selecionar(int idMedalha) => MedalhaSelecionada = idMedalha;
    public void OcultarErro() { }
    public void DefinirEdicaoDisponivel(bool disponivel) => EdicaoDisponivel = disponivel;
    public MedalhaAdmin? AbrirEditor(MedalhaAdmin? existente) { EditoresAbertos.Add(existente); return ResultadoEditor(existente); }
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) { }
}

public sealed class MedalhaEditorViewFalsa : IMedalhaEditorView
{
    public string Nome { get; set; } = "";
    public string? Raridade { get; set; }
    public string Descricao { get; set; } = "";
    public string? Titulo { get; private set; }
    public string? Aviso { get; private set; }
    public string? Erro { get; private set; }
    public MedalhaAdmin? Salva { get; private set; }
    public bool RespostaConfirmacao { get; set; } = true;
    public List<string> Confirmacoes { get; } = new();

    public void Preencher(string titulo, string nome, string raridade, string descricao, string? aviso)
    {
        Titulo = titulo; Nome = nome; Raridade = raridade; Descricao = descricao; Aviso = aviso;
    }
    public bool Confirmar(string mensagem, string titulo) { Confirmacoes.Add(mensagem); return RespostaConfirmacao; }
    public void Concluir(MedalhaAdmin salva) => Salva = salva;
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) => Erro = mensagem;
}

public sealed class MinhaContaViewFalsa : IMinhaContaView
{
    public string SenhaAtual { get; set; } = "";
    public string SenhaNova { get; set; } = "";
    public string ConfirmacaoSenha { get; set; } = "";
    public string EmailNovo { get; set; } = "";
    public string SenhaParaEmail { get; set; } = "";

    public PerfilExibicao? Perfil { get; private set; }
    public string? Erro { get; private set; }
    public string? Informacao { get; private set; }
    public bool SenhasLimpas { get; private set; }
    public bool EmailLimpo { get; private set; }

    public void ExibirPerfil(PerfilExibicao perfil) => Perfil = perfil;
    public void LimparCamposSenha() => SenhasLimpas = true;
    public void LimparCamposEmail() => EmailLimpo = true;
    public void Informar(string mensagem) => Informacao = mensagem;
    public void OcultarMensagem() { Erro = null; Informacao = null; }
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) => Erro = mensagem;
}
