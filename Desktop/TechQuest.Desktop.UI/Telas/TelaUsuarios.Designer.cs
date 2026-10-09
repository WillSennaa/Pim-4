#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaUsuarios
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
            pnlBarra = new Panel();
            btnAtualizar = new Button();
            btnNovo = new Button();
            txtBusca = new TextBox();
            lblBusca = new Label();
            cmbSituacao = new ComboBox();
            lblSituacao = new Label();
            cmbPapel = new ComboBox();
            lblPapel = new Label();
            faixaErro = new Componentes.FaixaDeErro();
            pnlCorpo = new Panel();
            dgvUsuarios = new Componentes.GradePadrao();
            pnlRodape = new Panel();
            btnStatus = new Button();
            lblBloqueio = new Label();
            lblResumo = new Label();
            colNome = new DataGridViewTextBoxColumn();
            colEmail = new DataGridViewTextBoxColumn();
            colPapel = new DataGridViewTextBoxColumn();
            colSituacao = new DataGridViewTextBoxColumn();
            colCadastro = new DataGridViewTextBoxColumn();
            pnlBarra.SuspendLayout();
            pnlCorpo.SuspendLayout();
            pnlRodape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).BeginInit();
            SuspendLayout();
            //
            // pnlBarra
            //
            pnlBarra.Controls.Add(btnAtualizar);
            pnlBarra.Controls.Add(btnNovo);
            pnlBarra.Controls.Add(txtBusca);
            pnlBarra.Controls.Add(lblBusca);
            pnlBarra.Controls.Add(cmbSituacao);
            pnlBarra.Controls.Add(lblSituacao);
            pnlBarra.Controls.Add(cmbPapel);
            pnlBarra.Controls.Add(lblPapel);
            pnlBarra.Dock = DockStyle.Top;
            pnlBarra.Size = new Size(900, 48);
            pnlBarra.Name = "pnlBarra";
            pnlBarra.TabIndex = 0;
            //
            // btnAtualizar
            //
            btnAtualizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAtualizar.BackColor = Color.White;
            btnAtualizar.Cursor = Cursors.Hand;
            btnAtualizar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnAtualizar.FlatStyle = FlatStyle.Flat;
            btnAtualizar.Location = new Point(772, 6);
            btnAtualizar.Size = new Size(120, 34);
            btnAtualizar.Text = "Atualizar";
            btnAtualizar.UseVisualStyleBackColor = false;
            btnAtualizar.Name = "btnAtualizar";
            btnAtualizar.TabIndex = 7;
            btnAtualizar.Click += btnAtualizar_Click;
            //
            // btnNovo
            //
            btnNovo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNovo.BackColor = Color.FromArgb(37, 99, 235);
            btnNovo.Cursor = Cursors.Hand;
            btnNovo.FlatAppearance.BorderSize = 0;
            btnNovo.FlatStyle = FlatStyle.Flat;
            btnNovo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNovo.ForeColor = Color.White;
            btnNovo.Location = new Point(644, 6);
            btnNovo.Size = new Size(120, 34);
            btnNovo.Text = "Novo usuário";
            btnNovo.UseVisualStyleBackColor = false;
            btnNovo.Name = "btnNovo";
            btnNovo.TabIndex = 6;
            btnNovo.Click += btnNovo_Click;
            //
            // txtBusca
            //
            txtBusca.Location = new Point(470, 11);
            txtBusca.MaxLength = 100;
            txtBusca.Size = new Size(160, 25);
            txtBusca.Name = "txtBusca";
            txtBusca.TabIndex = 5;
            txtBusca.TextChanged += txtBusca_TextChanged;
            //
            // lblBusca
            //
            lblBusca.AutoSize = true;
            lblBusca.ForeColor = Color.FromArgb(71, 85, 105);
            lblBusca.Location = new Point(418, 14);
            lblBusca.Text = "Buscar";
            lblBusca.Name = "lblBusca";
            lblBusca.TabIndex = 4;
            //
            // cmbSituacao
            //
            cmbSituacao.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSituacao.FlatStyle = FlatStyle.Flat;
            cmbSituacao.Location = new Point(284, 11);
            cmbSituacao.Size = new Size(120, 25);
            cmbSituacao.Name = "cmbSituacao";
            cmbSituacao.TabIndex = 3;
            cmbSituacao.SelectedIndexChanged += filtro_Changed;
            //
            // lblSituacao
            //
            lblSituacao.AutoSize = true;
            lblSituacao.ForeColor = Color.FromArgb(71, 85, 105);
            lblSituacao.Location = new Point(218, 14);
            lblSituacao.Text = "Situação";
            lblSituacao.Name = "lblSituacao";
            lblSituacao.TabIndex = 2;
            //
            // cmbPapel
            //
            cmbPapel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPapel.FlatStyle = FlatStyle.Flat;
            cmbPapel.Location = new Point(56, 11);
            cmbPapel.Size = new Size(150, 25);
            cmbPapel.Name = "cmbPapel";
            cmbPapel.TabIndex = 1;
            cmbPapel.SelectedIndexChanged += filtro_Changed;
            //
            // lblPapel
            //
            lblPapel.AutoSize = true;
            lblPapel.ForeColor = Color.FromArgb(71, 85, 105);
            lblPapel.Location = new Point(8, 14);
            lblPapel.Text = "Perfil";
            lblPapel.Name = "lblPapel";
            lblPapel.TabIndex = 0;
            //
            // faixaErro
            //
            faixaErro.Dock = DockStyle.Top;
            faixaErro.Name = "faixaErro";
            faixaErro.TabIndex = 1;
            faixaErro.TentarNovamente += faixaErro_TentarNovamente;
            //
            // pnlCorpo
            //
            pnlCorpo.Controls.Add(dgvUsuarios);
            pnlCorpo.Dock = DockStyle.Fill;
            pnlCorpo.Padding = new Padding(8, 8, 8, 0);
            pnlCorpo.Size = new Size(900, 480);
            pnlCorpo.Name = "pnlCorpo";
            pnlCorpo.TabIndex = 2;
            //
            // dgvUsuarios
            //
            dgvUsuarios.Columns.AddRange(new DataGridViewColumn[] { colNome, colEmail, colPapel, colSituacao, colCadastro });
            dgvUsuarios.Dock = DockStyle.Fill;
            dgvUsuarios.Name = "dgvUsuarios";
            dgvUsuarios.TabIndex = 11;
            dgvUsuarios.SelectionChanged += dgvUsuarios_SelectionChanged;
            dgvUsuarios.DataBindingComplete += dgvUsuarios_DataBindingComplete;
            //
            // colNome
            //
            colNome.DataPropertyName = "Nome";
            colNome.FillWeight = 28F;
            colNome.HeaderText = "Nome";
            colNome.Name = "colNome";
            colNome.ReadOnly = true;
            //
            // colEmail
            //
            colEmail.DataPropertyName = "Email";
            colEmail.FillWeight = 28F;
            colEmail.HeaderText = "E-mail";
            colEmail.Name = "colEmail";
            colEmail.ReadOnly = true;
            //
            // colPapel
            //
            colPapel.DataPropertyName = "Papel";
            colPapel.FillWeight = 14F;
            colPapel.HeaderText = "Perfil";
            colPapel.Name = "colPapel";
            colPapel.ReadOnly = true;
            //
            // colSituacao
            //
            colSituacao.DataPropertyName = "Situacao";
            colSituacao.FillWeight = 10F;
            colSituacao.HeaderText = "Situação";
            colSituacao.Name = "colSituacao";
            colSituacao.ReadOnly = true;
            //
            // colCadastro
            //
            colCadastro.DataPropertyName = "Cadastro";
            colCadastro.FillWeight = 12F;
            colCadastro.HeaderText = "Cadastro";
            colCadastro.Name = "colCadastro";
            colCadastro.ReadOnly = true;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(btnStatus);
            pnlRodape.Controls.Add(lblBloqueio);
            pnlRodape.Controls.Add(lblResumo);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Size = new Size(900, 56);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.TabIndex = 3;
            //
            // btnStatus
            //
            btnStatus.Enabled = false;
            btnStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStatus.BackColor = Color.White;
            btnStatus.Cursor = Cursors.Hand;
            btnStatus.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnStatus.FlatStyle = FlatStyle.Flat;
            btnStatus.Location = new Point(732, 12);
            btnStatus.Size = new Size(160, 34);
            btnStatus.Text = "Desativar conta";
            btnStatus.UseVisualStyleBackColor = false;
            btnStatus.Name = "btnStatus";
            btnStatus.TabIndex = 2;
            btnStatus.Click += btnStatus_Click;
            //
            // lblBloqueio
            //
            lblBloqueio.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBloqueio.ForeColor = Color.FromArgb(148, 163, 184);
            lblBloqueio.Location = new Point(360, 20);
            lblBloqueio.Size = new Size(364, 19);
            lblBloqueio.Text = "";
            lblBloqueio.TextAlign = ContentAlignment.MiddleRight;
            lblBloqueio.Name = "lblBloqueio";
            lblBloqueio.TabIndex = 1;
            //
            // lblResumo
            //
            lblResumo.AutoSize = true;
            lblResumo.ForeColor = Color.FromArgb(71, 85, 105);
            lblResumo.Location = new Point(8, 20);
            lblResumo.Text = "";
            lblResumo.Name = "lblResumo";
            lblResumo.TabIndex = 0;
            //
            // TelaUsuarios
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlCorpo);
            Controls.Add(pnlRodape);
            Controls.Add(faixaErro);
            Controls.Add(pnlBarra);
            BackColor = Color.FromArgb(241, 245, 249);
            Font = new Font("Segoe UI", 10F);
            Size = new Size(900, 640);
            Name = "TelaUsuarios";
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlCorpo.ResumeLayout(false);
            pnlCorpo.PerformLayout();
            pnlRodape.ResumeLayout(false);
            pnlRodape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlBarra;
        private Button btnAtualizar;
        private Button btnNovo;
        private TextBox txtBusca;
        private Label lblBusca;
        private ComboBox cmbSituacao;
        private Label lblSituacao;
        private ComboBox cmbPapel;
        private Label lblPapel;
        private Componentes.FaixaDeErro faixaErro;
        private Panel pnlCorpo;
        private Componentes.GradePadrao dgvUsuarios;
        private Panel pnlRodape;
        private Button btnStatus;
        private Label lblBloqueio;
        private Label lblResumo;
        private DataGridViewTextBoxColumn colNome;
        private DataGridViewTextBoxColumn colEmail;
        private DataGridViewTextBoxColumn colPapel;
        private DataGridViewTextBoxColumn colSituacao;
        private DataGridViewTextBoxColumn colCadastro;
    }
}
