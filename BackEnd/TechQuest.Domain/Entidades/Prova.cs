namespace TechQuest.Domain.Entidades;

public class Prova
{
    public int IdProva { get; set; }
    public string? Titulo { get; set; }
    public decimal NotaMinima { get; set; }
    public int TempoMinutos { get; set; }

    public ICollection<Questao> Questoes { get; set; } = new List<Questao>();
    public ICollection<Desempenho> Desempenhos { get; set; } = new List<Desempenho>();

    /// <summary>
    /// Regra de aprovacao no dominio, nao no controller: a nota minima vem do
    /// banco e a comparacao vive junto da entidade que a possui.
    /// </summary>
    public bool Aprovado(decimal nota) => nota >= NotaMinima;
}
