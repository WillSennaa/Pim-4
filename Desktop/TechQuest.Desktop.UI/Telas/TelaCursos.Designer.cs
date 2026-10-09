#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaCursos
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
            cmbStatus = new ComboBox();
            lblFiltro = new Label();
            faixaErro = new Componentes.FaixaDeErro();
            pnlCorpo = new Panel();
            dgvCursos = new Componentes.GradePadrao();
            pnlRodape = new Panel();
            btnAbrir = new Button();
            lblResumo = new Label();
            colCurso = new DataGridViewTextBoxColumn();
            colTutor = new DataGridViewTextBoxColumn();
            colCategoria = new DataGridViewTextBoxColumn();
            colNivel = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colAulas = new DataGridViewTextBoxColumn();
            colProva = new DataGridViewTextBoxColumn();
            pnlBarra.SuspendLayout();
            pnlCorpo.SuspendLayout();
            pnlRodape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCursos).BeginInit();
            SuspendLayout();
            //
            // pnlBarra
            //
            pnlBarra.Controls.Add(btnAtualizar);
            pnlBarra.Controls.Add(txtBusca);
            pnlBarra.Controls.Add(lblBusca);
            pnlBarra.Controls.Add(cmbStatus);
            pnlBarra.Controls.Add(lblFiltro);
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
            txtBusca.Location = new Point(336, 11);
            txtBusca.MaxLength = 100;
            txtBusca.Size = new Size(300, 25);
            txtBusca.Name = "txtBusca";
            txtBusca.TabIndex = 3;
            txtBusca.TextChanged += txtBusca_TextChanged;
            //
            // lblBusca
            //
            lblBusca.AutoSize = true;
            lblBusca.ForeColor = Color.FromArgb(71, 85, 105);
            lblBusca.Location = new Point(284, 14);
            lblBusca.Text = "Buscar";
            lblBusca.Name = "lblBusca";
            lblBusca.TabIndex = 2;
            //
            // cmbStatus
            //
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.FlatStyle = FlatStyle.Flat;
            cmbStatus.Location = new Point(64, 11);
            cmbStatus.Size = new Size(200, 25);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.TabIndex = 1;
            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;
            //
            // lblFiltro
            //
            lblFiltro.AutoSize = true;
            lblFiltro.ForeColor = Color.FromArgb(71, 85, 105);
            lblFiltro.Location = new Point(8, 14);
            lblFiltro.Text = "Estado";
            lblFiltro.Name = "lblFiltro";
            lblFiltro.TabIndex = 0;
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
            pnlCorpo.Controls.Add(dgvCursos);
            pnlCorpo.Dock = DockStyle.Fill;
            pnlCorpo.Padding = new Padding(8, 8, 8, 0);
            pnlCorpo.Size = new Size(900, 480);
            pnlCorpo.Name = "pnlCorpo";
            pnlCorpo.TabIndex = 2;
            //
            // dgvCursos
            //
            dgvCursos.Columns.AddRange(new DataGridViewColumn[] { colCurso, colTutor, colCategoria, colNivel, colStatus, colAulas, colProva });
            dgvCursos.Dock = DockStyle.Fill;
            dgvCursos.Name = "dgvCursos";
            dgvCursos.TabIndex = 8;
            dgvCursos.CellDoubleClick += dgvCursos_CellDoubleClick;
            dgvCursos.KeyDown += dgvCursos_KeyDown;
            dgvCursos.DataBindingComplete += dgvCursos_DataBindingComplete;
            dgvCursos.SelectionChanged += dgvCursos_SelectionChanged;
            //
            // colCurso
            //
            colCurso.DataPropertyName = "Curso";
            colCurso.FillWeight = 30F;
            colCurso.HeaderText = "Curso";
            colCurso.Name = "colCurso";
            colCurso.ReadOnly = true;
            //
            // colTutor
            //
            colTutor.DataPropertyName = "Tutor";
            colTutor.FillWeight = 17F;
            colTutor.HeaderText = "Tutor";
            colTutor.Name = "colTutor";
            colTutor.ReadOnly = true;
            //
            // colCategoria
            //
            colCategoria.DataPropertyName = "Categoria";
            colCategoria.FillWeight = 13F;
            colCategoria.HeaderText = "Categoria";
            colCategoria.Name = "colCategoria";
            colCategoria.ReadOnly = true;
            //
            // colNivel
            //
            colNivel.DataPropertyName = "Nivel";
            colNivel.FillWeight = 10F;
            colNivel.HeaderText = "Nível";
            colNivel.Name = "colNivel";
            colNivel.ReadOnly = true;
            //
            // colStatus
            //
            colStatus.DataPropertyName = "Status";
            colStatus.FillWeight = 11F;
            colStatus.HeaderText = "Estado";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            //
            // colAulas
            //
            colAulas.DataPropertyName = "Aulas";
            colAulas.FillWeight = 9F;
            colAulas.HeaderText = "Aulas";
            colAulas.Name = "colAulas";
            colAulas.ReadOnly = true;
            //
            // colProva
            //
            colProva.DataPropertyName = "Prova";
            colProva.FillWeight = 10F;
            colProva.HeaderText = "Prova";
            colProva.Name = "colProva";
            colProva.ReadOnly = true;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(btnAbrir);
            pnlRodape.Controls.Add(lblResumo);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Size = new Size(900, 56);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.TabIndex = 3;
            //
            // btnAbrir
            //
            btnAbrir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAbrir.BackColor = Color.FromArgb(37, 99, 235);
            btnAbrir.Cursor = Cursors.Hand;
            btnAbrir.Enabled = false;
            btnAbrir.FlatAppearance.BorderSize = 0;
            btnAbrir.FlatStyle = FlatStyle.Flat;
            btnAbrir.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAbrir.ForeColor = Color.White;
            btnAbrir.Location = new Point(732, 12);
            btnAbrir.Size = new Size(160, 36);
            btnAbrir.Text = "Abrir curso";
            btnAbrir.UseVisualStyleBackColor = false;
            btnAbrir.Name = "btnAbrir";
            btnAbrir.TabIndex = 1;
            btnAbrir.Click += btnAbrir_Click;
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
            // TelaCursos
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
            Name = "TelaCursos";
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlCorpo.ResumeLayout(false);
            pnlCorpo.PerformLayout();
            pnlRodape.ResumeLayout(false);
            pnlRodape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCursos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlBarra;
        private Button btnAtualizar;
        private TextBox txtBusca;
        private Label lblBusca;
        private ComboBox cmbStatus;
        private Label lblFiltro;
        private Componentes.FaixaDeErro faixaErro;
        private Panel pnlCorpo;
        private Componentes.GradePadrao dgvCursos;
        private Panel pnlRodape;
        private Button btnAbrir;
        private Label lblResumo;
        private DataGridViewTextBoxColumn colCurso;
        private DataGridViewTextBoxColumn colTutor;
        private DataGridViewTextBoxColumn colCategoria;
        private DataGridViewTextBoxColumn colNivel;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colAulas;
        private DataGridViewTextBoxColumn colProva;
    }
}
