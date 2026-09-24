namespace TechQuest.Domain.Entidades;

public class Alternativa
{
    public int IdAlternativa { get; set; }
    public int IdQuestao { get; set; }
    public Questao Questao { get; set; } = null!;

    public string? Letra { get; set; }
    public string? Texto { get; set; }

    /// <summary>
    /// Gabarito. Existe no dominio e no banco, mas NUNCA e projetado em DTO
    /// enviado ao cliente enquanto a prova esta em andamento.
    /// </summary>
    public bool EhCorreta { get; set; }
}
