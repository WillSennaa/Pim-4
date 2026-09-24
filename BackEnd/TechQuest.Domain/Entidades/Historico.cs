namespace TechQuest.Domain.Entidades;

public class Historico
{
    public int IdHistorico { get; set; }
    public int IdEstudante { get; set; }
    public Estudante Estudante { get; set; } = null!;

    public int IdCurso { get; set; }
    public Curso Curso { get; set; } = null!;

    public string? StatusConclusao { get; set; }
    public DateOnly? DataConclusao { get; set; }

    public Certificado? Certificado { get; set; }
}
