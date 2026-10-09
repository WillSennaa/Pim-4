using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>
/// Marcador provisorio das secoes ainda nao implementadas. Sai do projeto
/// quando a ultima tela real entrar. Montado em codigo, sem designer, por ser
/// descartavel.
/// </summary>
[System.ComponentModel.DesignerCategory("Code")]
public sealed class TelaEmConstrucao : UserControl
{
    public TelaEmConstrucao(string titulo)
    {
        BackColor = Tema.Fundo;
        Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Tema.TextoSuave,
            Font = new Font("Segoe UI", 12F),
            Text = $"{titulo}\n\nEsta seção será implementada nas próximas entregas."
        });
    }
}
