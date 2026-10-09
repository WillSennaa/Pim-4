namespace TechQuest.Desktop.Aplicacao.Comum;

/// <summary>
/// ResultadoOperacao que tambem carrega um valor em caso de sucesso (o
/// usuario recem-criado, por exemplo). Mesma ideia do Resultado&lt;T&gt; do
/// back-end.
/// </summary>
public sealed record ResultadoOperacao<T>(bool Sucesso, T? Valor, string? Mensagem)
{
    public static ResultadoOperacao<T> Ok(T valor) => new(true, valor, null);
    public static ResultadoOperacao<T> Falha(string mensagem) => new(false, default, mensagem);
}
