using Microsoft.EntityFrameworkCore;
using TechQuest.Domain.Entidades;
using TechQuest.Infrastructure.Persistencia.Consultas;

namespace TechQuest.Infrastructure.Persistencia;

/// <summary>
/// Mapeamento DATABASE-FIRST do esquema entregue no PIM III.
///
/// O banco e a fonte de verdade: nao ha Migrations neste projeto. Todo o
/// mapeamento e explicito via Fluent API, com os nomes reais de tabelas e
/// colunas, para que o modelo do PIM III continue valido sem renomear nada.
///
/// ALTERNATIVA REJEITADA: Code-First com Migrations. Recriaria o banco e
/// criaria uma segunda fonte de verdade, conflitando com o script que e
/// entregavel da Etapa 7.
/// </summary>
public class TechQuestDbContext : DbContext
{
    public TechQuestDbContext(DbContextOptions<TechQuestDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Adm> Adms => Set<Adm>();
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Estudante> Estudantes => Set<Estudante>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Material> Materiais => Set<Material>();
    public DbSet<Prova> Provas => Set<Prova>();
    public DbSet<Questao> Questoes => Set<Questao>();
    public DbSet<Alternativa> Alternativas => Set<Alternativa>();
    public DbSet<Desempenho> Desempenhos => Set<Desempenho>();
    public DbSet<Progresso> Progressos => Set<Progresso>();
    public DbSet<Medalha> Medalhas => Set<Medalha>();
    public DbSet<Conquista> Conquistas => Set<Conquista>();
    public DbSet<Historico> Historicos => Set<Historico>();
    public DbSet<Certificado> Certificados => Set<Certificado>();
    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<RelatorioDesempenhoItem> RelatorioDesempenho => Set<RelatorioDesempenhoItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ------------------------------------------------------------------
        // USUARIO
        // ------------------------------------------------------------------
        mb.Entity<Usuario>(e =>
        {
            // ATENCAO (bug real, nao teorico): a partir do EF Core 7 o SaveChanges
            // usa a clausula OUTPUT, que o SQL Server proibe em tabela com trigger
            // habilitada. A tabela Usuario tem a TRG_Auditoria_StatusUsuario, logo
            // sem HasTrigger o UPDATE quebra em tempo de execucao.
            e.ToTable("Usuario", tb => tb.HasTrigger("TRG_Auditoria_StatusUsuario"));

            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).HasColumnName("ID_Usuario");
            e.Property(x => x.Nome).HasColumnName("Nome_Usuario").HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasColumnName("Email_Usuario").HasMaxLength(100);
            e.Property(x => x.SenhaHash).HasColumnName("Senha_Usuario").HasMaxLength(255);
            e.Property(x => x.DataCadastro).HasColumnName("Data_Cadastro");
            e.Property(x => x.Ativo).HasColumnName("Status_Usuario");
            e.Property(x => x.Telefone).HasColumnName("Telefone_Usuario").HasMaxLength(20);
            e.Property(x => x.DataNascimento).HasColumnName("Data_Nascimento");
            e.Property(x => x.Cidade).HasColumnName("Cidade_Usuario").HasMaxLength(100);

            e.HasIndex(x => x.Email).IsUnique();
        });

