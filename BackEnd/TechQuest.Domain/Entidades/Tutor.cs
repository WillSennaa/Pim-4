namespace TechQuest.Domain.Entidades;

public class Tutor
{
    public int IdTutor { get; set; }
    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public ICollection<Curso> CursosCriados { get; set; } = new List<Curso>();
}
