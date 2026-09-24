namespace TechQuest.Domain.Entidades;

/// <summary>
/// Entidade generalizada do modelo. As especializacoes (Adm, Tutor, Estudante)
/// apontam para ela, reproduzindo a heranca do diagrama de classes do PIM III.
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }

    /// <summary>Hash BCrypt. A senha em texto puro nunca existe nesta camada.</summary>
    public string? SenhaHash { get; set; }

    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }

    public string? Telefone { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? Cidade { get; set; }

    // Especializacoes: no maximo uma delas e preenchida por usuario.
    public Adm? Adm { get; set; }
    public Tutor? Tutor { get; set; }
    public Estudante? Estudante { get; set; }

    /// <summary>Iniciais exibidas no avatar. Derivadas, nao persistidas.</summary>
    public string Iniciais()
    {
        var partes = Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "??";
        if (partes.Length == 1) return partes[0][..1].ToUpper();
        return string.Concat(partes[0][..1], partes[^1][..1]).ToUpper();
    }
}
