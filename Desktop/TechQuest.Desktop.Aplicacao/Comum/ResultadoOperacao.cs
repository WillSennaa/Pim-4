namespace TechQuest.Desktop.Aplicacao.Comum;

/// <summary>
/// Desfecho de uma operacao de negocio que pode ser recusada de forma
/// PREVISTA: senha errada, campo vazio, conta sem papel de admin.
///
/// RESULTADO x EXCECAO, mesma divisao do back-end (Resultado&lt;T&gt; nos
/// servicos e TratamentoDeErrosMiddleware para o resto): o que faz parte do
/// fluxo normal volta como valor; o que e falha de ambiente (rede, servidor
/// fora) sobe como excecao. Errar a senha nao e excepcional; e esperado.
/// </summary>
public sealed record ResultadoOperacao(bool Sucesso, string? Mensagem)
{
    public static ResultadoOperacao Ok() => new(true, null);
    public static ResultadoOperacao Falha(string mensagem) => new(false, mensagem);
}
