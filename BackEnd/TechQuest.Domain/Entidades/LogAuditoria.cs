namespace TechQuest.Domain.Entidades;

public class LogAuditoria
{
    public int IdLog { get; set; }
    public int IdAdm { get; set; }
    public Adm Adm { get; set; } = null!;

    public string? AcaoRealizada { get; set; }
    public DateTime DataAcao { get; set; }
}
