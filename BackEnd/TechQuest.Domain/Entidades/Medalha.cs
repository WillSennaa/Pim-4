namespace TechQuest.Domain.Entidades;

public class Medalha
{
    public int IdMedalha { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Raridade { get; set; }
    public string? Descricao { get; set; }

    /// <summary>
    /// Curso a que a medalha pertence. Preenchido = medalha de conclusao
    /// daquele curso, concedida automaticamente; nulo = medalha geral
    /// (criterio por contagem ou concessao manual pelo tutor).
    /// </summary>
    public int? IdCurso { get; set; }
    public Curso? Curso { get; set; }

    public ICollection<Conquista> Conquistas { get; set; } = new List<Conquista>();
}
