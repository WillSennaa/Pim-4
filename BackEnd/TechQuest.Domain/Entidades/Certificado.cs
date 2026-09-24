namespace TechQuest.Domain.Entidades;

public class Certificado
{
    public int IdCertificado { get; set; }
    public int IdHistorico { get; set; }
    public Historico Historico { get; set; } = null!;

    /// <summary>Gerado pelo banco (DEFAULT NEWID()), nao pela aplicacao.</summary>
    public Guid CodigoAutenticacao { get; set; }
    public DateTime DataEmissao { get; set; }
}
