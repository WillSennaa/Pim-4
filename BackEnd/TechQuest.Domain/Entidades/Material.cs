namespace TechQuest.Domain.Entidades;

public class Material
{
    public int IdMaterial { get; set; }
    public int IdCurso { get; set; }
    public Curso Curso { get; set; } = null!;

    public string? Titulo { get; set; }
    public string? Tipo { get; set; }

    public ICollection<Progresso> Progressos { get; set; } = new List<Progresso>();
}
