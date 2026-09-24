namespace TechQuest.Application.Dtos;

/// <summary>
/// Resultado de uma operacao de negocio.
///
/// POR QUE: sem isto, "nao encontrado", "sem permissao" e "estado invalido"
/// voltariam todos como null, e o controller teria de adivinhar qual HTTP
/// devolver. A decisao de negocio fica no servico; o controller so traduz.
/// </summary>
public enum StatusOperacao
{
    Ok,
    NaoEncontrado,
    SemPermissao,
    Invalido
}

public record Resultado<T>(StatusOperacao Status, T? Valor = default, string? Mensagem = null)
{
    public static Resultado<T> Ok(T valor) => new(StatusOperacao.Ok, valor);
    public static Resultado<T> NaoEncontrado(string msg = "Registro nao encontrado.")
        => new(StatusOperacao.NaoEncontrado, default, msg);
    public static Resultado<T> SemPermissao(string msg = "Operacao nao permitida para este usuario.")
        => new(StatusOperacao.SemPermissao, default, msg);
    public static Resultado<T> Invalido(string msg) => new(StatusOperacao.Invalido, default, msg);
}
