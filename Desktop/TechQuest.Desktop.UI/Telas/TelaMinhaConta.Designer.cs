#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaMinhaConta
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
            pnlPerfil = new Panel();
            lblDetalhes = new Label();
            lblEmail = new Label();
            lblNome = new Label();
            lblIniciais = new Label();
            pnlEspaco = new Panel();
            pnlFormularios = new Panel();
            grpEmail = new GroupBox();
            btnTrocarEmail = new Button();
            lblNotaEmail = new Label();
            txtSenhaEmail = new TextBox();
            lblSenhaEmail = new Label();
            txtEmailNovo = new TextBox();
            lblEmailNovo = new Label();
            grpSenha = new GroupBox();
            btnTrocarSenha = new Button();
            txtConfirmacao = new TextBox();
            lblConfirmacao = new Label();
            txtSenhaNova = new TextBox();
            lblSenhaNova = new Label();
            txtSenhaAtual = new TextBox();
            lblSenhaAtual = new Label();
            lblMensagem = new Label();
            lblNota = new Label();
            pnlPerfil.SuspendLayout();
            pnlFormularios.SuspendLayout();
            grpEmail.SuspendLayout();
            grpSenha.SuspendLayout();
            SuspendLayout();
            //
            // pnlPerfil
            //
            pnlPerfil.Controls.Add(lblDetalhes);
            pnlPerfil.Controls.Add(lblEmail);
            pnlPerfil.Controls.Add(lblNome);
            pnlPerfil.Controls.Add(lblIniciais);
            pnlPerfil.Dock = DockStyle.Top;
            pnlPerfil.BackColor = Color.White;
            pnlPerfil.Size = new Size(900, 112);
            pnlPerfil.Name = "pnlPerfil";
            pnlPerfil.TabIndex = 0;
            //
            // lblDetalhes
            //
            lblDetalhes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblDetalhes.ForeColor = Color.FromArgb(148, 163, 184);
            lblDetalhes.Location = new Point(106, 80);
            lblDetalhes.Size = new Size(770, 22);
            lblDetalhes.Text = "";
            lblDetalhes.Name = "lblDetalhes";
            lblDetalhes.TabIndex = 3;
            //
            // lblEmail
            //
            lblEmail.AutoSize = true;
            lblEmail.ForeColor = Color.FromArgb(71, 85, 105);
            lblEmail.Location = new Point(106, 54);
            lblEmail.Text = "";
            lblEmail.Name = "lblEmail";
            lblEmail.TabIndex = 2;
            //
            // lblNome
            //
            lblNome.AutoSize = true;
            lblNome.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblNome.ForeColor = Color.FromArgb(15, 23, 42);
            lblNome.Location = new Point(104, 18);
            lblNome.Text = "Carregando...";
            lblNome.Name = "lblNome";
            lblNome.TabIndex = 1;
            //
            // lblIniciais
            //
            lblIniciais.BackColor = Color.FromArgb(219, 234, 254);
            lblIniciais.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblIniciais.ForeColor = Color.FromArgb(30, 64, 175);
            lblIniciais.Location = new Point(24, 24);
            lblIniciais.Size = new Size(64, 64);
            lblIniciais.TextAlign = ContentAlignment.MiddleCenter;
            lblIniciais.Name = "lblIniciais";
            lblIniciais.TabIndex = 0;
            //
            // pnlEspaco
            //
            pnlEspaco.Dock = DockStyle.Top;
            pnlEspaco.Size = new Size(900, 16);
            pnlEspaco.Name = "pnlEspaco";
            pnlEspaco.TabIndex = 1;
            //
            // pnlFormularios
            //
            pnlFormularios.Controls.Add(grpEmail);
            pnlFormularios.Controls.Add(grpSenha);
            pnlFormularios.Dock = DockStyle.Top;
            pnlFormularios.Size = new Size(900, 250);
            pnlFormularios.Name = "pnlFormularios";
            pnlFormularios.TabIndex = 2;
            //
            // grpEmail
            //
            grpEmail.Controls.Add(btnTrocarEmail);
            grpEmail.Controls.Add(lblNotaEmail);
            grpEmail.Controls.Add(txtSenhaEmail);
            grpEmail.Controls.Add(lblSenhaEmail);
            grpEmail.Controls.Add(txtEmailNovo);
            grpEmail.Controls.Add(lblEmailNovo);
            grpEmail.BackColor = Color.White;
            grpEmail.ForeColor = Color.FromArgb(71, 85, 105);
            grpEmail.Location = new Point(446, 0);
            grpEmail.Padding = new Padding(12);
            grpEmail.Size = new Size(430, 246);
            grpEmail.Text = "Trocar e-mail de login";
            grpEmail.Name = "grpEmail";
            grpEmail.TabIndex = 1;
            //
            // btnTrocarEmail
            //
            btnTrocarEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnTrocarEmail.BackColor = Color.FromArgb(37, 99, 235);
            btnTrocarEmail.Cursor = Cursors.Hand;
            btnTrocarEmail.FlatAppearance.BorderSize = 0;
            btnTrocarEmail.FlatStyle = FlatStyle.Flat;
            btnTrocarEmail.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTrocarEmail.ForeColor = Color.White;
            btnTrocarEmail.Location = new Point(16, 198);
            btnTrocarEmail.Size = new Size(180, 34);
            btnTrocarEmail.Text = "Trocar e-mail";
            btnTrocarEmail.UseVisualStyleBackColor = false;
            btnTrocarEmail.Name = "btnTrocarEmail";
            btnTrocarEmail.TabIndex = 4;
            btnTrocarEmail.Click += btnTrocarEmail_Click;
            //
            // lblNotaEmail
            //
            lblNotaEmail.ForeColor = Color.FromArgb(148, 163, 184);
            lblNotaEmail.Location = new Point(16, 146);
            lblNotaEmail.Size = new Size(396, 40);
            lblNotaEmail.Text = "A sessão continua aberta depois da troca: não é preciso entrar de novo.";
            lblNotaEmail.Name = "lblNotaEmail";
            lblNotaEmail.TabIndex = 5;
            //
            // txtSenhaEmail
            //
            txtSenhaEmail.Location = new Point(16, 110);
            txtSenhaEmail.MaxLength = 100;
            txtSenhaEmail.Size = new Size(396, 26);
            txtSenhaEmail.UseSystemPasswordChar = true;
            txtSenhaEmail.Name = "txtSenhaEmail";
            txtSenhaEmail.TabIndex = 3;
            //
            // lblSenhaEmail
            //
            lblSenhaEmail.AutoSize = true;
            lblSenhaEmail.ForeColor = Color.FromArgb(71, 85, 105);
            lblSenhaEmail.Location = new Point(16, 88);
            lblSenhaEmail.Text = "Senha atual (confirmação)";
            lblSenhaEmail.Name = "lblSenhaEmail";
            lblSenhaEmail.TabIndex = 2;
            //
            // txtEmailNovo
            //
            txtEmailNovo.Location = new Point(16, 52);
            txtEmailNovo.MaxLength = 100;
            txtEmailNovo.Size = new Size(396, 26);
            txtEmailNovo.Name = "txtEmailNovo";
            txtEmailNovo.TabIndex = 1;
            //
            // lblEmailNovo
            //
            lblEmailNovo.AutoSize = true;
            lblEmailNovo.ForeColor = Color.FromArgb(71, 85, 105);
            lblEmailNovo.Location = new Point(16, 30);
            lblEmailNovo.Text = "Novo e-mail";
            lblEmailNovo.Name = "lblEmailNovo";
            lblEmailNovo.TabIndex = 0;
            //
            // grpSenha
            //
            grpSenha.Controls.Add(btnTrocarSenha);
            grpSenha.Controls.Add(txtConfirmacao);
            grpSenha.Controls.Add(lblConfirmacao);
            grpSenha.Controls.Add(txtSenhaNova);
            grpSenha.Controls.Add(lblSenhaNova);
            grpSenha.Controls.Add(txtSenhaAtual);
            grpSenha.Controls.Add(lblSenhaAtual);
            grpSenha.BackColor = Color.White;
            grpSenha.ForeColor = Color.FromArgb(71, 85, 105);
            grpSenha.Location = new Point(0, 0);
            grpSenha.Padding = new Padding(12);
            grpSenha.Size = new Size(430, 246);
            grpSenha.Text = "Trocar senha";
            grpSenha.Name = "grpSenha";
            grpSenha.TabIndex = 0;
            //
            // btnTrocarSenha
            //
            btnTrocarSenha.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnTrocarSenha.BackColor = Color.FromArgb(37, 99, 235);
            btnTrocarSenha.Cursor = Cursors.Hand;
            btnTrocarSenha.FlatAppearance.BorderSize = 0;
            btnTrocarSenha.FlatStyle = FlatStyle.Flat;
            btnTrocarSenha.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTrocarSenha.ForeColor = Color.White;
            btnTrocarSenha.Location = new Point(16, 198);
            btnTrocarSenha.Size = new Size(180, 34);
            btnTrocarSenha.Text = "Trocar senha";
            btnTrocarSenha.UseVisualStyleBackColor = false;
            btnTrocarSenha.Name = "btnTrocarSenha";
            btnTrocarSenha.TabIndex = 6;
            btnTrocarSenha.Click += btnTrocarSenha_Click;
            //
            // txtConfirmacao
            //
            txtConfirmacao.Location = new Point(16, 166);
            txtConfirmacao.MaxLength = 100;
            txtConfirmacao.Size = new Size(396, 26);
            txtConfirmacao.UseSystemPasswordChar = true;
            txtConfirmacao.Name = "txtConfirmacao";
            txtConfirmacao.TabIndex = 5;
            //
            // lblConfirmacao
            //
            lblConfirmacao.AutoSize = true;
            lblConfirmacao.ForeColor = Color.FromArgb(71, 85, 105);
            lblConfirmacao.Location = new Point(16, 146);
            lblConfirmacao.Text = "Repita a nova senha";
            lblConfirmacao.Name = "lblConfirmacao";
            lblConfirmacao.TabIndex = 4;
            //
            // txtSenhaNova
            //
            txtSenhaNova.Location = new Point(16, 110);
            txtSenhaNova.MaxLength = 100;
            txtSenhaNova.Size = new Size(396, 26);
            txtSenhaNova.UseSystemPasswordChar = true;
            txtSenhaNova.Name = "txtSenhaNova";
            txtSenhaNova.TabIndex = 3;
            //
            // lblSenhaNova
            //
            lblSenhaNova.AutoSize = true;
            lblSenhaNova.ForeColor = Color.FromArgb(71, 85, 105);
            lblSenhaNova.Location = new Point(16, 88);
            lblSenhaNova.Text = "Nova senha (mínimo de 6 caracteres)";
            lblSenhaNova.Name = "lblSenhaNova";
            lblSenhaNova.TabIndex = 2;
            //
            // txtSenhaAtual
            //
            txtSenhaAtual.Location = new Point(16, 52);
            txtSenhaAtual.MaxLength = 100;
            txtSenhaAtual.Size = new Size(396, 26);
            txtSenhaAtual.UseSystemPasswordChar = true;
            txtSenhaAtual.Name = "txtSenhaAtual";
            txtSenhaAtual.TabIndex = 1;
            //
            // lblSenhaAtual
            //
            lblSenhaAtual.AutoSize = true;
            lblSenhaAtual.ForeColor = Color.FromArgb(71, 85, 105);
            lblSenhaAtual.Location = new Point(16, 30);
            lblSenhaAtual.Text = "Senha atual";
            lblSenhaAtual.Name = "lblSenhaAtual";
            lblSenhaAtual.TabIndex = 0;
            //
            // lblMensagem
            //
            lblMensagem.Dock = DockStyle.Top;
            lblMensagem.Padding = new Padding(0, 12, 0, 0);
            lblMensagem.Size = new Size(900, 48);
            lblMensagem.Name = "lblMensagem";
            lblMensagem.TabIndex = 3;
            //
            // lblNota
            //
            lblNota.Dock = DockStyle.Top;
            lblNota.ForeColor = Color.FromArgb(148, 163, 184);
            lblNota.Size = new Size(900, 44);
            lblNota.Text = "Contas de administrador não podem ser desativadas pelo próprio dono, para a plataforma nunca ficar sem quem reative usuários. Outro administrador faz isso pela tela Usuários.";
            lblNota.Name = "lblNota";
            lblNota.TabIndex = 4;
            //
            // TelaMinhaConta
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblNota);
            Controls.Add(lblMensagem);
            Controls.Add(pnlFormularios);
            Controls.Add(pnlEspaco);
            Controls.Add(pnlPerfil);
            AutoScroll = true;
            BackColor = Color.FromArgb(241, 245, 249);
            Font = new Font("Segoe UI", 10F);
            Size = new Size(900, 640);
            Name = "TelaMinhaConta";
            pnlPerfil.ResumeLayout(false);
            pnlPerfil.PerformLayout();
            pnlFormularios.ResumeLayout(false);
            pnlFormularios.PerformLayout();
            grpEmail.ResumeLayout(false);
            grpEmail.PerformLayout();
            grpSenha.ResumeLayout(false);
            grpSenha.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlPerfil;
        private Label lblDetalhes;
        private Label lblEmail;
        private Label lblNome;
        private Label lblIniciais;
        private Panel pnlEspaco;
        private Panel pnlFormularios;
        private GroupBox grpEmail;
        private Button btnTrocarEmail;
        private Label lblNotaEmail;
        private TextBox txtSenhaEmail;
        private Label lblSenhaEmail;
        private TextBox txtEmailNovo;
        private Label lblEmailNovo;
        private GroupBox grpSenha;
        private Button btnTrocarSenha;
        private TextBox txtConfirmacao;
        private Label lblConfirmacao;
        private TextBox txtSenhaNova;
        private Label lblSenhaNova;
        private TextBox txtSenhaAtual;
        private Label lblSenhaAtual;
        private Label lblMensagem;
        private Label lblNota;
    }
}
