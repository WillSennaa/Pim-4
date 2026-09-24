namespace TechQuest.Domain.Entidades;

public class Medalha
{
    public int IdMedalha { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Raridade { get; set; }
    public string? Descricao { get; set; }

    public ICollection<Conquista> Conquistas { get; set; } = new List<Conquista>();
}
