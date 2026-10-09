namespace TechQuest.Desktop.Aplicacao.Comum;

/// <summary>
/// Converte as datas que a API devolve para o horario local da maquina.
///
/// O PROBLEMA: no Azure SQL, GETDATE() devolve hora UTC, e a API grava com
/// DateTime.UtcNow. Mas a coluna DATETIME nao guarda fuso; ao ler de volta, o
/// EF Core entrega DateTimeKind.Unspecified, e o JSON sai sem o "Z"
/// ("2026-10-09T13:00:00"). Exibido cru, um cadastro feito as 10h de
/// Brasilia apareceria as 13h.
///
/// A REGRA: toda data vinda do servidor e UTC. Unspecified e tratada como
/// UTC; Utc e convertida normalmente; Local (nao deveria chegar) fica como
/// esta.
///
/// ALTERNATIVA REJEITADA: corrigir no servidor trocando DATETIME por
/// DATETIMEOFFSET. E a solucao definitiva, mas altera o modelo congelado do
/// PIM III; a conversao no cliente resolve sem tocar no banco.
/// </summary>
public static class HorarioServidor
{
    public static DateTime ParaLocal(DateTime doServidor, TimeZoneInfo? fuso = null)
    {
        var utc = doServidor.Kind switch
        {
            DateTimeKind.Utc => doServidor,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(doServidor, DateTimeKind.Utc),
            _ => doServidor.ToUniversalTime()
        };
        return TimeZoneInfo.ConvertTimeFromUtc(utc, fuso ?? TimeZoneInfo.Local);
    }
}
