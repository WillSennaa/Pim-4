#nullable disable

namespace TechQuest.Desktop.UI.Formularios
{
    partial class FormRevisaoCurso
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
            lblStatus = new Label();
            lblInfo = new Label();
            lblNome = new Label();
            tabs = new TabControl();
            tabGeral = new TabPage();
            rtbGeral = new RichTextBox();
            tabMateriais = new TabPage();
            splitMateriais = new SplitContainer();
            lstMateriais = new ListBox();
            rtbMaterial = new RichTextBox();
            tabProva = new TabPage();
            rtbProva = new RichTextBox();
            pnlAcoes = new Panel();
            lblErro = new Label();
            btnFechar = new Button();
            btnRejeitar = new Button();
            btnAprovar = new Button();
            txtMotivo = new TextBox();
            lblMotivo = new Label();
            pnlCabecalho.SuspendLayout();
            tabs.SuspendLayout();
            tabGeral.SuspendLayout();
            tabMateriais.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMateriais).BeginInit();
            splitMateriais.Panel1.SuspendLayout();
            splitMateriais.Panel2.SuspendLayout();
            splitMateriais.SuspendLayout();
            tabProva.SuspendLayout();
            pnlAcoes.SuspendLayout();
            SuspendLayout();
            //
            // pnlCabecalho
            //
            pnlCabecalho.BackColor = Color.White;
            pnlCabecalho.Controls.Add(lblStatus);
            pnlCabecalho.Controls.Add(lblInfo);
            pnlCabecalho.Controls.Add(lblNome);
            pnlCabecalho.Dock = DockStyle.Top;
            pnlCabecalho.Location = new Point(0, 0);
            pnlCabecalho.Name = "pnlCabecalho";
            pnlCabecalho.Size = new Size(980, 92);
            pnlCabecalho.TabIndex = 0;
            //
            // lblStatus
            //
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStatus.BackColor = Color.FromArgb(241, 245, 249);
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(840, 18);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(120, 28);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "CARREGANDO";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblInfo
            //
            lblInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblInfo.ForeColor = Color.FromArgb(71, 85, 105);
            lblInfo.Location = new Point(20, 56);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(800, 24);
            lblInfo.TabIndex = 1;
            //
            // lblNome
            //
            lblNome.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblNome.AutoEllipsis = true;
            lblNome.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblNome.ForeColor = Color.FromArgb(15, 23, 42);
            lblNome.Location = new Point(18, 14);
            lblNome.Name = "lblNome";
            lblNome.Size = new Size(810, 36);
            lblNome.TabIndex = 0;
            lblNome.Text = "Carregando curso...";
            //
            // tabs
            //
            tabs.Controls.Add(tabGeral);
            tabs.Controls.Add(tabMateriais);
            tabs.Controls.Add(tabProva);
            tabs.Dock = DockStyle.Fill;
            tabs.Enabled = false;
            tabs.Location = new Point(0, 92);
            tabs.Name = "tabs";
            tabs.Padding = new Point(14, 6);
            tabs.SelectedIndex = 0;
            tabs.Size = new Size(980, 472);
            tabs.TabIndex = 1;
            //
            // tabGeral
            //
            tabGeral.Controls.Add(rtbGeral);
            tabGeral.Location = new Point(4, 34);
            tabGeral.Name = "tabGeral";
            tabGeral.Padding = new Padding(12);
            tabGeral.Size = new Size(972, 434);
            tabGeral.TabIndex = 0;
            tabGeral.Text = "Visão geral";
            tabGeral.UseVisualStyleBackColor = true;
            //
            // rtbGeral
            //
            rtbGeral.BackColor = Color.White;
            rtbGeral.BorderStyle = BorderStyle.None;
            rtbGeral.Dock = DockStyle.Fill;
            rtbGeral.Location = new Point(12, 12);
            rtbGeral.Name = "rtbGeral";
            rtbGeral.ReadOnly = true;
            rtbGeral.Size = new Size(948, 410);
            rtbGeral.TabIndex = 0;
            rtbGeral.Text = "";
            //
            // tabMateriais
            //
            tabMateriais.Controls.Add(splitMateriais);
            tabMateriais.Location = new Point(4, 34);
            tabMateriais.Name = "tabMateriais";
            tabMateriais.Padding = new Padding(12);
            tabMateriais.Size = new Size(972, 434);
            tabMateriais.TabIndex = 1;
            tabMateriais.Text = "Materiais";
            tabMateriais.UseVisualStyleBackColor = true;
            //
            // splitMateriais
            //
            splitMateriais.Dock = DockStyle.Fill;
            splitMateriais.Location = new Point(12, 12);
            splitMateriais.Name = "splitMateriais";
            //
            // splitMateriais.Panel1
            //
            splitMateriais.Panel1.Controls.Add(lstMateriais);
            //
            // splitMateriais.Panel2
            //
            splitMateriais.Panel2.Controls.Add(rtbMaterial);
            splitMateriais.Size = new Size(948, 410);
            splitMateriais.SplitterDistance = 280;
            splitMateriais.TabIndex = 0;
            //
            // lstMateriais
            //
            lstMateriais.BorderStyle = BorderStyle.None;
            lstMateriais.Dock = DockStyle.Fill;
            lstMateriais.IntegralHeight = false;
            lstMateriais.ItemHeight = 17;
            lstMateriais.Location = new Point(0, 0);
            lstMateriais.Name = "lstMateriais";
            lstMateriais.Size = new Size(280, 410);
            lstMateriais.TabIndex = 0;
            lstMateriais.SelectedIndexChanged += lstMateriais_SelectedIndexChanged;
            //
            // rtbMaterial
            //
            rtbMaterial.BackColor = Color.White;
            rtbMaterial.BorderStyle = BorderStyle.None;
            rtbMaterial.Dock = DockStyle.Fill;
            rtbMaterial.Location = new Point(0, 0);
            rtbMaterial.Name = "rtbMaterial";
            rtbMaterial.ReadOnly = true;
            rtbMaterial.Size = new Size(664, 410);
            rtbMaterial.TabIndex = 0;
            rtbMaterial.Text = "";
            //
            // tabProva
            //
            tabProva.Controls.Add(rtbProva);
            tabProva.Location = new Point(4, 34);
            tabProva.Name = "tabProva";
            tabProva.Padding = new Padding(12);
            tabProva.Size = new Size(972, 434);
            tabProva.TabIndex = 2;
            tabProva.Text = "Prova";
            tabProva.UseVisualStyleBackColor = true;
            //
            // rtbProva
            //
            rtbProva.BackColor = Color.White;
            rtbProva.BorderStyle = BorderStyle.None;
            rtbProva.Dock = DockStyle.Fill;
            rtbProva.Location = new Point(12, 12);
            rtbProva.Name = "rtbProva";
            rtbProva.ReadOnly = true;
            rtbProva.Size = new Size(948, 410);
            rtbProva.TabIndex = 0;
            rtbProva.Text = "";
            //
            // pnlAcoes
            //
            pnlAcoes.BackColor = Color.White;
            pnlAcoes.Controls.Add(lblErro);
            pnlAcoes.Controls.Add(btnFechar);
            pnlAcoes.Controls.Add(btnRejeitar);
            pnlAcoes.Controls.Add(btnAprovar);
            pnlAcoes.Controls.Add(txtMotivo);
            pnlAcoes.Controls.Add(lblMotivo);
            pnlAcoes.Dock = DockStyle.Bottom;
            pnlAcoes.Location = new Point(0, 564);
            pnlAcoes.Name = "pnlAcoes";
            pnlAcoes.Size = new Size(980, 156);
            pnlAcoes.TabIndex = 2;
            //
            // lblMotivo
            //
            lblMotivo.AutoSize = true;
            lblMotivo.ForeColor = Color.FromArgb(71, 85, 105);
            lblMotivo.Location = new Point(18, 12);
            lblMotivo.Name = "lblMotivo";
            lblMotivo.Size = new Size(380, 19);
            lblMotivo.TabIndex = 0;
            lblMotivo.Text = "Motivo da rejeição (enviado ao tutor como mensagem)";
            //
            // txtMotivo
            //
            txtMotivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMotivo.Location = new Point(20, 34);
            txtMotivo.MaxLength = 1000;
            txtMotivo.Multiline = true;
            txtMotivo.Name = "txtMotivo";
            txtMotivo.ScrollBars = ScrollBars.Vertical;
            txtMotivo.Size = new Size(600, 72);
            txtMotivo.TabIndex = 1;
            //
            // btnAprovar
            //
            btnAprovar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAprovar.BackColor = Color.FromArgb(21, 128, 61);
            btnAprovar.Cursor = Cursors.Hand;
            btnAprovar.Enabled = false;
            btnAprovar.FlatAppearance.BorderSize = 0;
            btnAprovar.FlatStyle = FlatStyle.Flat;
            btnAprovar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAprovar.ForeColor = Color.White;
            btnAprovar.Location = new Point(640, 34);
            btnAprovar.Name = "btnAprovar";
            btnAprovar.Size = new Size(320, 36);
            btnAprovar.TabIndex = 3;
            btnAprovar.Text = "Aprovar e publicar";
            btnAprovar.UseVisualStyleBackColor = false;
            btnAprovar.Click += btnAprovar_Click;
            //
            // btnRejeitar
            //
            btnRejeitar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRejeitar.BackColor = Color.White;
            btnRejeitar.Cursor = Cursors.Hand;
            btnRejeitar.Enabled = false;
            btnRejeitar.FlatAppearance.BorderColor = Color.FromArgb(185, 28, 28);
            btnRejeitar.FlatStyle = FlatStyle.Flat;
            btnRejeitar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRejeitar.ForeColor = Color.FromArgb(185, 28, 28);
            btnRejeitar.Location = new Point(640, 76);
            btnRejeitar.Name = "btnRejeitar";
            btnRejeitar.Size = new Size(156, 32);
            btnRejeitar.TabIndex = 2;
            btnRejeitar.Text = "Rejeitar";
            btnRejeitar.UseVisualStyleBackColor = false;
            btnRejeitar.Click += btnRejeitar_Click;
            //
            // btnFechar
            //
            btnFechar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFechar.BackColor = Color.White;
            btnFechar.Cursor = Cursors.Hand;
            btnFechar.DialogResult = DialogResult.Cancel;
            btnFechar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnFechar.FlatStyle = FlatStyle.Flat;
            btnFechar.Location = new Point(804, 76);
            btnFechar.Name = "btnFechar";
            btnFechar.Size = new Size(156, 32);
            btnFechar.TabIndex = 4;
            btnFechar.Text = "Fechar";
            btnFechar.UseVisualStyleBackColor = false;
            //
            // lblErro
            //
            lblErro.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblErro.ForeColor = Color.FromArgb(185, 28, 28);
            lblErro.Location = new Point(18, 114);
            lblErro.Name = "lblErro";
            lblErro.Size = new Size(942, 34);
            lblErro.TabIndex = 5;
            //
            // FormRevisaoCurso
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(241, 245, 249);
            CancelButton = btnFechar;
            ClientSize = new Size(980, 720);
            Controls.Add(tabs);
            Controls.Add(pnlAcoes);
            Controls.Add(pnlCabecalho);
            Font = new Font("Segoe UI", 10F);
            MinimizeBox = false;
            MinimumSize = new Size(820, 600);
            Name = "FormRevisaoCurso";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Revisão de curso";
            pnlCabecalho.ResumeLayout(false);
            tabs.ResumeLayout(false);
            tabGeral.ResumeLayout(false);
            tabMateriais.ResumeLayout(false);
            splitMateriais.Panel1.ResumeLayout(false);
            splitMateriais.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMateriais).EndInit();
            splitMateriais.ResumeLayout(false);
            tabProva.ResumeLayout(false);
            pnlAcoes.ResumeLayout(false);
            pnlAcoes.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlCabecalho;
        private Label lblStatus;
        private Label lblInfo;
        private Label lblNome;
        private TabControl tabs;
        private TabPage tabGeral;
        private RichTextBox rtbGeral;
        private TabPage tabMateriais;
        private SplitContainer splitMateriais;
        private ListBox lstMateriais;
        private RichTextBox rtbMaterial;
        private TabPage tabProva;
        private RichTextBox rtbProva;
        private Panel pnlAcoes;
        private Label lblErro;
        private Button btnFechar;
        private Button btnRejeitar;
        private Button btnAprovar;
        private TextBox txtMotivo;
        private Label lblMotivo;
    }
}
