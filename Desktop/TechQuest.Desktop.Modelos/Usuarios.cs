namespace TechQuest.Desktop.Modelos;

/// <summary>
/// Linha de GET /api/admin/usuarios.
///
/// Papel chega como TEXTO ("Admin", "Tutor", "Estudante"), diferente do login,
/// que manda o enum como numero. Sao dois DTOs distintos no servidor; o
/// desktop espelha cada um como ele e, e converte com PapelDe quando precisa.
/// </summary>
public record UsuarioAdmin(
    int Id,
    string Nome,
    string? Email,
    string Papel,
    bool Ativo,
    DateTime DataCadastro)
{
    public PapelUsuario? PapelDe()
        => Enum.TryParse<PapelUsuario>(Papel, ignoreCase: true, out var p) ? p : null;
}

/// <summary>Corpo de POST /api/admin/usuarios. O papel vai como texto.</summary>
public record NovoUsuario(string Nome, string Email, string Senha, string Papel);

/// <summary>Corpo de PATCH /api/admin/usuarios/{id}/status.</summary>
public record AlteracaoStatus(bool Ativo);

/// <summary>Linha de GET /api/admin/logs.</summary>
public record LogAuditoria(int Id, int IdAdm, string? Administrador, string? Acao, DateTime Data);
