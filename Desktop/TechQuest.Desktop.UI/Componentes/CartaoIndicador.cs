using System.ComponentModel;
using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Componentes;

/// <summary>
/// Cartao de indicador: titulo pequeno, numero grande, detalhe opcional.
/// Equivale ao ".kpi" do CSS do web.
///
/// POR QUE UM COMPONENTE: o painel tem nove cartoes iguais. Montar cada um
/// com tres Labels soltos seriam 27 controles para alinhar a mao no designer,
/// e qualquer ajuste de estilo teria de ser repetido nove vezes. Como
/// UserControl, o cartao e arrastavel da Toolbox e o estilo vive aqui.
/// </summary>
public partial class CartaoIndicador : UserControl
{
    public CartaoIndicador()
    {
        InitializeComponent();
        SetStyle(ControlStyles.ResizeRedraw, true); // redesenha a borda ao redimensionar
    }

    [Category("Tech Quest"), Description("Rótulo acima do número.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue("")]
    public string Titulo
    {
        get => lblTitulo.Text;
        set => lblTitulo.Text = value;
    }

    [Category("Tech Quest"), Description("Número em destaque.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue("—")]
    public string Valor
    {
        get => lblValor.Text;
        set => lblValor.Text = value;
    }

    [Category("Tech Quest"), Description("Texto complementar abaixo do número.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue("")]
    public string Detalhe
    {
        get => lblDetalhe.Text;
        set => lblDetalhe.Text = value;
    }

    /// <summary>
    /// Cor do numero, trocada em tempo de execucao (alerta quando ha
    /// pendencia). Oculta do designer: nao e configuracao fixa do cartao.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color CorValor
    {
        get => lblValor.ForeColor;
        set => lblValor.ForeColor = value;
    }

    /// <summary>Borda de 1px na cor --color-border do web.</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var caneta = new Pen(Tema.Borda);
        e.Graphics.DrawRectangle(caneta, 0, 0, Width - 1, Height - 1);
    }
}
