namespace TechQuest.Desktop.Modelos;

/// <summary>Corpo de POST /api/auth/login.</summary>
public record CredenciaisLogin(string Email, string Senha);

/// <summary>
/// Resposta de POST /api/auth/login.
///
/// Espelha LoginResponse do servidor, mas so com o que o desktop usa. O
/// System.Text.Json ignora campos do JSON que nao existem aqui (Xp, Nivel...),
/// entao a API pode crescer sem quebrar este cliente.
/// </summary>
public record RespostaLogin(string Token, DateTime ExpiraEm, UsuarioLogado Usuario);

public record UsuarioLogado(
    int Id,
    string Nome,
    string? Email,
    string? Iniciais,
    PapelUsuario Papel);
