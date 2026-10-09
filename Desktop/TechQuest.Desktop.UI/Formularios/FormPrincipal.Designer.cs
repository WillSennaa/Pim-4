#nullable disable

namespace TechQuest.Desktop.UI.Formularios
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _presenter?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlMenu = new Panel();
            flpMenu = new FlowLayoutPanel();
            btnSair = new Button();
            lblMarca = new Label();
            pnlTopo = new Panel();
            lblUsuario = new Label();
            lblIniciais = new Label();
            lblTituloSecao = new Label();
            pnlLinhaTopo = new Panel();
            pnlConteudo = new Panel();
            pnlMenu.SuspendLayout();
            pnlTopo.SuspendLayout();
            SuspendLayout();
            //
            // pnlMenu
            //
            pnlMenu.BackColor = Color.FromArgb(15, 23, 42);
            pnlMenu.Controls.Add(flpMenu);
            pnlMenu.Controls.Add(btnSair);
            pnlMenu.Controls.Add(lblMarca);
            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.Location = new Point(0, 0);
            pnlMenu.Name = "pnlMenu";
            pnlMenu.Size = new Size(230, 760);
            pnlMenu.TabIndex = 0;
            //
            // flpMenu
            //
            flpMenu.Dock = DockStyle.Fill;
            flpMenu.FlowDirection = FlowDirection.TopDown;
            flpMenu.Location = new Point(0, 72);
            flpMenu.Name = "flpMenu";
            flpMenu.Padding = new Padding(12, 8, 12, 8);
            flpMenu.Size = new Size(230, 640);
            flpMenu.TabIndex = 1;
            flpMenu.WrapContents = false;
            //
            // btnSair
            //
            btnSair.Cursor = Cursors.Hand;
            btnSair.Dock = DockStyle.Bottom;
            btnSair.FlatAppearance.BorderSize = 0;
            btnSair.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btnSair.FlatStyle = FlatStyle.Flat;
            btnSair.ForeColor = Color.FromArgb(203, 213, 225);
            btnSair.Location = new Point(0, 712);
            btnSair.Name = "btnSair";
            btnSair.Padding = new Padding(24, 0, 0, 0);
            btnSair.Size = new Size(230, 48);
            btnSair.TabIndex = 2;
            btnSair.Text = "Sair";
            btnSair.TextAlign = ContentAlignment.MiddleLeft;
            btnSair.UseVisualStyleBackColor = false;
            btnSair.Click += btnSair_Click;
            //
            // lblMarca
            //
            lblMarca.Dock = DockStyle.Top;
            lblMarca.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblMarca.ForeColor = Color.White;
            lblMarca.Location = new Point(0, 0);
            lblMarca.Name = "lblMarca";
            lblMarca.Padding = new Padding(20, 0, 0, 0);
            lblMarca.Size = new Size(230, 72);
            lblMarca.TabIndex = 0;
            lblMarca.Text = "Tech Quest";
            lblMarca.TextAlign = ContentAlignment.MiddleLeft;
            //
            // pnlTopo
            //
            pnlTopo.BackColor = Color.White;
            pnlTopo.Controls.Add(lblUsuario);
            pnlTopo.Controls.Add(lblIniciais);
            pnlTopo.Controls.Add(lblTituloSecao);
            pnlTopo.Controls.Add(pnlLinhaTopo);
            pnlTopo.Dock = DockStyle.Top;
            pnlTopo.Location = new Point(230, 0);
            pnlTopo.Name = "pnlTopo";
            pnlTopo.Size = new Size(970, 64);
            pnlTopo.TabIndex = 1;
            //
            // lblUsuario
            //
            lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUsuario.ForeColor = Color.FromArgb(71, 85, 105);
            lblUsuario.Location = new Point(632, 14);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(270, 36);
            lblUsuario.TabIndex = 1;
            lblUsuario.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblIniciais
            //
            lblIniciais.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblIniciais.BackColor = Color.FromArgb(219, 234, 254);
            lblIniciais.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblIniciais.ForeColor = Color.FromArgb(30, 64, 175);
            lblIniciais.Location = new Point(910, 14);
            lblIniciais.Name = "lblIniciais";
            lblIniciais.Size = new Size(36, 36);
            lblIniciais.TabIndex = 2;
            lblIniciais.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblTituloSecao
            //
            lblTituloSecao.AutoSize = true;
            lblTituloSecao.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloSecao.ForeColor = Color.FromArgb(15, 23, 42);
            lblTituloSecao.Location = new Point(24, 16);
            lblTituloSecao.Name = "lblTituloSecao";
            lblTituloSecao.Size = new Size(0, 28);
            lblTituloSecao.TabIndex = 0;
            //
            // pnlLinhaTopo
            //
            pnlLinhaTopo.BackColor = Color.FromArgb(226, 232, 240);
            pnlLinhaTopo.Dock = DockStyle.Bottom;
            pnlLinhaTopo.Location = new Point(0, 63);
            pnlLinhaTopo.Name = "pnlLinhaTopo";
            pnlLinhaTopo.Size = new Size(970, 1);
            pnlLinhaTopo.TabIndex = 3;
            //
            // pnlConteudo
            //
            pnlConteudo.AutoScroll = true;
            pnlConteudo.BackColor = Color.FromArgb(241, 245, 249);
            pnlConteudo.Dock = DockStyle.Fill;
            pnlConteudo.Location = new Point(230, 64);
            pnlConteudo.Name = "pnlConteudo";
            pnlConteudo.Padding = new Padding(24);
            pnlConteudo.Size = new Size(970, 696);
            pnlConteudo.TabIndex = 2;
            //
            // FormPrincipal
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(241, 245, 249);
            ClientSize = new Size(1200, 760);
            Controls.Add(pnlConteudo);
            Controls.Add(pnlTopo);
            Controls.Add(pnlMenu);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1000, 640);
            Name = "FormPrincipal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tech Quest — Administração";
            pnlMenu.ResumeLayout(false);
            pnlTopo.ResumeLayout(false);
            pnlTopo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlMenu;
        private FlowLayoutPanel flpMenu;
        private Button btnSair;
        private Label lblMarca;
        private Panel pnlTopo;
        private Label lblUsuario;
        private Label lblIniciais;
        private Label lblTituloSecao;
        private Panel pnlLinhaTopo;
        private Panel pnlConteudo;
    }
}
