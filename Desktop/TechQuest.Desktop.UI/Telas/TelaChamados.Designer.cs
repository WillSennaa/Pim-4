#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaChamados
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
            cmbSituacao = new ComboBox();
            lblFiltro = new Label();
            faixaErro = new Componentes.FaixaDeErro();
            pnlCorpo = new Panel();
            dgvChamados = new Componentes.GradePadrao();
            pnlEspaco = new Panel();
            pnlDetalhe = new Panel();
            pnlConteudoDetalhe = new Panel();
            btnResponder = new Button();
            btnAssumir = new Button();
            txtResposta = new TextBox();
            lblResposta = new Label();
            txtDescricao = new TextBox();
            lblSituacao = new Label();
            lblMeta = new Label();
            lblTitulo = new Label();
            lblSemSelecao = new Label();
            pnlRodape = new Panel();
            lblResumo = new Label();
            colNumero = new DataGridViewTextBoxColumn();
            colAssunto = new DataGridViewTextBoxColumn();
            colRemetente = new DataGridViewTextBoxColumn();
            colAbertoEm = new DataGridViewTextBoxColumn();
            colSituacao = new DataGridViewTextBoxColumn();
            pnlBarra.SuspendLayout();
            pnlCorpo.SuspendLayout();
            pnlDetalhe.SuspendLayout();
            pnlConteudoDetalhe.SuspendLayout();
            pnlRodape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChamados).BeginInit();
            SuspendLayout();
            //
            // pnlBarra
            //
            pnlBarra.Controls.Add(btnAtualizar);
            pnlBarra.Controls.Add(txtBusca);
            pnlBarra.Controls.Add(lblBusca);
            pnlBarra.Controls.Add(cmbSituacao);
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
            txtBusca.Location = new Point(316, 11);
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
            lblBusca.Location = new Point(264, 14);
            lblBusca.Text = "Buscar";
            lblBusca.Name = "lblBusca";
            lblBusca.TabIndex = 2;
            //
            // cmbSituacao
            //
            cmbSituacao.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSituacao.FlatStyle = FlatStyle.Flat;
            cmbSituacao.Location = new Point(76, 11);
            cmbSituacao.Size = new Size(170, 25);
            cmbSituacao.Name = "cmbSituacao";
            cmbSituacao.TabIndex = 1;
            cmbSituacao.SelectedIndexChanged += cmbSituacao_SelectedIndexChanged;
            //
            // lblFiltro
            //
            lblFiltro.AutoSize = true;
            lblFiltro.ForeColor = Color.FromArgb(71, 85, 105);
            lblFiltro.Location = new Point(8, 14);
            lblFiltro.Text = "Situação";
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
            pnlCorpo.Controls.Add(dgvChamados);
            pnlCorpo.Controls.Add(pnlEspaco);
            pnlCorpo.Controls.Add(pnlDetalhe);
            pnlCorpo.Dock = DockStyle.Fill;
            pnlCorpo.Padding = new Padding(8, 8, 8, 0);
            pnlCorpo.Size = new Size(900, 548);
            pnlCorpo.Name = "pnlCorpo";
            pnlCorpo.TabIndex = 2;
            //
            // dgvChamados
            //
            dgvChamados.Columns.AddRange(new DataGridViewColumn[] { colNumero, colAssunto, colRemetente, colAbertoEm, colSituacao });
            dgvChamados.Dock = DockStyle.Fill;
            dgvChamados.Name = "dgvChamados";
            dgvChamados.TabIndex = 8;
            dgvChamados.SelectionChanged += dgvChamados_SelectionChanged;
            dgvChamados.DataBindingComplete += dgvChamados_DataBindingComplete;
            //
            // colNumero
            //
            colNumero.DataPropertyName = "Numero";
            colNumero.FillWeight = 9F;
            colNumero.HeaderText = "#";
            colNumero.Name = "colNumero";
            colNumero.ReadOnly = true;
            //
            // colAssunto
            //
            colAssunto.DataPropertyName = "Assunto";
            colAssunto.FillWeight = 39F;
            colAssunto.HeaderText = "Assunto";
            colAssunto.Name = "colAssunto";
            colAssunto.ReadOnly = true;
            //
            // colRemetente
            //
            colRemetente.DataPropertyName = "Remetente";
            colRemetente.FillWeight = 22F;
            colRemetente.HeaderText = "Aberto por";
            colRemetente.Name = "colRemetente";
            colRemetente.ReadOnly = true;
            //
            // colAbertoEm
            //
            colAbertoEm.DataPropertyName = "AbertoEm";
            colAbertoEm.FillWeight = 17F;
            colAbertoEm.HeaderText = "Em";
            colAbertoEm.Name = "colAbertoEm";
            colAbertoEm.ReadOnly = true;
            //
            // colSituacao
            //
            colSituacao.DataPropertyName = "Situacao";
            colSituacao.FillWeight = 13F;
            colSituacao.HeaderText = "Situação";
            colSituacao.Name = "colSituacao";
            colSituacao.ReadOnly = true;
            //
            // pnlEspaco
            //
            pnlEspaco.Dock = DockStyle.Right;
            pnlEspaco.BackColor = Color.FromArgb(241, 245, 249);
            pnlEspaco.Size = new Size(8, 540);
            pnlEspaco.Name = "pnlEspaco";
            pnlEspaco.TabIndex = 1;
            //
            // pnlDetalhe
            //
            pnlDetalhe.Controls.Add(pnlConteudoDetalhe);
            pnlDetalhe.Controls.Add(lblSemSelecao);
            pnlDetalhe.Dock = DockStyle.Right;
            pnlDetalhe.BackColor = Color.White;
            pnlDetalhe.Padding = new Padding(16);
            pnlDetalhe.Size = new Size(400, 540);
            pnlDetalhe.Name = "pnlDetalhe";
            pnlDetalhe.TabIndex = 2;
            //
            // pnlConteudoDetalhe
            //
            pnlConteudoDetalhe.Controls.Add(btnResponder);
            pnlConteudoDetalhe.Controls.Add(btnAssumir);
            pnlConteudoDetalhe.Controls.Add(txtResposta);
            pnlConteudoDetalhe.Controls.Add(lblResposta);
            pnlConteudoDetalhe.Controls.Add(txtDescricao);
            pnlConteudoDetalhe.Controls.Add(lblSituacao);
            pnlConteudoDetalhe.Controls.Add(lblMeta);
            pnlConteudoDetalhe.Controls.Add(lblTitulo);
            pnlConteudoDetalhe.Dock = DockStyle.Fill;
            pnlConteudoDetalhe.Visible = false;
            pnlConteudoDetalhe.Size = new Size(368, 508);
            pnlConteudoDetalhe.Name = "pnlConteudoDetalhe";
            pnlConteudoDetalhe.TabIndex = 0;
            //
            // btnResponder
            //
            btnResponder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnResponder.BackColor = Color.FromArgb(37, 99, 235);
            btnResponder.Cursor = Cursors.Hand;
            btnResponder.FlatAppearance.BorderSize = 0;
            btnResponder.FlatStyle = FlatStyle.Flat;
            btnResponder.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnResponder.ForeColor = Color.White;
            btnResponder.Location = new Point(118, 470);
            btnResponder.Size = new Size(250, 34);
            btnResponder.Text = "Responder e resolver";
            btnResponder.UseVisualStyleBackColor = false;
            btnResponder.Name = "btnResponder";
            btnResponder.TabIndex = 7;
            btnResponder.Click += btnResponder_Click;
            //
            // btnAssumir
            //
            btnAssumir.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAssumir.BackColor = Color.White;
            btnAssumir.Cursor = Cursors.Hand;
            btnAssumir.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnAssumir.FlatStyle = FlatStyle.Flat;
            btnAssumir.Location = new Point(0, 470);
            btnAssumir.Size = new Size(110, 34);
            btnAssumir.Text = "Assumir";
            btnAssumir.UseVisualStyleBackColor = false;
            btnAssumir.Name = "btnAssumir";
            btnAssumir.TabIndex = 6;
            btnAssumir.Click += btnAssumir_Click;
            //
            // txtResposta
            //
            txtResposta.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtResposta.Location = new Point(0, 292);
            txtResposta.MaxLength = 2000;
            txtResposta.Multiline = true;
            txtResposta.ScrollBars = ScrollBars.Vertical;
            txtResposta.Size = new Size(368, 168);
            txtResposta.Name = "txtResposta";
            txtResposta.TabIndex = 5;
            //
            // lblResposta
            //
            lblResposta.AutoSize = true;
            lblResposta.ForeColor = Color.FromArgb(71, 85, 105);
            lblResposta.Location = new Point(0, 270);
            lblResposta.Text = "Resposta ao usuário";
            lblResposta.Name = "lblResposta";
            lblResposta.TabIndex = 4;
            //
            // txtDescricao
            //
            txtDescricao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescricao.BackColor = Color.FromArgb(248, 250, 252);
            txtDescricao.Location = new Point(0, 118);
            txtDescricao.MaxLength = 32767;
            txtDescricao.Multiline = true;
            txtDescricao.ScrollBars = ScrollBars.Vertical;
            txtDescricao.ReadOnly = true;
            txtDescricao.Size = new Size(368, 140);
            txtDescricao.Name = "txtDescricao";
            txtDescricao.TabIndex = 3;
            //
            // lblSituacao
            //
            lblSituacao.AutoSize = true;
            lblSituacao.ForeColor = Color.FromArgb(71, 85, 105);
            lblSituacao.Location = new Point(0, 94);
            lblSituacao.Text = "";
            lblSituacao.Name = "lblSituacao";
            lblSituacao.TabIndex = 2;
            //
            // lblMeta
            //
            lblMeta.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblMeta.ForeColor = Color.FromArgb(148, 163, 184);
            lblMeta.Location = new Point(0, 50);
            lblMeta.Size = new Size(368, 40);
            lblMeta.Text = "";
            lblMeta.Name = "lblMeta";
            lblMeta.TabIndex = 1;
            //
            // lblTitulo
            //
            lblTitulo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Size = new Size(368, 48);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.TabIndex = 0;
            //
            // lblSemSelecao
            //
            lblSemSelecao.Dock = DockStyle.Fill;
            lblSemSelecao.ForeColor = Color.FromArgb(148, 163, 184);
            lblSemSelecao.Size = new Size(368, 508);
            lblSemSelecao.Text = "Selecione um chamado na lista para ler e responder.";
            lblSemSelecao.TextAlign = ContentAlignment.MiddleCenter;
            lblSemSelecao.Name = "lblSemSelecao";
            lblSemSelecao.TabIndex = 1;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(lblResumo);
            pnlRodape.Dock = DockStyle.Bottom;
            pnlRodape.Size = new Size(900, 44);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.TabIndex = 3;
            //
            // lblResumo
            //
            lblResumo.AutoSize = true;
            lblResumo.ForeColor = Color.FromArgb(71, 85, 105);
            lblResumo.Location = new Point(8, 12);
            lblResumo.Text = "";
            lblResumo.Name = "lblResumo";
            lblResumo.TabIndex = 0;
            //
            // TelaChamados
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
            Name = "TelaChamados";
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlCorpo.ResumeLayout(false);
            pnlCorpo.PerformLayout();
            pnlDetalhe.ResumeLayout(false);
            pnlDetalhe.PerformLayout();
            pnlConteudoDetalhe.ResumeLayout(false);
            pnlConteudoDetalhe.PerformLayout();
            pnlRodape.ResumeLayout(false);
            pnlRodape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChamados).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlBarra;
        private Button btnAtualizar;
        private TextBox txtBusca;
        private Label lblBusca;
        private ComboBox cmbSituacao;
        private Label lblFiltro;
        private Componentes.FaixaDeErro faixaErro;
        private Panel pnlCorpo;
        private Componentes.GradePadrao dgvChamados;
        private Panel pnlEspaco;
        private Panel pnlDetalhe;
        private Panel pnlConteudoDetalhe;
        private Button btnResponder;
        private Button btnAssumir;
        private TextBox txtResposta;
        private Label lblResposta;
        private TextBox txtDescricao;
        private Label lblSituacao;
        private Label lblMeta;
        private Label lblTitulo;
        private Label lblSemSelecao;
        private Panel pnlRodape;
        private Label lblResumo;
        private DataGridViewTextBoxColumn colNumero;
        private DataGridViewTextBoxColumn colAssunto;
        private DataGridViewTextBoxColumn colRemetente;
        private DataGridViewTextBoxColumn colAbertoEm;
        private DataGridViewTextBoxColumn colSituacao;
    }
}
