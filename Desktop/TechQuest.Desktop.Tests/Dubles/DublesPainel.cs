using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Apresentacao.Painel;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Tests.Dubles;

public sealed class PainelGatewayFalso : IPainelGateway
{
    private readonly Func<ResumoAdmin> _resposta;
    public PainelGatewayFalso(Func<ResumoAdmin> resposta) => _resposta = resposta;

    public Task<ResumoAdmin> ObterResumoAsync(CancellationToken ct = default)
        => Task.FromResult(_resposta());
}

public sealed class PainelViewFalsa : IPainelView
{
    public PainelExibicao? Exibido { get; private set; }
    public int VezesExibido { get; private set; }
    public string? Erro { get; private set; }
    public int ErrosOcultados { get; private set; }
    public List<bool> Ocupado { get; } = new();

    public void Exibir(PainelExibicao dados) { Exibido = dados; VezesExibido++; }
    public void OcultarErro() { ErrosOcultados++; Erro = null; }
    public void DefinirOcupado(bool ocupado) => Ocupado.Add(ocupado);
    public void MostrarErro(string mensagem) => Erro = mensagem;
}

public sealed class NavegadorFalso : INavegador
{
    public List<SecaoAdmin> Pedidos { get; } = new();
    public void Navegar(SecaoAdmin secao) => Pedidos.Add(secao);
}
