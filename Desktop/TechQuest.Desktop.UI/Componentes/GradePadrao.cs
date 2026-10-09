using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Componentes;

/// <summary>
/// DataGridView ja configurada no visual e no comportamento das listas do
/// Tech Quest: somente leitura, linha inteira selecionada, sem linha de
/// inclusao, cabecalho claro, selecao azul-clara.
///
/// POR QUE HERDAR DO DataGridView: sao mais de vinte propriedades que toda
/// lista do admin repete. Herdando, cada tela declara so as colunas; mudar o
/// estilo das listas e mexer num arquivo. A tela de Aprovacoes, anterior a
/// este componente, configura a grade no proprio designer e continua
/// funcionando igual.
/// </summary>
[System.ComponentModel.DesignerCategory("Code")]
public class GradePadrao : DataGridView
{
    public GradePadrao()
    {
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        AllowUserToResizeRows = false;
        AutoGenerateColumns = false;
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        BackgroundColor = Color.White;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        ColumnHeadersHeight = 40;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        EnableHeadersVisualStyles = false;
        GridColor = Tema.Borda;
        MultiSelect = false;
        ReadOnly = true;
        RowHeadersVisible = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        RowTemplate.Height = 38;

        ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        ColumnHeadersDefaultCellStyle.BackColor = Tema.FundoCodigo;
        ColumnHeadersDefaultCellStyle.ForeColor = Tema.TextoSecundario;
        ColumnHeadersDefaultCellStyle.SelectionBackColor = Tema.FundoCodigo;
        ColumnHeadersDefaultCellStyle.SelectionForeColor = Tema.TextoSecundario;
        ColumnHeadersDefaultCellStyle.Font = Fontes.PequenaNegrito;
        ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

        DefaultCellStyle.BackColor = Color.White;
        DefaultCellStyle.ForeColor = Tema.Texto;
        DefaultCellStyle.SelectionBackColor = Tema.PrimariaClara;
        DefaultCellStyle.SelectionForeColor = Tema.Texto;
        DefaultCellStyle.Font = Fontes.Normal;
        DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
        DefaultCellStyle.WrapMode = DataGridViewTriState.False;
    }

    /// <summary>Item da linha selecionada, ja no tipo da lista vinculada.</summary>
    public T? Selecionado<T>() where T : class => CurrentRow?.DataBoundItem as T;

    /// <summary>
    /// Seleciona a linha cujo item satisfaz o criterio (para manter a selecao
    /// depois de recarregar a lista).
    /// </summary>
    public void SelecionarOnde<T>(Func<T, bool> criterio) where T : class
    {
        foreach (DataGridViewRow linha in Rows)
        {
            if (linha.DataBoundItem is T item && criterio(item))
            {
                CurrentCell = linha.Cells.Cast<DataGridViewCell>().First(c => c.Visible);
                linha.Selected = true;
                return;
            }
        }
    }
}
