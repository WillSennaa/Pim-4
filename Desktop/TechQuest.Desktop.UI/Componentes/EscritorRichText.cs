using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Componentes;

/// <summary>
/// Escreve trechos formatados no fim de um RichTextBox.
///
/// O RichTextBox aplica fonte e cor a SELECAO: para formatar um trecho e
/// preciso posicionar o cursor no fim, definir o estilo e so entao inserir o
/// texto. Esse ritual de tres passos, repetido em cada paragrafo, ficaria
/// espalhado pelas telas; aqui ele vira uma chamada por trecho.
///
/// ALTERNATIVA REJEITADA: um WebBrowser exibindo HTML gerado. Reaproveitaria
/// o visual do web, mas o controle WebBrowser usa o motor do Internet
/// Explorer, e o WebView2 exigiria pacote NuGet e o runtime do Edge
/// instalado na maquina do admin.
/// </summary>
internal sealed class EscritorRichText
{
    private readonly RichTextBox _caixa;

    public EscritorRichText(RichTextBox caixa)
    {
        _caixa = caixa;
        _caixa.Clear();
    }

    public EscritorRichText Texto(string texto, Font? fonte = null, Color? cor = null,
                                  Color? fundo = null, int recuo = 0)
    {
        _caixa.SelectionStart = _caixa.TextLength;
        _caixa.SelectionLength = 0;
        _caixa.SelectionFont = fonte ?? Fontes.Normal;
        _caixa.SelectionColor = cor ?? Tema.Texto;
        _caixa.SelectionBackColor = fundo ?? _caixa.BackColor;
        _caixa.SelectionIndent = recuo;
        _caixa.AppendText(texto);
        return this;
    }

    public EscritorRichText Linha(string texto = "", Font? fonte = null, Color? cor = null,
                                  Color? fundo = null, int recuo = 0)
        => Texto(texto + "\n", fonte, cor, fundo, recuo);

    /// <summary>Bloco de codigo: fonte monoespacada, fundo cinza, recuado.</summary>
    public EscritorRichText Codigo(string codigo)
    {
        foreach (var linha in codigo.Replace("\r\n", "\n").Split('\n'))
            Linha(" " + linha + " ", Fontes.Codigo, Tema.Texto, Tema.FundoCodigo, recuo: 12);
        return Linha();
    }

    /// <summary>Volta a rolagem para o topo depois de montar o texto.</summary>
    public void Concluir()
    {
        _caixa.SelectionStart = 0;
        _caixa.SelectionLength = 0;
        _caixa.ScrollToCaret();
    }
}
