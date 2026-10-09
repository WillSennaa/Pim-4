namespace TechQuest.Desktop.Aplicacao.Interfaces;

/// <summary>
/// O que a camada de Integracao precisa saber da sessao: so o token.
///
/// POR QUE UMA INTERFACE TAO PEQUENA: o cliente HTTP le o token a cada
/// requisicao, mas nao deve poder iniciar nem encerrar a sessao. Expor a
/// SessaoAdmin inteira daria a ele poderes que nao lhe cabem (principio da
/// segregacao de interfaces, o "I" do SOLID).
/// </summary>
public interface IProvedorDeToken
{
    /// <summary>Token vigente, ou null se nao ha sessao ou ela venceu.</summary>
    string? Token { get; }
}