        // ------------------------------------------------------------------
        // ESPECIALIZACOES (heranca por tabelas separadas, como no diagrama)
        // ------------------------------------------------------------------
        mb.Entity<Adm>(e =>
        {
            e.ToTable("ADM");
            e.HasKey(x => x.IdAdm);
            e.Property(x => x.IdAdm).HasColumnName("ID_ADM");
            e.Property(x => x.IdUsuario).HasColumnName("ID_Usuario");
            e.HasOne(x => x.Usuario).WithOne(u => u.Adm)
             .HasForeignKey<Adm>(x => x.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Tutor>(e =>
        {
            e.ToTable("Tutor");
            e.HasKey(x => x.IdTutor);
            e.Property(x => x.IdTutor).HasColumnName("ID_Tutor");
            e.Property(x => x.IdUsuario).HasColumnName("ID_Usuario");
            e.HasOne(x => x.Usuario).WithOne(u => u.Tutor)
             .HasForeignKey<Tutor>(x => x.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Estudante>(e =>
        {
            e.ToTable("Estudante");
            e.HasKey(x => x.IdEstudante);
            e.Property(x => x.IdEstudante).HasColumnName("ID_Estudante");
            e.Property(x => x.IdUsuario).HasColumnName("ID_Usuario");
            e.HasOne(x => x.Usuario).WithOne(u => u.Estudante)
             .HasForeignKey<Estudante>(x => x.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ------------------------------------------------------------------
        // CURSO / MATERIAL
        // ------------------------------------------------------------------
        mb.Entity<Curso>(e =>
        {
            e.ToTable("Curso");
            e.HasKey(x => x.IdCurso);
            e.Property(x => x.IdCurso).HasColumnName("ID_Curso");
            e.Property(x => x.Nome).HasColumnName("Nome_Curso").HasMaxLength(150).IsRequired();
            e.Property(x => x.Descricao).HasColumnName("Descricao_Curso").HasMaxLength(500);
            e.Property(x => x.Categoria).HasColumnName("Categoria_Curso").HasMaxLength(50);
            e.Property(x => x.Nivel).HasColumnName("Nivel_Curso").HasMaxLength(30);
            e.Property(x => x.DuracaoHoras).HasColumnName("Duracao_Horas");
            e.Property(x => x.Status).HasColumnName("Status_Curso").HasMaxLength(30);
            e.Property(x => x.IdTutorCriou).HasColumnName("ID_Tutor_Criou");
            e.Property(x => x.IdAdmAvaliou).HasColumnName("ID_ADM_Avaliou");
            e.Property(x => x.IdProva).HasColumnName("ID_Prova");

            e.HasOne(x => x.TutorCriou).WithMany(t => t.CursosCriados)
             .HasForeignKey(x => x.IdTutorCriou).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AdmAvaliou).WithMany(a => a.CursosAvaliados)
             .HasForeignKey(x => x.IdAdmAvaliou).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Prova).WithMany()
             .HasForeignKey(x => x.IdProva).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Material>(e =>
        {
            e.ToTable("Material");
            e.HasKey(x => x.IdMaterial);
            e.Property(x => x.IdMaterial).HasColumnName("ID_Material");
            e.Property(x => x.IdCurso).HasColumnName("ID_Curso");
            e.Property(x => x.Titulo).HasColumnName("Titulo_Material").HasMaxLength(150);
            e.Property(x => x.Tipo).HasColumnName("Tipo_Material").HasMaxLength(50);

            e.HasOne(x => x.Curso).WithMany(c => c.Materiais)
             .HasForeignKey(x => x.IdCurso).OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // PROVA / QUESTAO / ALTERNATIVA
        // ------------------------------------------------------------------
        mb.Entity<Prova>(e =>
        {
            e.ToTable("Prova");
            e.HasKey(x => x.IdProva);
            e.Property(x => x.IdProva).HasColumnName("ID_Prova");
            e.Property(x => x.Titulo).HasColumnName("Titulo_Prova").HasMaxLength(100);
            e.Property(x => x.NotaMinima).HasColumnName("Nota_Minima").HasColumnType("decimal(4,2)");
            e.Property(x => x.TempoMinutos).HasColumnName("Tempo_Minutos");
        });

        mb.Entity<Questao>(e =>
        {
            e.ToTable("Questao");
            e.HasKey(x => x.IdQuestao);
            e.Property(x => x.IdQuestao).HasColumnName("ID_Questao");
            e.Property(x => x.IdProva).HasColumnName("ID_Prova");
            e.Property(x => x.Enunciado).HasColumnName("Enunciado").IsRequired();
            e.Property(x => x.CodigoExemplo).HasColumnName("Codigo_Exemplo");
            e.Property(x => x.Ordem).HasColumnName("Ordem_Questao");

            e.HasOne(x => x.Prova).WithMany(p => p.Questoes)
             .HasForeignKey(x => x.IdProva).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Alternativa>(e =>
        {
            e.ToTable("Alternativas"); // nome real da tabela, no plural
            e.HasKey(x => x.IdAlternativa);
            e.Property(x => x.IdAlternativa).HasColumnName("ID_Alternativa");
            e.Property(x => x.IdQuestao).HasColumnName("ID_Questao");
            e.Property(x => x.Letra).HasColumnName("Letra_Alternativa").HasMaxLength(1).IsFixedLength();
            e.Property(x => x.Texto).HasColumnName("Texto_Alternativa").HasMaxLength(500);
            e.Property(x => x.EhCorreta).HasColumnName("Eh_Correta");

            e.HasOne(x => x.Questao).WithMany(q => q.Alternativas)
             .HasForeignKey(x => x.IdQuestao).OnDelete(DeleteBehavior.Cascade);
        });

        // ------------------------------------------------------------------
        // DESEMPENHO / PROGRESSO
        // ------------------------------------------------------------------
        mb.Entity<Desempenho>(e =>
        {
            e.ToTable("Desempenho");
            e.HasKey(x => x.IdDesempenho);
            e.Property(x => x.IdDesempenho).HasColumnName("ID_Desempenho");
            e.Property(x => x.IdEstudante).HasColumnName("ID_Estudante");
            e.Property(x => x.IdProva).HasColumnName("ID_Prova");
            e.Property(x => x.Nota).HasColumnName("Nota").HasColumnType("decimal(5,2)");
            e.Property(x => x.DataRealizacao).HasColumnName("Data_Realizacao");
            e.Property(x => x.Tentativas).HasColumnName("Tentativas");

            e.HasOne(x => x.Estudante).WithMany(s => s.Desempenhos)
             .HasForeignKey(x => x.IdEstudante).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Prova).WithMany(p => p.Desempenhos)
             .HasForeignKey(x => x.IdProva).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Progresso>(e =>
        {
            e.ToTable("Progresso");
            e.HasKey(x => x.IdProgresso);
            e.Property(x => x.IdProgresso).HasColumnName("ID_Progresso");
            e.Property(x => x.IdEstudante).HasColumnName("ID_Estudante");
            e.Property(x => x.IdMaterial).HasColumnName("ID_Material");
            e.Property(x => x.Concluido).HasColumnName("Status_Conclusao");
            e.Property(x => x.DataVisualizacao).HasColumnName("Data_Visualizacao");
            e.Property(x => x.PorcentagemAssistida).HasColumnName("Porcentagem_Assistida");

            e.HasOne(x => x.Estudante).WithMany(s => s.Progressos)
             .HasForeignKey(x => x.IdEstudante).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Material).WithMany(m => m.Progressos)
             .HasForeignKey(x => x.IdMaterial).OnDelete(DeleteBehavior.Restrict);
        });

        // ------------------------------------------------------------------
        // MEDALHA / CONQUISTA
        // ------------------------------------------------------------------
        mb.Entity<Medalha>(e =>
        {
            e.ToTable("Medalha");
            e.HasKey(x => x.IdMedalha);
            e.Property(x => x.IdMedalha).HasColumnName("ID_Medalha");
            e.Property(x => x.Nome).HasColumnName("Nome_Medalha").HasMaxLength(50).IsRequired();
            e.Property(x => x.Raridade).HasColumnName("Raridade").HasMaxLength(20);
            e.Property(x => x.Descricao).HasColumnName("Descricao_Medalha").HasMaxLength(200);
        });

        mb.Entity<Conquista>(e =>
        {
            e.ToTable("Conquista");
            e.HasKey(x => x.IdConquista);
            e.Property(x => x.IdConquista).HasColumnName("ID_Conquista");
            e.Property(x => x.IdEstudante).HasColumnName("ID_Estudante");
            e.Property(x => x.IdMedalha).HasColumnName("ID_Medalha");
            e.Property(x => x.DataConquista).HasColumnName("Data_Conquista");

            e.HasOne(x => x.Estudante).WithMany(s => s.Conquistas)
             .HasForeignKey(x => x.IdEstudante).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Medalha).WithMany(m => m.Conquistas)
             .HasForeignKey(x => x.IdMedalha).OnDelete(DeleteBehavior.Restrict);
        });

        // ------------------------------------------------------------------
        // HISTORICO / CERTIFICADO
        // ------------------------------------------------------------------
        mb.Entity<Historico>(e =>
        {
            e.ToTable("Historico");
            e.HasKey(x => x.IdHistorico);
            e.Property(x => x.IdHistorico).HasColumnName("ID_Historico");
            e.Property(x => x.IdEstudante).HasColumnName("ID_Estudante");
            e.Property(x => x.IdCurso).HasColumnName("ID_Curso");
            e.Property(x => x.StatusConclusao).HasColumnName("Status_Conclusao").HasMaxLength(20);
            e.Property(x => x.DataConclusao).HasColumnName("Data_Conclusao");

            e.HasOne(x => x.Estudante).WithMany(s => s.Historicos)
             .HasForeignKey(x => x.IdEstudante).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Curso).WithMany(c => c.Historicos)
             .HasForeignKey(x => x.IdCurso).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Certificado>(e =>
        {
            e.ToTable("Certificado");
            e.HasKey(x => x.IdCertificado);
            e.Property(x => x.IdCertificado).HasColumnName("ID_Certificado");
            e.Property(x => x.IdHistorico).HasColumnName("ID_Historico");
            e.Property(x => x.DataEmissao).HasColumnName("Data_Emissao");

            // O codigo de autenticacao e gerado pelo banco (DEFAULT NEWID()),
            // nao pela aplicacao: garante unicidade mesmo com varias instancias
            // da API rodando em paralelo na nuvem.
            e.Property(x => x.CodigoAutenticacao)
             .HasColumnName("Codigo_Autenticacao")
             .HasDefaultValueSql("NEWID()")
             .ValueGeneratedOnAdd();

            e.HasOne(x => x.Historico).WithOne(h => h.Certificado)
             .HasForeignKey<Certificado>(x => x.IdHistorico)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ------------------------------------------------------------------
        // CHAMADO / LOG
        // ------------------------------------------------------------------
        mb.Entity<Chamado>(e =>
        {
            e.ToTable("Chamado");
            e.HasKey(x => x.IdChamado);
            e.Property(x => x.IdChamado).HasColumnName("ID_Chamado");
            e.Property(x => x.IdRemetente).HasColumnName("ID_Remetente");
            e.Property(x => x.IdDestinatario).HasColumnName("ID_Destinatario");
            e.Property(x => x.Tipo).HasColumnName("Tipo_Chamado").HasMaxLength(20);
            e.Property(x => x.Assunto).HasColumnName("Assunto").HasMaxLength(100);
            e.Property(x => x.Descricao).HasColumnName("Descricao");
            e.Property(x => x.DataAbertura).HasColumnName("Data_Abertura");
            e.Property(x => x.Status).HasColumnName("Status_Chamado").HasMaxLength(20);

            // Duas FKs para a mesma tabela: precisam ser declaradas
            // explicitamente, senao o EF cria relacionamentos duplicados.
            e.HasOne(x => x.Remetente).WithMany()
             .HasForeignKey(x => x.IdRemetente).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Destinatario).WithMany()
             .HasForeignKey(x => x.IdDestinatario).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<LogAuditoria>(e =>
        {
            e.ToTable("Log_Auditoria");
            e.HasKey(x => x.IdLog);
            e.Property(x => x.IdLog).HasColumnName("ID_Log");
            e.Property(x => x.IdAdm).HasColumnName("ID_ADM");
            e.Property(x => x.AcaoRealizada).HasColumnName("Acao_Realizada").HasMaxLength(255);
            e.Property(x => x.DataAcao).HasColumnName("Data_Acao");

            e.HasOne(x => x.Adm).WithMany(a => a.Logs)
             .HasForeignKey(x => x.IdAdm).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
