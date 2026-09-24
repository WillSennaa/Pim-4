namespace TechQuest.Domain.Entidades;

public class Questao
{
    public int IdQuestao { get; set; }
    public int IdProva { get; set; }
    public Prova Prova { get; set; } = null!;

    public string Enunciado { get; set; } = string.Empty;

    /// <summary>Trecho de codigo exibido em bloco separado do enunciado.</summary>
    public string? CodigoExemplo { get; set; }

    public int Ordem { get; set; }

    public ICollection<Alternativa> Alternativas { get; set; } = new List<Alternativa>();

    public Alternativa? AlternativaCorreta() => Alternativas.FirstOrDefault(a => a.EhCorreta);
}
