#nullable disable

namespace TechQuest.Desktop.UI.Formularios
{
    partial class FormLogin
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlCabecalho = new Panel();
            lblSubtitulo = new Label();
            lblMarca = new Label();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblSenha = new Label();
            txtSenha = new TextBox();
            lblErro = new Label();
            btnEntrar = new Button();
            lblRodape = new Label();
            pnlCabecalho.SuspendLayout();
            SuspendLayout();
            //
            // pnlCabecalho
            //
            pnlCabecalho.BackColor = Color.FromArgb(37, 99, 235);
            pnlCabecalho.Controls.Add(lblSubtitulo);
            pnlCabecalho.Controls.Add(lblMarca);
            pnlCabecalho.Dock = DockStyle.Top;
            pnlCabecalho.Location = new Point(0, 0);
            pnlCabecalho.Name = "pnlCabecalho";
            pnlCabecalho.Size = new Size(400, 120);
            pnlCabecalho.TabIndex = 0;
            //
            // lblMarca
            //
            lblMarca.AutoSize = true;
            lblMarca.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblMarca.ForeColor = Color.White;
            lblMarca.Location = new Point(28, 24);
            lblMarca.Name = "lblMarca";
            lblMarca.Size = new Size(156, 37);
            lblMarca.TabIndex = 0;
            lblMarca.Text = "Tech Quest";
            //
            // lblSubtitulo
            //
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = new Font("Segoe UI", 11F);
            lblSubtitulo.ForeColor = Color.FromArgb(219, 234, 254);
            lblSubtitulo.Location = new Point(31, 70);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(204, 20);
            lblSubtitulo.TabIndex = 1;
            lblSubtitulo.Text = "Área administrativa — Desktop";
            //
            // lblEmail
            //
            lblEmail.AutoSize = true;
            lblEmail.ForeColor = Color.FromArgb(71, 85, 105);
            lblEmail.Location = new Point(32, 148);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(45, 19);
            lblEmail.TabIndex = 1;
            lblEmail.Text = "E-mail";
            //
            // txtEmail
            //
            txtEmail.Font = new Font("Segoe UI", 11F);
            txtEmail.Location = new Point(32, 170);
            txtEmail.MaxLength = 150;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(336, 27);
            txtEmail.TabIndex = 2;
            //
            // lblSenha
            //
            lblSenha.AutoSize = true;
            lblSenha.ForeColor = Color.FromArgb(71, 85, 105);
            lblSenha.Location = new Point(32, 216);
            lblSenha.Name = "lblSenha";
            lblSenha.Size = new Size(44, 19);
            lblSenha.TabIndex = 3;
            lblSenha.Text = "Senha";
            //
            // txtSenha
            //
            txtSenha.Font = new Font("Segoe UI", 11F);
            txtSenha.Location = new Point(32, 238);
            txtSenha.MaxLength = 100;
            txtSenha.Name = "txtSenha";
            txtSenha.Size = new Size(336, 27);
            txtSenha.TabIndex = 4;
            txtSenha.UseSystemPasswordChar = true;
            //
            // lblErro
            //
            lblErro.ForeColor = Color.FromArgb(185, 28, 28);
            lblErro.Location = new Point(32, 278);
            lblErro.Name = "lblErro";
            lblErro.Size = new Size(336, 52);
            lblErro.TabIndex = 5;
            lblErro.Visible = false;
            //
            // btnEntrar
            //
            btnEntrar.BackColor = Color.FromArgb(37, 99, 235);
            btnEntrar.Cursor = Cursors.Hand;
            btnEntrar.FlatAppearance.BorderSize = 0;
            btnEntrar.FlatStyle = FlatStyle.Flat;
            btnEntrar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnEntrar.ForeColor = Color.White;
            btnEntrar.Location = new Point(32, 336);
            btnEntrar.Name = "btnEntrar";
            btnEntrar.Size = new Size(336, 44);
            btnEntrar.TabIndex = 6;
            btnEntrar.Text = "Entrar";
            btnEntrar.UseVisualStyleBackColor = false;
            btnEntrar.Click += btnEntrar_Click;
            //
            // lblRodape
            //
            lblRodape.ForeColor = Color.FromArgb(148, 163, 184);
            lblRodape.Location = new Point(32, 396);
            lblRodape.Name = "lblRodape";
            lblRodape.Size = new Size(336, 44);
            lblRodape.TabIndex = 7;
            lblRodape.Text = "Acesso exclusivo para administradores. Tutores e estudantes usam o site ou o aplicativo.";
            //
            // FormLogin
            //
            AcceptButton = btnEntrar;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(400, 460);
            Controls.Add(lblRodape);
            Controls.Add(btnEntrar);
            Controls.Add(lblErro);
            Controls.Add(txtSenha);
            Controls.Add(lblSenha);
            Controls.Add(txtEmail);
            Controls.Add(lblEmail);
            Controls.Add(pnlCabecalho);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FormLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tech Quest — Administração";
            pnlCabecalho.ResumeLayout(false);
            pnlCabecalho.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlCabecalho;
        private Label lblMarca;
        private Label lblSubtitulo;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblSenha;
        private TextBox txtSenha;
        private Label lblErro;
        private Button btnEntrar;
        private Label lblRodape;
    }
}
