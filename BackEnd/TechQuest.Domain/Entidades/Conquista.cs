namespace TechQuest.Domain.Entidades;

public class Conquista
{
    public int IdConquista { get; set; }
    public int IdEstudante { get; set; }
    public Estudante Estudante { get; set; } = null!;

    public int IdMedalha { get; set; }
    public Medalha Medalha { get; set; } = null!;

    public DateTime DataConquista { get; set; }
}
