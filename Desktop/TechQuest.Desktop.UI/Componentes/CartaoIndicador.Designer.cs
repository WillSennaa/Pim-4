#nullable disable

namespace TechQuest.Desktop.UI.Componentes
{
    partial class CartaoIndicador
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            lblTitulo = new Label();
            lblValor = new Label();
            lblDetalhe = new Label();
            SuspendLayout();
            //
            // lblTitulo
            //
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.Font = new Font("Segoe UI", 9.5F);
            lblTitulo.ForeColor = Color.FromArgb(71, 85, 105);
            lblTitulo.Location = new Point(16, 14);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(188, 22);
            lblTitulo.TabIndex = 0;
            //
            // lblValor
            //
            lblValor.Dock = DockStyle.Top;
            lblValor.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblValor.ForeColor = Color.FromArgb(15, 23, 42);
            lblValor.Location = new Point(16, 36);
            lblValor.Name = "lblValor";
            lblValor.Size = new Size(188, 44);
            lblValor.TabIndex = 1;
            lblValor.Text = "—";
            //
            // lblDetalhe
            //
            lblDetalhe.Dock = DockStyle.Top;
            lblDetalhe.Font = new Font("Segoe UI", 9F);
            lblDetalhe.ForeColor = Color.FromArgb(148, 163, 184);
            lblDetalhe.Location = new Point(16, 80);
            lblDetalhe.Name = "lblDetalhe";
            lblDetalhe.Size = new Size(188, 20);
            lblDetalhe.TabIndex = 2;
            //
            // CartaoIndicador
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(lblDetalhe);
            Controls.Add(lblValor);
            Controls.Add(lblTitulo);
            Font = new Font("Segoe UI", 10F);
            Margin = new Padding(8);
            Name = "CartaoIndicador";
            Padding = new Padding(16, 14, 16, 8);
            Size = new Size(220, 112);
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitulo;
        private Label lblValor;
        private Label lblDetalhe;
    }
}
