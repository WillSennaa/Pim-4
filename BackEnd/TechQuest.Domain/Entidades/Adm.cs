namespace TechQuest.Domain.Entidades;

public class Adm
{
    public int IdAdm { get; set; }
    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public ICollection<Curso> CursosAvaliados { get; set; } = new List<Curso>();
    public ICollection<LogAuditoria> Logs { get; set; } = new List<LogAuditoria>();
}
