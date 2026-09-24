namespace TechQuest.Domain.Entidades;

/// <summary>
/// Uma linha por tentativa realizada. O campo Tentativas guarda o numero
/// sequencial daquela tentativa (1a, 2a, 3a...), nao um contador global.
/// </summary>
public class Desempenho
{
    public int IdDesempenho { get; set; }
    public int IdEstudante { get; set; }
    public Estudante Estudante { get; set; } = null!;

    public int IdProva { get; set; }
    public Prova Prova { get; set; } = null!;

    public decimal? Nota { get; set; }
    public DateTime DataRealizacao { get; set; }
    public int Tentativas { get; set; }
}
