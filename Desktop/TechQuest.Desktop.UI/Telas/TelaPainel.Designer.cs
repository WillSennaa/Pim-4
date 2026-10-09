#nullable disable

namespace TechQuest.Desktop.UI.Telas
{
    partial class TelaPainel
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
            lblAtualizado = new Label();
            pnlFalha = new Panel();
            lblFalha = new Label();
            btnTentarNovamente = new Button();
            tlpIndicadores = new TableLayoutPanel();
            lblSecaoUsuarios = new Label();
            lblSecaoCursos = new Label();
            lblSecaoAtencao = new Label();
            cartaoUsuarios = new Componentes.CartaoIndicador();
            cartaoAtivos = new Componentes.CartaoIndicador();
            cartaoEstudantes = new Componentes.CartaoIndicador();
            cartaoTutores = new Componentes.CartaoIndicador();
            cartaoPublicados = new Componentes.CartaoIndicador();
            cartaoPendentes = new Componentes.CartaoIndicador();
            cartaoMatriculas = new Componentes.CartaoIndicador();
            cartaoCertificados = new Componentes.CartaoIndicador();
            cartaoChamados = new Componentes.CartaoIndicador();
            pnlAtalhoAprovacoes = new Panel();
            btnRevisar = new Button();
            lblAtalhoDetalhe = new Label();
            lblAtalhoTitulo = new Label();
            pnlBarra.SuspendLayout();
            pnlFalha.SuspendLayout();
            tlpIndicadores.SuspendLayout();
            pnlAtalhoAprovacoes.SuspendLayout();
            SuspendLayout();
            //
            // pnlBarra
            //
            pnlBarra.Controls.Add(btnAtualizar);
            pnlBarra.Controls.Add(lblAtualizado);
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
            btnAtualizar.ForeColor = Color.FromArgb(15, 23, 42);
            btnAtualizar.Location = new Point(772, 6);
            btnAtualizar.Name = "btnAtualizar";
            btnAtualizar.Size = new Size(120, 34);
            btnAtualizar.TabIndex = 1;
            btnAtualizar.Text = "Atualizar";
            btnAtualizar.UseVisualStyleBackColor = false;
            btnAtualizar.Click += btnAtualizar_Click;
            //
            // lblAtualizado
            //
            lblAtualizado.AutoSize = true;
            lblAtualizado.ForeColor = Color.FromArgb(148, 163, 184);
            lblAtualizado.Location = new Point(8, 14);
            lblAtualizado.Name = "lblAtualizado";
            lblAtualizado.Size = new Size(0, 19);
            lblAtualizado.TabIndex = 0;
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
            btnTentarNovamente.Click += btnTentarNovamente_Click;
            //
            // tlpIndicadores
            //
            tlpIndicadores.AutoSize = true;
            tlpIndicadores.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpIndicadores.ColumnCount = 4;
            tlpIndicadores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpIndicadores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpIndicadores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpIndicadores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpIndicadores.Controls.Add(lblSecaoUsuarios, 0, 0);
            tlpIndicadores.Controls.Add(lblSecaoCursos, 0, 2);
            tlpIndicadores.Controls.Add(lblSecaoAtencao, 0, 4);
            tlpIndicadores.Controls.Add(cartaoUsuarios, 0, 1);
            tlpIndicadores.Controls.Add(cartaoAtivos, 1, 1);
            tlpIndicadores.Controls.Add(cartaoEstudantes, 2, 1);
            tlpIndicadores.Controls.Add(cartaoTutores, 3, 1);
            tlpIndicadores.Controls.Add(cartaoPublicados, 0, 3);
            tlpIndicadores.Controls.Add(cartaoPendentes, 1, 3);
            tlpIndicadores.Controls.Add(cartaoMatriculas, 2, 3);
            tlpIndicadores.Controls.Add(cartaoCertificados, 3, 3);
            tlpIndicadores.Controls.Add(cartaoChamados, 3, 5);
            tlpIndicadores.Controls.Add(pnlAtalhoAprovacoes, 0, 5);
            tlpIndicadores.Dock = DockStyle.Top;
            tlpIndicadores.Location = new Point(0, 104);
            tlpIndicadores.Name = "tlpIndicadores";
            tlpIndicadores.RowCount = 6;
            tlpIndicadores.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpIndicadores.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
            tlpIndicadores.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpIndicadores.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
            tlpIndicadores.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpIndicadores.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
            tlpIndicadores.Size = new Size(900, 474);
            tlpIndicadores.TabIndex = 2;
            //
            // lblSecaoUsuarios
            //
            lblSecaoUsuarios.AutoSize = true;
            tlpIndicadores.SetColumnSpan(lblSecaoUsuarios, 4);
            lblSecaoUsuarios.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSecaoUsuarios.ForeColor = Color.FromArgb(71, 85, 105);
            lblSecaoUsuarios.Margin = new Padding(8, 16, 8, 0);
            lblSecaoUsuarios.Name = "lblSecaoUsuarios";
            lblSecaoUsuarios.TabIndex = 0;
            lblSecaoUsuarios.Text = "Usuários";
            //
            // lblSecaoCursos
            //
            lblSecaoCursos.AutoSize = true;
            tlpIndicadores.SetColumnSpan(lblSecaoCursos, 4);
            lblSecaoCursos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSecaoCursos.ForeColor = Color.FromArgb(71, 85, 105);
            lblSecaoCursos.Margin = new Padding(8, 16, 8, 0);
            lblSecaoCursos.Name = "lblSecaoCursos";
            lblSecaoCursos.TabIndex = 10;
            lblSecaoCursos.Text = "Cursos e aprendizado";
            //
            // lblSecaoAtencao
            //
            lblSecaoAtencao.AutoSize = true;
            tlpIndicadores.SetColumnSpan(lblSecaoAtencao, 4);
            lblSecaoAtencao.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSecaoAtencao.ForeColor = Color.FromArgb(71, 85, 105);
            lblSecaoAtencao.Margin = new Padding(8, 16, 8, 0);
            lblSecaoAtencao.Name = "lblSecaoAtencao";
            lblSecaoAtencao.TabIndex = 20;
            lblSecaoAtencao.Text = "Atenção";
            //
            // cartaoUsuarios
            //
            cartaoUsuarios.Detalhe = "contas cadastradas";
            cartaoUsuarios.Dock = DockStyle.Fill;
            cartaoUsuarios.Name = "cartaoUsuarios";
            cartaoUsuarios.TabIndex = 11;
            cartaoUsuarios.Titulo = "Usuários";
            //
            // cartaoAtivos
            //
            cartaoAtivos.Dock = DockStyle.Fill;
            cartaoAtivos.Name = "cartaoAtivos";
            cartaoAtivos.TabIndex = 12;
            cartaoAtivos.Titulo = "Ativos";
            //
            // cartaoEstudantes
            //
            cartaoEstudantes.Dock = DockStyle.Fill;
            cartaoEstudantes.Name = "cartaoEstudantes";
            cartaoEstudantes.TabIndex = 13;
            cartaoEstudantes.Titulo = "Estudantes";
            //
            // cartaoTutores
            //
            cartaoTutores.Dock = DockStyle.Fill;
            cartaoTutores.Name = "cartaoTutores";
            cartaoTutores.TabIndex = 14;
            cartaoTutores.Titulo = "Tutores";
            //
            // cartaoPublicados
            //
            cartaoPublicados.Detalhe = "visíveis aos estudantes";
            cartaoPublicados.Dock = DockStyle.Fill;
            cartaoPublicados.Name = "cartaoPublicados";
            cartaoPublicados.TabIndex = 31;
            cartaoPublicados.Titulo = "Cursos publicados";
            //
            // cartaoPendentes
            //
            cartaoPendentes.Detalhe = "enviados por tutores";
            cartaoPendentes.Dock = DockStyle.Fill;
            cartaoPendentes.Name = "cartaoPendentes";
            cartaoPendentes.TabIndex = 32;
            cartaoPendentes.Titulo = "Aguardando aprovação";
            //
            // cartaoMatriculas
            //
            cartaoMatriculas.Detalhe = "inscrições em cursos";
            cartaoMatriculas.Dock = DockStyle.Fill;
            cartaoMatriculas.Name = "cartaoMatriculas";
            cartaoMatriculas.TabIndex = 33;
            cartaoMatriculas.Titulo = "Matrículas";
            //
            // cartaoCertificados
            //
            cartaoCertificados.Dock = DockStyle.Fill;
            cartaoCertificados.Name = "cartaoCertificados";
            cartaoCertificados.TabIndex = 34;
            cartaoCertificados.Titulo = "Certificados emitidos";
            //
            // cartaoChamados
            //
            cartaoChamados.Detalhe = "dúvidas e técnicos, todos";
            cartaoChamados.Dock = DockStyle.Fill;
            cartaoChamados.Name = "cartaoChamados";
            cartaoChamados.TabIndex = 54;
            cartaoChamados.Titulo = "Chamados abertos";
            //
            // pnlAtalhoAprovacoes
            //
            pnlAtalhoAprovacoes.BackColor = Color.White;
            tlpIndicadores.SetColumnSpan(pnlAtalhoAprovacoes, 3);
            pnlAtalhoAprovacoes.Controls.Add(btnRevisar);
            pnlAtalhoAprovacoes.Controls.Add(lblAtalhoDetalhe);
            pnlAtalhoAprovacoes.Controls.Add(lblAtalhoTitulo);
            pnlAtalhoAprovacoes.Dock = DockStyle.Fill;
            pnlAtalhoAprovacoes.Margin = new Padding(8);
            pnlAtalhoAprovacoes.Name = "pnlAtalhoAprovacoes";
            pnlAtalhoAprovacoes.Padding = new Padding(16, 14, 16, 8);
            pnlAtalhoAprovacoes.TabIndex = 50;
            //
            // lblAtalhoTitulo
            //
            lblAtalhoTitulo.Dock = DockStyle.Top;
            lblAtalhoTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblAtalhoTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblAtalhoTitulo.Location = new Point(16, 14);
            lblAtalhoTitulo.Name = "lblAtalhoTitulo";
            lblAtalhoTitulo.Size = new Size(620, 26);
            lblAtalhoTitulo.TabIndex = 0;
            lblAtalhoTitulo.Text = "Aprovações";
            //
            // lblAtalhoDetalhe
            //
            lblAtalhoDetalhe.Dock = DockStyle.Top;
            lblAtalhoDetalhe.ForeColor = Color.FromArgb(71, 85, 105);
            lblAtalhoDetalhe.Location = new Point(16, 40);
            lblAtalhoDetalhe.Name = "lblAtalhoDetalhe";
            lblAtalhoDetalhe.Size = new Size(620, 24);
            lblAtalhoDetalhe.TabIndex = 1;
            //
            // btnRevisar
            //
            btnRevisar.BackColor = Color.FromArgb(37, 99, 235);
            btnRevisar.Cursor = Cursors.Hand;
            btnRevisar.FlatAppearance.BorderSize = 0;
            btnRevisar.FlatStyle = FlatStyle.Flat;
            btnRevisar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRevisar.ForeColor = Color.White;
            btnRevisar.Location = new Point(16, 70);
            btnRevisar.Name = "btnRevisar";
            btnRevisar.Size = new Size(170, 34);
            btnRevisar.TabIndex = 2;
            btnRevisar.Text = "Ir para aprovações";
            btnRevisar.UseVisualStyleBackColor = false;
            btnRevisar.Click += btnRevisar_Click;
            //
            // TelaPainel
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(tlpIndicadores);
            Controls.Add(pnlFalha);
            Controls.Add(pnlBarra);
            Font = new Font("Segoe UI", 10F);
            Name = "TelaPainel";
            Size = new Size(900, 640);
            pnlBarra.ResumeLayout(false);
            pnlBarra.PerformLayout();
            pnlFalha.ResumeLayout(false);
            tlpIndicadores.ResumeLayout(false);
            tlpIndicadores.PerformLayout();
            pnlAtalhoAprovacoes.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlBarra;
        private Button btnAtualizar;
        private Label lblAtualizado;
        private Panel pnlFalha;
        private Label lblFalha;
        private Button btnTentarNovamente;
        private TableLayoutPanel tlpIndicadores;
        private Label lblSecaoUsuarios;
        private Label lblSecaoCursos;
        private Label lblSecaoAtencao;
        private Componentes.CartaoIndicador cartaoUsuarios;
        private Componentes.CartaoIndicador cartaoAtivos;
        private Componentes.CartaoIndicador cartaoEstudantes;
        private Componentes.CartaoIndicador cartaoTutores;
        private Componentes.CartaoIndicador cartaoPublicados;
        private Componentes.CartaoIndicador cartaoPendentes;
        private Componentes.CartaoIndicador cartaoMatriculas;
        private Componentes.CartaoIndicador cartaoCertificados;
        private Componentes.CartaoIndicador cartaoChamados;
        private Panel pnlAtalhoAprovacoes;
        private Label lblAtalhoTitulo;
        private Label lblAtalhoDetalhe;
        private Button btnRevisar;
    }
}
