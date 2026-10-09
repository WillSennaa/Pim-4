using System.Globalization;
using System.Text;

namespace TechQuest.Desktop.Aplicacao.Comum;

/// <summary>
/// Busca que ignora maiusculas E acentos: "joao" encontra "João".
///
/// POR QUE NO CLIENTE: o banco usa a collation SQL_Latin1_General_CP1_CI_AS,
/// insensivel a maiuscula mas SENSIVEL a acento; uma busca feita no servidor
/// nao acharia "João" digitando "Joao". As listas do admin (usuarios, cursos,
/// logs) ja chegam inteiras ao desktop, entao filtrar aqui resolve o problema
/// sem COLLATE no SQL e sem nova requisicao a cada tecla.
///
/// ALTERNATIVA REJEITADA: endpoint de busca com COLLATE ..._AI_ no servidor.
/// Correto para listas que nao cabem na memoria; nao e o caso aqui, e exigiria
/// mudar a API que o web ja usa.
/// </summary>
public static class TextoBusca
{
    /// <summary>
    /// Verdadeiro se o termo aparece em algum dos campos. Termo vazio aceita
    /// tudo: campo de busca em branco significa "sem filtro".
    /// </summary>
    public static bool Contem(string? termo, params string?[] campos)
    {
        var t = Normalizar(termo);
        if (t.Length == 0) return true;
        return campos.Any(c => Normalizar(c).Contains(t, StringComparison.Ordinal));
    }

    /// <summary>
    /// Decompoe cada letra acentuada em letra + acento (forma NFD), descarta os
    /// acentos e passa para minusculas: "Ação" vira "acao".
    /// </summary>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var decomposto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
