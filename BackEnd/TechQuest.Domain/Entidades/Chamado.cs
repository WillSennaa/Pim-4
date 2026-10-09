namespace TechQuest.Domain.Entidades;

public class Chamado
{
    public int IdChamado { get; set; }

    public int IdRemetente { get; set; }
    public Usuario Remetente { get; set; } = null!;

    public int? IdDestinatario { get; set; }
    public Usuario? Destinatario { get; set; }

    /// <summary>
    /// Curso a que a duvida se refere. Nulo em chamado tecnico, que nao e
    /// sobre conteudo. E o que permite rotear a duvida ao tutor certo.
    /// </summary>
    public int? IdCurso { get; set; }
    public Curso? Curso { get; set; }

    public string? Tipo { get; set; }
    public string? Assunto { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataAbertura { get; set; }
    public string Status { get; set; } = "Aberto";
}
