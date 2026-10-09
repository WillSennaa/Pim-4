#nullable disable

namespace TechQuest.Desktop.UI.Formularios
{
    partial class FormCadastroUsuario
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
            lblTitulo = new Label();
            lblNome = new Label();
            txtNome = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblPapel = new Label();
            cmbPapel = new ComboBox();
            lblSenha = new Label();
            txtSenha = new TextBox();
            btnGerar = new Button();
            chkMostrar = new CheckBox();
            lblAviso = new Label();
            lblErro = new Label();
            btnCancelar = new Button();
            btnCriar = new Button();
            SuspendLayout();
            //
            // lblTitulo
            //
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitulo.Location = new Point(22, 16);
            lblTitulo.Text = "Novo usuário";
            lblTitulo.Name = "lblTitulo";
            lblTitulo.TabIndex = 0;
            //
            // lblNome
            //
            lblNome.AutoSize = true;
            lblNome.ForeColor = Color.FromArgb(71, 85, 105);
            lblNome.Location = new Point(24, 62);
            lblNome.Text = "Nome completo";
            lblNome.Name = "lblNome";
            lblNome.TabIndex = 1;
            //
            // txtNome
            //
            txtNome.Font = new Font("Segoe UI", 10.5F);
            txtNome.Location = new Point(24, 84);
            txtNome.MaxLength = 100;
            txtNome.Size = new Size(412, 26);
            txtNome.Name = "txtNome";
            txtNome.TabIndex = 2;
            //
            // lblEmail
            //
            lblEmail.AutoSize = true;
            lblEmail.ForeColor = Color.FromArgb(71, 85, 105);
            lblEmail.Location = new Point(24, 122);
            lblEmail.Text = "E-mail (será o login)";
            lblEmail.Name = "lblEmail";
            lblEmail.TabIndex = 3;
            //
            // txtEmail
            //
            txtEmail.Font = new Font("Segoe UI", 10.5F);
            txtEmail.Location = new Point(24, 144);
            txtEmail.MaxLength = 100;
            txtEmail.Size = new Size(412, 26);
            txtEmail.Name = "txtEmail";
            txtEmail.TabIndex = 4;
            //
            // lblPapel
            //
            lblPapel.AutoSize = true;
            lblPapel.ForeColor = Color.FromArgb(71, 85, 105);
            lblPapel.Location = new Point(24, 182);
            lblPapel.Text = "Perfil de acesso";
            lblPapel.Name = "lblPapel";
            lblPapel.TabIndex = 5;
            //
            // cmbPapel
            //
            cmbPapel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPapel.FlatStyle = FlatStyle.Flat;
            cmbPapel.Location = new Point(24, 204);
            cmbPapel.Size = new Size(220, 25);
            cmbPapel.Name = "cmbPapel";
            cmbPapel.TabIndex = 6;
            //
            // lblSenha
            //
            lblSenha.AutoSize = true;
            lblSenha.ForeColor = Color.FromArgb(71, 85, 105);
            lblSenha.Location = new Point(24, 242);
            lblSenha.Text = "Senha inicial";
            lblSenha.Name = "lblSenha";
            lblSenha.TabIndex = 7;
            //
            // txtSenha
            //
            txtSenha.Font = new Font("Segoe UI", 10.5F);
            txtSenha.Location = new Point(24, 264);
            txtSenha.MaxLength = 100;
            txtSenha.Size = new Size(300, 26);
            txtSenha.UseSystemPasswordChar = true;
            txtSenha.Name = "txtSenha";
            txtSenha.TabIndex = 8;
            //
            // btnGerar
            //
            btnGerar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnGerar.BackColor = Color.White;
            btnGerar.Cursor = Cursors.Hand;
            btnGerar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnGerar.FlatStyle = FlatStyle.Flat;
            btnGerar.Location = new Point(332, 262);
            btnGerar.Size = new Size(104, 34);
            btnGerar.Text = "Gerar outra";
            btnGerar.UseVisualStyleBackColor = false;
            btnGerar.Name = "btnGerar";
            btnGerar.TabIndex = 9;
            btnGerar.Click += btnGerar_Click;
            //
            // chkMostrar
            //
            chkMostrar.AutoSize = true;
            chkMostrar.ForeColor = Color.FromArgb(71, 85, 105);
            chkMostrar.Location = new Point(24, 298);
            chkMostrar.Text = "Mostrar senha";
            chkMostrar.Name = "chkMostrar";
            chkMostrar.TabIndex = 10;
            chkMostrar.CheckedChanged += chkMostrar_CheckedChanged;
            //
            // lblAviso
            //
            lblAviso.ForeColor = Color.FromArgb(148, 163, 184);
            lblAviso.Location = new Point(24, 328);
            lblAviso.Size = new Size(412, 40);
            lblAviso.Text = "A plataforma não envia e-mail: depois de criar, repasse o e-mail e a senha à pessoa. Ela pode trocar a senha em Minha conta.";
            lblAviso.Name = "lblAviso";
            lblAviso.TabIndex = 11;
            //
            // lblErro
            //
            lblErro.ForeColor = Color.FromArgb(185, 28, 28);
            lblErro.Location = new Point(24, 370);
            lblErro.Size = new Size(412, 36);
            lblErro.Text = "";
            lblErro.Name = "lblErro";
            lblErro.TabIndex = 12;
            //
            // btnCancelar
            //
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnCancelar.BackColor = Color.White;
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Location = new Point(226, 410);
            btnCancelar.Size = new Size(100, 34);
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Name = "btnCancelar";
            btnCancelar.TabIndex = 14;
            //
            // btnCriar
            //
            btnCriar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnCriar.BackColor = Color.FromArgb(37, 99, 235);
            btnCriar.Cursor = Cursors.Hand;
            btnCriar.FlatAppearance.BorderSize = 0;
            btnCriar.FlatStyle = FlatStyle.Flat;
            btnCriar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCriar.ForeColor = Color.White;
            btnCriar.Location = new Point(336, 410);
            btnCriar.Size = new Size(100, 34);
            btnCriar.Text = "Criar conta";
            btnCriar.UseVisualStyleBackColor = false;
            btnCriar.Name = "btnCriar";
            btnCriar.TabIndex = 13;
            btnCriar.Click += btnCriar_Click;
            //
            // FormCadastroUsuario
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblTitulo);
            Controls.Add(lblNome);
            Controls.Add(txtNome);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblPapel);
            Controls.Add(cmbPapel);
            Controls.Add(lblSenha);
            Controls.Add(txtSenha);
            Controls.Add(btnGerar);
            Controls.Add(chkMostrar);
            Controls.Add(lblAviso);
            Controls.Add(lblErro);
            Controls.Add(btnCancelar);
            Controls.Add(btnCriar);
            AcceptButton = btnCriar;
            BackColor = Color.White;
            CancelButton = btnCancelar;
            ClientSize = new Size(460, 462);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Novo usuário";
            Name = "FormCadastroUsuario";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblNome;
        private TextBox txtNome;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblPapel;
        private ComboBox cmbPapel;
        private Label lblSenha;
        private TextBox txtSenha;
        private Button btnGerar;
        private CheckBox chkMostrar;
        private Label lblAviso;
        private Label lblErro;
        private Button btnCancelar;
        private Button btnCriar;
    }
}
