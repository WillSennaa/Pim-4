#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaLogs
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
            txtBusca = new TextBox();
            lblBusca = new Label();
            cmbLimite = new ComboBox();
            lblMostrar = new Label();
            faixaErro = new Componentes.FaixaDeErro();
            pnlCorpo = new Panel();
            dgvLogs = new Componentes.GradePadrao();
            pnlRodape = new Panel();
            lblLimitacao = new Label();
            lblResumo = new Label();
            colData = new DataGridViewTextBoxColumn();
            colAdministrador = new DataGridViewTextBoxColumn();
            colAcao = new DataGridViewTextBoxColumn();
            pnlBarra.SuspendLayout();
            pnlCorpo.SuspendLayout();
            pnlRodape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLogs).BeginInit();
            SuspendLayout();
            //
            // pnlBarra
            //
            pnlBarra.Controls.Add(btnAtualizar);
            pnlBarra.Controls.Add(txtBusca);
            pnlBarra.Controls.Add(lblBusca);
            pnlBarra.Controls.Add(cmbLimite);
            pnlBarra.Controls.Add(lblMostrar);
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
            btnAtualizar.TabIndex = 4;
            btnAtualizar.Click += btnAtualizar_Click;
            //
            // txtBusca
            //
            txtBusca.Location = new Point(262, 11);
            txtBusca.MaxLength = 100;
            txtBusca.Size = new Size(320, 25);
            txtBusca.Name = "txtBusca";
            txtBusca.TabIndex = 3;
            txtBusca.TextChanged += txtBusca_TextChanged;
            //
            // lblBusca
            //
            lblBusca.AutoSize = true;
            lblBusca.ForeColor = Color.FromArgb(71, 85, 105);
            lblBusca.Location = new Point(210, 14);
            lblBusca.Text = "Buscar";
            lblBusca.Name = "lblBusca";
            lblBusca.TabIndex = 2;
            //
            // cmbLimite
            //
            cmbLimite.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLimite.FlatStyle = FlatStyle.Flat;
            cmbLimite.Location = new Point(72, 11);
            cmbLimite.Size = new Size(120, 25);
            cmbLimite.Name = "cmbLimite";
            cmbLimite.TabIndex = 1;
            cmbLimite.SelectedIndexChanged += cmbLimite_SelectedIndexChanged;
            //
            // lblMostrar
            //
            lblMostrar.AutoSize = true;
            lblMostrar.ForeColor = Color.FromArgb(71, 85, 105);
            lblMostrar.Location = new Point(8, 14);
            lblMostrar.Text = "Mostrar";
            lblMostrar.Name = "lblMostrar";
            lblMostrar.TabIndex = 0;
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
            pnlCorpo.Controls.Add(dgvLogs);
            pnlCorpo.Dock = DockStyle.Fill;
            pnlCorpo.Padding = new Padding(8, 8, 8, 0);
            pnlCorpo.Size = new Size(900, 470);
            pnlCorpo.Name = "pnlCorpo";
            pnlCorpo.TabIndex = 2;
            //
            // dgvLogs
            //
            dgvLogs.Columns.AddRange(new DataGridViewColumn[] { colData, colAdministrador, colAcao });
            dgvLogs.Dock = DockStyle.Fill;
            dgvLogs.Name = "dgvLogs";
            dgvLogs.TabIndex = 8;
            //
            // colData
            //
            colData.DataPropertyName = "Data";
            colData.FillWeight = 16F;
            colData.HeaderText = "Data e hora";
            colData.Name = "colData";
            colData.ReadOnly = true;
            //
            // colAdministrador
            //
            colAdministrador.DataPropertyName = "Administrador";
            colAdministrador.FillWeight = 20F;
            colAdministrador.HeaderText = "Administrador";
            colAdministrador.Name = "colAdministrador";
            colAdministrador.ReadOnly = true;
            //
            // colAcao
            //
            colAcao.DataPropertyName = "Acao";
            colAcao.FillWeight = 64F;
            colAcao.HeaderText = "Ação";
            colAcao.Name = "colAcao";
            colAcao.ReadOnly = true;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(lblLimitacao);
            pnlRodape.Controls.Add(lblResumo);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Size = new Size(900, 66);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.TabIndex = 3;
            //
            // lblLimitacao
            //
            lblLimitacao.Font = new Font("Segoe UI", 9F);
            lblLimitacao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblLimitacao.ForeColor = Color.FromArgb(148, 163, 184);
            lblLimitacao.Location = new Point(8, 32);
            lblLimitacao.Size = new Size(884, 30);
            lblLimitacao.Text = "Mudanças de status de usuário são gravadas pela trigger do banco, que não sabe qual administrador agiu: elas aparecem sempre como o administrador nº 1 (limitação conhecida do modelo).";
            lblLimitacao.Name = "lblLimitacao";
            lblLimitacao.TabIndex = 1;
            //
            // lblResumo
            //
            lblResumo.AutoSize = true;
            lblResumo.ForeColor = Color.FromArgb(71, 85, 105);
            lblResumo.Location = new Point(8, 10);
            lblResumo.Text = "";
            lblResumo.Name = "lblResumo";
            lblResumo.TabIndex = 0;
            //
            // TelaLogs
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
            Name = "TelaLogs";
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlCorpo.ResumeLayout(false);
            pnlCorpo.PerformLayout();
            pnlRodape.ResumeLayout(false);
            pnlRodape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLogs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlBarra;
        private Button btnAtualizar;
        private TextBox txtBusca;
        private Label lblBusca;
        private ComboBox cmbLimite;
        private Label lblMostrar;
        private Componentes.FaixaDeErro faixaErro;
        private Panel pnlCorpo;
        private Componentes.GradePadrao dgvLogs;
        private Panel pnlRodape;
        private Label lblLimitacao;
        private Label lblResumo;
        private DataGridViewTextBoxColumn colData;
        private DataGridViewTextBoxColumn colAdministrador;
        private DataGridViewTextBoxColumn colAcao;
    }
}
