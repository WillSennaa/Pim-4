#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaMedalhas
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
            btnNova = new Button();
            txtBusca = new TextBox();
            lblBusca = new Label();
            cmbRaridade = new ComboBox();
            lblRaridade = new Label();
            faixaErro = new Componentes.FaixaDeErro();
            pnlCorpo = new Panel();
            dgvMedalhas = new Componentes.GradePadrao();
            pnlRodape = new Panel();
            btnEditar = new Button();
            lblResumo = new Label();
            colNome = new DataGridViewTextBoxColumn();
            colRaridade = new DataGridViewTextBoxColumn();
            colDescricao = new DataGridViewTextBoxColumn();
            colConquistas = new DataGridViewTextBoxColumn();
            pnlBarra.SuspendLayout();
            pnlCorpo.SuspendLayout();
            pnlRodape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMedalhas).BeginInit();
            SuspendLayout();
            //
            // pnlBarra
            //
            pnlBarra.Controls.Add(btnAtualizar);
            pnlBarra.Controls.Add(btnNova);
            pnlBarra.Controls.Add(txtBusca);
            pnlBarra.Controls.Add(lblBusca);
            pnlBarra.Controls.Add(cmbRaridade);
            pnlBarra.Controls.Add(lblRaridade);
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
            btnAtualizar.TabIndex = 5;
            btnAtualizar.Click += btnAtualizar_Click;
            //
            // btnNova
            //
            btnNova.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNova.BackColor = Color.FromArgb(37, 99, 235);
            btnNova.Cursor = Cursors.Hand;
            btnNova.FlatAppearance.BorderSize = 0;
            btnNova.FlatStyle = FlatStyle.Flat;
            btnNova.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNova.ForeColor = Color.White;
            btnNova.Location = new Point(644, 6);
            btnNova.Size = new Size(120, 34);
            btnNova.Text = "Nova medalha";
            btnNova.UseVisualStyleBackColor = false;
            btnNova.Name = "btnNova";
            btnNova.TabIndex = 4;
            btnNova.Click += btnNova_Click;
            //
            // txtBusca
            //
            txtBusca.Location = new Point(316, 11);
            txtBusca.MaxLength = 100;
            txtBusca.Size = new Size(260, 25);
            txtBusca.Name = "txtBusca";
            txtBusca.TabIndex = 3;
            txtBusca.TextChanged += txtBusca_TextChanged;
            //
            // lblBusca
            //
            lblBusca.AutoSize = true;
            lblBusca.ForeColor = Color.FromArgb(71, 85, 105);
            lblBusca.Location = new Point(264, 14);
            lblBusca.Text = "Buscar";
            lblBusca.Name = "lblBusca";
            lblBusca.TabIndex = 2;
            //
            // cmbRaridade
            //
            cmbRaridade.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRaridade.FlatStyle = FlatStyle.Flat;
            cmbRaridade.Location = new Point(80, 11);
            cmbRaridade.Size = new Size(170, 25);
            cmbRaridade.Name = "cmbRaridade";
            cmbRaridade.TabIndex = 1;
            cmbRaridade.SelectedIndexChanged += cmbRaridade_SelectedIndexChanged;
            //
            // lblRaridade
            //
            lblRaridade.AutoSize = true;
            lblRaridade.ForeColor = Color.FromArgb(71, 85, 105);
            lblRaridade.Location = new Point(8, 14);
            lblRaridade.Text = "Raridade";
            lblRaridade.Name = "lblRaridade";
            lblRaridade.TabIndex = 0;
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
            pnlCorpo.Controls.Add(dgvMedalhas);
            pnlCorpo.Dock = DockStyle.Fill;
            pnlCorpo.Padding = new Padding(8, 8, 8, 0);
            pnlCorpo.Size = new Size(900, 480);
            pnlCorpo.Name = "pnlCorpo";
            pnlCorpo.TabIndex = 2;
            //
            // dgvMedalhas
            //
            dgvMedalhas.Columns.AddRange(new DataGridViewColumn[] { colNome, colRaridade, colDescricao, colConquistas });
            dgvMedalhas.Dock = DockStyle.Fill;
            dgvMedalhas.Name = "dgvMedalhas";
            dgvMedalhas.TabIndex = 9;
            dgvMedalhas.SelectionChanged += dgvMedalhas_SelectionChanged;
            dgvMedalhas.CellDoubleClick += dgvMedalhas_CellDoubleClick;
            dgvMedalhas.DataBindingComplete += dgvMedalhas_DataBindingComplete;
            //
            // colNome
            //
            colNome.DataPropertyName = "Nome";
            colNome.FillWeight = 26F;
            colNome.HeaderText = "Medalha";
            colNome.Name = "colNome";
            colNome.ReadOnly = true;
            //
            // colRaridade
            //
            colRaridade.DataPropertyName = "Raridade";
            colRaridade.FillWeight = 12F;
            colRaridade.HeaderText = "Raridade";
            colRaridade.Name = "colRaridade";
            colRaridade.ReadOnly = true;
            //
            // colDescricao
            //
            colDescricao.DataPropertyName = "Descricao";
            colDescricao.FillWeight = 44F;
            colDescricao.HeaderText = "Como se conquista";
            colDescricao.Name = "colDescricao";
            colDescricao.ReadOnly = true;
            //
            // colConquistas
            //
            colConquistas.DataPropertyName = "Conquistas";
            colConquistas.FillWeight = 18F;
            colConquistas.HeaderText = "Conquistada por";
            colConquistas.Name = "colConquistas";
            colConquistas.ReadOnly = true;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(btnEditar);
            pnlRodape.Controls.Add(lblResumo);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Size = new Size(900, 56);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.TabIndex = 3;
            //
            // btnEditar
            //
            btnEditar.Enabled = false;
            btnEditar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEditar.BackColor = Color.White;
            btnEditar.Cursor = Cursors.Hand;
            btnEditar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnEditar.FlatStyle = FlatStyle.Flat;
            btnEditar.Location = new Point(732, 12);
            btnEditar.Size = new Size(160, 34);
            btnEditar.Text = "Editar medalha";
            btnEditar.UseVisualStyleBackColor = false;
            btnEditar.Name = "btnEditar";
            btnEditar.TabIndex = 1;
            btnEditar.Click += btnEditar_Click;
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
            // TelaMedalhas
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
            Name = "TelaMedalhas";
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlCorpo.ResumeLayout(false);
            pnlCorpo.PerformLayout();
            pnlRodape.ResumeLayout(false);
            pnlRodape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMedalhas).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlBarra;
        private Button btnAtualizar;
        private Button btnNova;
        private TextBox txtBusca;
        private Label lblBusca;
        private ComboBox cmbRaridade;
        private Label lblRaridade;
        private Componentes.FaixaDeErro faixaErro;
        private Panel pnlCorpo;
        private Componentes.GradePadrao dgvMedalhas;
        private Panel pnlRodape;
        private Button btnEditar;
        private Label lblResumo;
        private DataGridViewTextBoxColumn colNome;
        private DataGridViewTextBoxColumn colRaridade;
        private DataGridViewTextBoxColumn colDescricao;
        private DataGridViewTextBoxColumn colConquistas;
    }
}
