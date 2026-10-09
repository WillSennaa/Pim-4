#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaAprovacoes
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
            DataGridViewCellStyle estiloCabecalho = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloCelula = new DataGridViewCellStyle();
            pnlBarra = new Panel();
            btnAtualizar = new Button();
            lblResumo = new Label();
            pnlFalha = new Panel();
            lblFalha = new Label();
            btnTentarNovamente = new Button();
            pnlCorpo = new Panel();
            dgvFila = new DataGridView();
            colCurso = new DataGridViewTextBoxColumn();
            colTutor = new DataGridViewTextBoxColumn();
            colCategoria = new DataGridViewTextBoxColumn();
            colNivel = new DataGridViewTextBoxColumn();
            colAulas = new DataGridViewTextBoxColumn();
            colProva = new DataGridViewTextBoxColumn();
            lblVazio = new Label();
            pnlRodape = new Panel();
            btnRevisar = new Button();
            lblDica = new Label();
            pnlBarra.SuspendLayout();
            pnlFalha.SuspendLayout();
            pnlCorpo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFila).BeginInit();
            pnlRodape.SuspendLayout();
            SuspendLayout();
            //
            // pnlBarra
            //
            pnlBarra.Controls.Add(btnAtualizar);
            pnlBarra.Controls.Add(lblResumo);
            pnlBarra.Dock = DockStyle.Top;
            pnlBarra.Location = new Point(0, 0);
            pnlBarra.Name = "pnlBarra";
            pnlBarra.Size = new Size(900, 48);
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
            btnAtualizar.Name = "btnAtualizar";
            btnAtualizar.Size = new Size(120, 34);
            btnAtualizar.TabIndex = 1;
            btnAtualizar.Text = "Atualizar";
            btnAtualizar.UseVisualStyleBackColor = false;
            btnAtualizar.Click += btnAtualizar_Click;
            //
            // lblResumo
            //
            lblResumo.AutoSize = true;
            lblResumo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumo.ForeColor = Color.FromArgb(71, 85, 105);
            lblResumo.Location = new Point(8, 14);
            lblResumo.Name = "lblResumo";
            lblResumo.Size = new Size(0, 19);
            lblResumo.TabIndex = 0;
            //
            // pnlFalha
            //
            pnlFalha.BackColor = Color.FromArgb(254, 226, 226);
            pnlFalha.Controls.Add(lblFalha);
            pnlFalha.Controls.Add(btnTentarNovamente);
            pnlFalha.Dock = DockStyle.Top;
            pnlFalha.Location = new Point(0, 48);
            pnlFalha.Name = "pnlFalha";
            pnlFalha.Padding = new Padding(8);
            pnlFalha.Size = new Size(900, 56);
            pnlFalha.TabIndex = 1;
            pnlFalha.Visible = false;
            //
            // lblFalha
            //
            lblFalha.Dock = DockStyle.Fill;
            lblFalha.ForeColor = Color.FromArgb(185, 28, 28);
            lblFalha.Location = new Point(8, 8);
            lblFalha.Name = "lblFalha";
            lblFalha.Padding = new Padding(4, 0, 0, 0);
            lblFalha.Size = new Size(734, 40);
            lblFalha.TabIndex = 0;
            lblFalha.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnTentarNovamente
            //
            btnTentarNovamente.BackColor = Color.White;
            btnTentarNovamente.Cursor = Cursors.Hand;
            btnTentarNovamente.Dock = DockStyle.Right;
            btnTentarNovamente.FlatAppearance.BorderColor = Color.FromArgb(185, 28, 28);
            btnTentarNovamente.FlatStyle = FlatStyle.Flat;
            btnTentarNovamente.ForeColor = Color.FromArgb(185, 28, 28);
            btnTentarNovamente.Location = new Point(742, 8);
            btnTentarNovamente.Name = "btnTentarNovamente";
            btnTentarNovamente.Size = new Size(150, 40);
            btnTentarNovamente.TabIndex = 1;
            btnTentarNovamente.Text = "Tentar novamente";
            btnTentarNovamente.UseVisualStyleBackColor = false;
            btnTentarNovamente.Click += btnAtualizar_Click;
            //
            // pnlCorpo
            //
            pnlCorpo.Controls.Add(dgvFila);
            pnlCorpo.Controls.Add(lblVazio);
            pnlCorpo.Dock = DockStyle.Fill;
            pnlCorpo.Location = new Point(0, 104);
            pnlCorpo.Name = "pnlCorpo";
            pnlCorpo.Padding = new Padding(8, 8, 8, 0);
            pnlCorpo.Size = new Size(900, 480);
            pnlCorpo.TabIndex = 2;
            //
            // dgvFila
            //
            dgvFila.AllowUserToAddRows = false;
            dgvFila.AllowUserToDeleteRows = false;
            dgvFila.AllowUserToResizeRows = false;
            dgvFila.AutoGenerateColumns = false;
            dgvFila.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFila.BackgroundColor = Color.White;
            dgvFila.BorderStyle = BorderStyle.None;
            dgvFila.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvFila.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            estiloCabecalho.Alignment = DataGridViewContentAlignment.MiddleLeft;
            estiloCabecalho.BackColor = Color.FromArgb(248, 250, 252);
            estiloCabecalho.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            estiloCabecalho.ForeColor = Color.FromArgb(71, 85, 105);
            estiloCabecalho.Padding = new Padding(8, 0, 0, 0);
            estiloCabecalho.SelectionBackColor = Color.FromArgb(248, 250, 252);
            estiloCabecalho.SelectionForeColor = Color.FromArgb(71, 85, 105);
            estiloCabecalho.WrapMode = DataGridViewTriState.False;
            dgvFila.ColumnHeadersDefaultCellStyle = estiloCabecalho;
            dgvFila.ColumnHeadersHeight = 40;
            dgvFila.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvFila.Columns.AddRange(new DataGridViewColumn[] { colCurso, colTutor, colCategoria, colNivel, colAulas, colProva });
            estiloCelula.Alignment = DataGridViewContentAlignment.MiddleLeft;
            estiloCelula.BackColor = Color.White;
            estiloCelula.Font = new Font("Segoe UI", 10F);
            estiloCelula.ForeColor = Color.FromArgb(15, 23, 42);
            estiloCelula.Padding = new Padding(8, 0, 0, 0);
            estiloCelula.SelectionBackColor = Color.FromArgb(219, 234, 254);
            estiloCelula.SelectionForeColor = Color.FromArgb(15, 23, 42);
            estiloCelula.WrapMode = DataGridViewTriState.False;
            dgvFila.DefaultCellStyle = estiloCelula;
            dgvFila.Dock = DockStyle.Fill;
            dgvFila.EnableHeadersVisualStyles = false;
            dgvFila.GridColor = Color.FromArgb(226, 232, 240);
            dgvFila.Location = new Point(8, 8);
            dgvFila.MultiSelect = false;
            dgvFila.Name = "dgvFila";
            dgvFila.ReadOnly = true;
            dgvFila.RowHeadersVisible = false;
            dgvFila.RowTemplate.Height = 38;
            dgvFila.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFila.Size = new Size(884, 472);
            dgvFila.TabIndex = 0;
            dgvFila.CellDoubleClick += dgvFila_CellDoubleClick;
            dgvFila.DataBindingComplete += dgvFila_DataBindingComplete;
            dgvFila.KeyDown += dgvFila_KeyDown;
            //
            // colCurso
            //
            colCurso.DataPropertyName = "Curso";
            colCurso.FillWeight = 34F;
            colCurso.HeaderText = "Curso";
            colCurso.Name = "colCurso";
            colCurso.ReadOnly = true;
            //
            // colTutor
            //
            colTutor.DataPropertyName = "Tutor";
            colTutor.FillWeight = 20F;
            colTutor.HeaderText = "Tutor";
            colTutor.Name = "colTutor";
            colTutor.ReadOnly = true;
            //
            // colCategoria
            //
            colCategoria.DataPropertyName = "Categoria";
            colCategoria.FillWeight = 14F;
            colCategoria.HeaderText = "Categoria";
            colCategoria.Name = "colCategoria";
            colCategoria.ReadOnly = true;
            //
            // colNivel
            //
            colNivel.DataPropertyName = "Nivel";
            colNivel.FillWeight = 12F;
            colNivel.HeaderText = "Nível";
            colNivel.Name = "colNivel";
            colNivel.ReadOnly = true;
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
            colProva.FillWeight = 11F;
            colProva.HeaderText = "Prova";
            colProva.Name = "colProva";
            colProva.ReadOnly = true;
            //
            // lblVazio
            //
            lblVazio.BackColor = Color.White;
            lblVazio.Dock = DockStyle.Fill;
            lblVazio.Font = new Font("Segoe UI", 11F);
            lblVazio.ForeColor = Color.FromArgb(148, 163, 184);
            lblVazio.Location = new Point(8, 8);
            lblVazio.Name = "lblVazio";
            lblVazio.Size = new Size(884, 472);
            lblVazio.TabIndex = 1;
            lblVazio.Text = "Nenhum curso aguardando avaliação.\r\nQuando um tutor submeter um curso, ele aparece aqui.";
            lblVazio.TextAlign = ContentAlignment.MiddleCenter;
            lblVazio.Visible = false;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(btnRevisar);
            pnlRodape.Controls.Add(lblDica);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Location = new Point(0, 584);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.Size = new Size(900, 56);
            pnlRodape.TabIndex = 3;
            //
            // btnRevisar
            //
            btnRevisar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRevisar.BackColor = Color.FromArgb(37, 99, 235);
            btnRevisar.Cursor = Cursors.Hand;
            btnRevisar.Enabled = false;
            btnRevisar.FlatAppearance.BorderSize = 0;
            btnRevisar.FlatStyle = FlatStyle.Flat;
            btnRevisar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRevisar.ForeColor = Color.White;
            btnRevisar.Location = new Point(732, 12);
            btnRevisar.Name = "btnRevisar";
            btnRevisar.Size = new Size(160, 36);
            btnRevisar.TabIndex = 1;
            btnRevisar.Text = "Revisar curso";
            btnRevisar.UseVisualStyleBackColor = false;
            btnRevisar.Click += btnRevisar_Click;
            //
            // lblDica
            //
            lblDica.AutoSize = true;
            lblDica.ForeColor = Color.FromArgb(148, 163, 184);
            lblDica.Location = new Point(8, 20);
            lblDica.Name = "lblDica";
            lblDica.Size = new Size(395, 19);
            lblDica.TabIndex = 0;
            lblDica.Text = "Dê dois cliques no curso (ou Enter) para abrir a revisão completa.";
            //
            // TelaAprovacoes
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(pnlCorpo);
            Controls.Add(pnlRodape);
            Controls.Add(pnlFalha);
            Controls.Add(pnlBarra);
            Font = new Font("Segoe UI", 10F);
            Name = "TelaAprovacoes";
            Size = new Size(900, 640);
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlFalha.ResumeLayout(false);
            pnlCorpo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvFila).EndInit();
            pnlRodape.ResumeLayout(false);
            pnlRodape.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBarra;
        private Button btnAtualizar;
        private Label lblResumo;
        private Panel pnlFalha;
        private Label lblFalha;
        private Button btnTentarNovamente;
        private Panel pnlCorpo;
        private DataGridView dgvFila;
        private DataGridViewTextBoxColumn colCurso;
        private DataGridViewTextBoxColumn colTutor;
        private DataGridViewTextBoxColumn colCategoria;
        private DataGridViewTextBoxColumn colNivel;
        private DataGridViewTextBoxColumn colAulas;
        private DataGridViewTextBoxColumn colProva;
        private Label lblVazio;
        private Panel pnlRodape;
        private Button btnRevisar;
        private Label lblDica;
    }
}
