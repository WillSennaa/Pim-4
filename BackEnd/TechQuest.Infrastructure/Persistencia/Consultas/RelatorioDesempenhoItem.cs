using Microsoft.EntityFrameworkCore;

namespace TechQuest.Infrastructure.Persistencia.Consultas;

/// <summary>
/// Tipo SEM CHAVE, mapeado apenas para receber o resultado da procedure
/// SP_RelatorioDesempenhoEstudante. Nao e entidade de dominio: nao tem
/// identidade, nao e rastreado pelo EF e nao corresponde a uma tabela.
/// Por isso vive na Infrastructure, nao no Domain.
///
/// Os nomes das propriedades seguem os aliases do SELECT da procedure.
/// </summary>
[Keyless]
public class RelatorioDesempenhoItem
{
    public string Estudante { get; set; } = string.Empty;
    public string? Prova { get; set; }
    public decimal? Nota { get; set; }
    public int Tentativas { get; set; }
    public DateTime Data_Realizacao { get; set; }
}
