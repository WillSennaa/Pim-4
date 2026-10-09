#nullable disable

namespace TechQuest.Desktop.UI.Formularios
{
    partial class FormMedalha
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
            lblRaridadeCampo = new Label();
            cmbRaridade = new ComboBox();
            lblDescricao = new Label();
            txtDescricao = new TextBox();
            lblAviso = new Label();
            lblErro = new Label();
            btnCancelar = new Button();
            btnSalvar = new Button();
            SuspendLayout();
            //
            // lblTitulo
            //
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitulo.Location = new Point(22, 16);
            lblTitulo.Text = "Medalha";
            lblTitulo.Name = "lblTitulo";
            lblTitulo.TabIndex = 0;
            //
            // lblNome
            //
            lblNome.AutoSize = true;
            lblNome.ForeColor = Color.FromArgb(71, 85, 105);
            lblNome.Location = new Point(24, 62);
            lblNome.Text = "Nome (máximo de 50 caracteres)";
            lblNome.Name = "lblNome";
            lblNome.TabIndex = 1;
            //
            // txtNome
            //
            txtNome.Location = new Point(24, 84);
            txtNome.MaxLength = 50;
            txtNome.Size = new Size(392, 26);
            txtNome.Name = "txtNome";
            txtNome.TabIndex = 2;
            //
            // lblRaridadeCampo
            //
            lblRaridadeCampo.AutoSize = true;
            lblRaridadeCampo.ForeColor = Color.FromArgb(71, 85, 105);
            lblRaridadeCampo.Location = new Point(24, 122);
            lblRaridadeCampo.Text = "Raridade";
            lblRaridadeCampo.Name = "lblRaridadeCampo";
            lblRaridadeCampo.TabIndex = 3;
            //
            // cmbRaridade
            //
            cmbRaridade.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRaridade.FlatStyle = FlatStyle.Flat;
            cmbRaridade.Location = new Point(24, 144);
            cmbRaridade.Size = new Size(200, 25);
            cmbRaridade.Name = "cmbRaridade";
            cmbRaridade.TabIndex = 4;
            //
            // lblDescricao
            //
            lblDescricao.AutoSize = true;
            lblDescricao.ForeColor = Color.FromArgb(71, 85, 105);
            lblDescricao.Location = new Point(24, 182);
            lblDescricao.Text = "Como se conquista (máximo de 200 caracteres)";
            lblDescricao.Name = "lblDescricao";
            lblDescricao.TabIndex = 5;
            //
            // txtDescricao
            //
            txtDescricao.Location = new Point(24, 204);
            txtDescricao.MaxLength = 200;
            txtDescricao.Multiline = true;
            txtDescricao.ScrollBars = ScrollBars.Vertical;
            txtDescricao.Size = new Size(392, 80);
            txtDescricao.Name = "txtDescricao";
            txtDescricao.TabIndex = 6;
            //
            // lblAviso
            //
            lblAviso.ForeColor = Color.FromArgb(180, 83, 9);
            lblAviso.Location = new Point(24, 292);
            lblAviso.Size = new Size(392, 36);
            lblAviso.Text = "";
            lblAviso.Name = "lblAviso";
            lblAviso.TabIndex = 7;
            //
            // lblErro
            //
            lblErro.ForeColor = Color.FromArgb(185, 28, 28);
            lblErro.Location = new Point(24, 330);
            lblErro.Size = new Size(392, 22);
            lblErro.Text = "";
            lblErro.Name = "lblErro";
            lblErro.TabIndex = 8;
            //
            // btnCancelar
            //
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnCancelar.BackColor = Color.White;
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Location = new Point(206, 360);
            btnCancelar.Size = new Size(100, 34);
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Name = "btnCancelar";
            btnCancelar.TabIndex = 10;
            //
            // btnSalvar
            //
            btnSalvar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnSalvar.BackColor = Color.FromArgb(37, 99, 235);
            btnSalvar.Cursor = Cursors.Hand;
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.FlatStyle = FlatStyle.Flat;
            btnSalvar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSalvar.ForeColor = Color.White;
            btnSalvar.Location = new Point(316, 360);
            btnSalvar.Size = new Size(100, 34);
            btnSalvar.Text = "Salvar";
            btnSalvar.UseVisualStyleBackColor = false;
            btnSalvar.Name = "btnSalvar";
            btnSalvar.TabIndex = 9;
            btnSalvar.Click += btnSalvar_Click;
            //
            // FormMedalha
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblTitulo);
            Controls.Add(lblNome);
            Controls.Add(txtNome);
            Controls.Add(lblRaridadeCampo);
            Controls.Add(cmbRaridade);
            Controls.Add(lblDescricao);
            Controls.Add(txtDescricao);
            Controls.Add(lblAviso);
            Controls.Add(lblErro);
            Controls.Add(btnCancelar);
            Controls.Add(btnSalvar);
            AcceptButton = btnSalvar;
            BackColor = Color.White;
            CancelButton = btnCancelar;
            ClientSize = new Size(440, 412);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Medalha";
            Name = "FormMedalha";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblNome;
        private TextBox txtNome;
        private Label lblRaridadeCampo;
        private ComboBox cmbRaridade;
        private Label lblDescricao;
        private TextBox txtDescricao;
        private Label lblAviso;
        private Label lblErro;
        private Button btnCancelar;
        private Button btnSalvar;
    }
}
