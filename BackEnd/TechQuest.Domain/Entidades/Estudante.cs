namespace TechQuest.Domain.Entidades;

public class Estudante
{
    public int IdEstudante { get; set; }
    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public ICollection<Desempenho> Desempenhos { get; set; } = new List<Desempenho>();
    public ICollection<Progresso> Progressos { get; set; } = new List<Progresso>();
    public ICollection<Conquista> Conquistas { get; set; } = new List<Conquista>();
    public ICollection<Historico> Historicos { get; set; } = new List<Historico>();
}
