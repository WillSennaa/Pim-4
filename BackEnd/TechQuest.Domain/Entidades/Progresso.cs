namespace TechQuest.Domain.Entidades;

public class Progresso
{
    public int IdProgresso { get; set; }
    public int IdEstudante { get; set; }
    public Estudante Estudante { get; set; } = null!;

    public int IdMaterial { get; set; }
    public Material Material { get; set; } = null!;

    public bool Concluido { get; set; }
    public DateTime DataVisualizacao { get; set; }
    public int PorcentagemAssistida { get; set; }
}
