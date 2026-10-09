using System.Net;
using System.Text;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Apresentacao.Login;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Tests.Dubles;

/// <summary>
/// Servidor HTTP falso: devolve sempre o status e o corpo configurados e
/// guarda a ultima requisicao para o teste inspecionar.
/// </summary>
public sealed class HandlerFalso : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _corpo;

    public HandlerFalso(HttpStatusCode status, string corpo)
    {
        _status = status;
        _corpo = corpo;
    }

    public HttpRequestMessage? Ultima { get; private set; }
    public string? CorpoEnviado { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        Ultima = r;
        CorpoEnviado = r.Content is null ? null : await r.Content.ReadAsStringAsync(ct);
        return new HttpResponseMessage(_status)
        {
            Content = new StringContent(_corpo, Encoding.UTF8, "application/json")
        };
    }
}

/// <summary>Servidor que nunca responde, para provocar o Timeout.</summary>
public sealed class HandlerLento : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        await Task.Delay(Timeout.Infinite, ct);
        return new HttpResponseMessage(HttpStatusCode.OK);
    }
}

public sealed class AutenticacaoGatewayFalso : IAutenticacaoGateway
{
    private readonly Func<RespostaLogin> _resposta;
    public AutenticacaoGatewayFalso(Func<RespostaLogin> resposta) => _resposta = resposta;

    public int Chamadas { get; private set; }

    public Task<RespostaLogin> EntrarAsync(CredenciaisLogin credenciais, CancellationToken ct = default)
    {
        Chamadas++;
        return Task.FromResult(_resposta());
    }
}

/// <summary>Relogio controlado: o teste avanca o tempo sem esperar.</summary>
public sealed class RelogioFalso : TimeProvider
{
    private DateTimeOffset _agora = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _agora;
    public void Avancar(TimeSpan tempo) => _agora += tempo;
}

/// <summary>Tela de login falsa: registra o que o presenter mandou fazer.</summary>
public sealed class LoginViewFalsa : ILoginView
{
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";

    public List<bool> Ocupado { get; } = new();
    public string? Erro { get; private set; }
    public bool Concluiu { get; private set; }
    public bool SenhaLimpa { get; private set; }

    public void DefinirOcupado(bool ocupado) => Ocupado.Add(ocupado);
    public void MostrarErro(string mensagem) => Erro = mensagem;
    public void LimparSenha() => SenhaLimpa = true;
    public void ConcluirLogin() => Concluiu = true;
}

public sealed class PrincipalViewFalsa : IPrincipalView
{
    public string? Iniciais { get; private set; }
    public string? Titulo { get; private set; }
    public List<SecaoAdmin> Exibidas { get; } = new();
    public int AvisosDeExpiracao { get; private set; }
    public SaidaPrincipal? Saida { get; private set; }
    public bool RespostaConfirmacao { get; set; } = true;
    public int? Selo { get; private set; }

    public void MostrarUsuario(string nome, string iniciais) => Iniciais = iniciais;
    public void ExibirSecao(SecaoAdmin secao, string titulo) { Exibidas.Add(secao); Titulo = titulo; }
    public void AtualizarSelo(SecaoAdmin secao, int? quantidade) { if (secao == SecaoAdmin.Aprovacoes) Selo = quantidade; }
    public bool ConfirmarSaida() => RespostaConfirmacao;
    public void AvisarSessaoExpirada() => AvisosDeExpiracao++;
    public void Fechar(SaidaPrincipal saida) => Saida = saida;
}

public static class Fabrica
{
    public static RespostaLogin Resposta(PapelUsuario papel, DateTime? expiraEm = null)
        => new("token-teste", expiraEm ?? DateTime.UtcNow.AddHours(8),
               new UsuarioLogado(1, "Ana Souza", "ana@techquest.com", null, papel));
}
