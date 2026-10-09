using System.ComponentModel;
using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Componentes;

/// <summary>
/// Faixa vermelha com a mensagem de falha e o botao "Tentar novamente".
/// Escondida enquanto nao ha erro. A tela assina TentarNovamente.
/// </summary>
[DesignerCategory("Code")]
public class FaixaDeErro : UserControl
{
    private readonly Label _mensagem;
    private readonly Button _tentar;

    public event EventHandler? TentarNovamente;

    public FaixaDeErro()
    {
        BackColor = Tema.PerigoClaro;
        Padding = new Padding(8);
        Height = 56;
        Visible = false;

        _tentar = new Button
        {
            Text = "Tentar novamente",
            Dock = DockStyle.Right,
            Width = 150,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Tema.Perigo,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        _tentar.FlatAppearance.BorderColor = Tema.Perigo;
        _tentar.Click += (_, _) => TentarNovamente?.Invoke(this, EventArgs.Empty);

        _mensagem = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Tema.Perigo,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0)
        };

        Controls.Add(_mensagem); // Fill antes de Right: o Right e posicionado primeiro
        Controls.Add(_tentar);
    }

    public void Mostrar(string mensagem)
    {
        _mensagem.Text = mensagem;
        Visible = true;
    }

    public void Ocultar() => Visible = false;

    /// <summary>Desabilita o botao enquanto uma nova tentativa esta em andamento.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool BotaoHabilitado
    {
        get => _tentar.Enabled;
        set => _tentar.Enabled = value;
    }
}
