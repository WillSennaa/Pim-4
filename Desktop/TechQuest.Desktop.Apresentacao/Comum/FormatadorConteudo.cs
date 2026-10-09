namespace TechQuest.Desktop.Apresentacao.Comum;

/// <summary>Trecho de um material: paragrafo de texto ou bloco de codigo.</summary>
public sealed record BlocoConteudo(string Texto, bool EhCodigo);

/// <summary>
/// Divide o texto de um material em paragrafos e blocos de codigo, com a
/// MESMA convencao do web (formatarConteudoMaterial em app.js):
///   - blocos sao separados por linha em branco;
///   - um bloco em que TODA linha nao vazia comeca com quatro espacos e
///     codigo, e os quatro espacos sao removidos.
///
/// POR QUE REPETIR A REGRA AQUI: o tutor escreve o material uma vez, e ele
/// precisa aparecer igual no web e no desktop. O texto e guardado puro no
/// banco (coluna Conteudo); cada cliente o interpreta com a mesma regra.
///
/// ALTERNATIVA REJEITADA: exibir o texto cru numa TextBox. Funciona, mas o
/// avaliador nao distinguiria o exemplo de codigo do texto corrido, que e
/// justamente o que ele precisa revisar com atencao.
/// </summary>
public static class FormatadorConteudo
{
    private const string Recuo = "    ";

    public static IReadOnlyList<BlocoConteudo> Blocos(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return Array.Empty<BlocoConteudo>();

        var normalizado = texto.Replace("\r\n", "\n");
        var blocos = new List<BlocoConteudo>();

        foreach (var bruto in SepararPorLinhaEmBranco(normalizado))
        {
            var linhas = bruto.Split('\n');
            var naoVazias = linhas.Where(l => l.Trim().Length > 0).ToList();
            if (naoVazias.Count == 0) continue;

            var ehCodigo = naoVazias.All(l => l.StartsWith(Recuo, StringComparison.Ordinal));
            if (ehCodigo)
            {
                var codigo = string.Join("\n", linhas.Select(l =>
                    l.StartsWith(Recuo, StringComparison.Ordinal) ? l[Recuo.Length..] : l)).Trim('\n');
                blocos.Add(new BlocoConteudo(codigo, true));
            }
            else
            {
                blocos.Add(new BlocoConteudo(bruto.Trim(), false));
            }
        }

        return blocos;
    }

    /// <summary>Equivale ao split(/\n\s*\n/) do web: linha "em branco" pode ter espacos.</summary>
    private static IEnumerable<string> SepararPorLinhaEmBranco(string texto)
    {
        var atual = new List<string>();
        foreach (var linha in texto.Split('\n'))
        {
            if (linha.Trim().Length == 0)
            {
                if (atual.Count > 0) yield return string.Join("\n", atual);
                atual.Clear();
            }
            else
            {
                atual.Add(linha);
            }
        }
        if (atual.Count > 0) yield return string.Join("\n", atual);
    }
}
