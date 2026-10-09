-- =============================================================
-- 01_estrutura.sql  |  Tech Quest - criacao completa do banco
--
-- Cria o banco do zero, ja com TUDO que os scripts incrementais
-- (02_ajustes, 04_ajustes_chamado, 05_conteudo_material) acrescentaram.
-- Rode isto num banco NOVO; depois, 02_dados_demonstracao.sql.
--
-- NAO RODE ESTE ARQUIVO NO BANCO QUE JA ESTA EM PRODUCAO.
-- Ele e para criar um banco igual do zero. O banco existente ja chegou
-- neste estado pelos scripts numerados, que continuam no repositorio como
-- o registro do que de fato rodou la -- cada um e um fato datado, nao um
-- rascunho. E a mesma distincao entre "migracoes" e "schema atual" que
-- ferramentas de migracao mantem em arquivos separados.
--
-- DE ONDE VEIO ESTA LISTA DE COLUNAS: nao foi digitada de memoria. Foi
-- extraida do mapeamento Fluent API do TechQuestDbContext, que e o
-- contrato entre a aplicacao e o banco -- se uma coluna faltar aqui, a API
-- quebra na primeira consulta aquela entidade. A conferencia no fim do
-- arquivo compara o banco criado com o que a aplicacao espera.
--
-- COMO RODAR
--   SQL Server local / SSMS / Azure Data Studio: rode o arquivo inteiro.
--   Portal do Azure (editor de consultas): o portal nao aceita 'GO' nem
--   CREATE DATABASE. Crie o banco pelo portal, abra o editor dentro dele e
--   cole a partir da SECAO 2, removendo as linhas 'GO'.
-- =============================================================

-- =============================================================
-- SECAO 1 - BANCO (pular no portal do Azure)
-- =============================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TechQuestDB')
BEGIN
    CREATE DATABASE TechQuestDB;
END
GO

USE TechQuestDB;
GO

-- =============================================================
-- SECAO 2 - TABELAS SEM DEPENDENCIA
-- =============================================================

CREATE TABLE Usuario (
    ID_Usuario       INT IDENTITY(1,1) NOT NULL,
    Nome_Usuario     VARCHAR(100) NOT NULL,
    Email_Usuario    VARCHAR(100) UNIQUE,
    -- 255: o hash BCrypt tem 60 caracteres; a folga cobre troca de algoritmo
    -- sem alterar a tabela. A senha em claro nunca chega aqui.
    Senha_Usuario    VARCHAR(255),
    Data_Cadastro    DATETIME DEFAULT GETDATE(),
    Status_Usuario   BIT DEFAULT 1,              -- 1 ativo, 0 inativo
    -- Acrescentadas no 02: dados de contato do perfil.
    Telefone_Usuario VARCHAR(20),
    Data_Nascimento  DATE,
    Cidade_Usuario   VARCHAR(100),
    CONSTRAINT PK_Usuario PRIMARY KEY (ID_Usuario)
);

CREATE TABLE Medalha (
    ID_Medalha        INT IDENTITY(1,1) NOT NULL,
    Nome_Medalha      VARCHAR(50) NOT NULL,
    Raridade          VARCHAR(20),
    Descricao_Medalha VARCHAR(200),              -- 02
    CONSTRAINT PK_Medalha PRIMARY KEY (ID_Medalha)
);

CREATE TABLE Prova (
    ID_Prova      INT IDENTITY(1,1) NOT NULL,
    Titulo_Prova  VARCHAR(100),
    -- 02: a nota minima e do MODELO, nao uma constante no codigo. Cada prova
    -- pode exigir uma nota diferente.
    Nota_Minima   DECIMAL(4,2) NOT NULL DEFAULT 7.00,
    Tempo_Minutos INT NOT NULL DEFAULT 30,
    CONSTRAINT PK_Prova PRIMARY KEY (ID_Prova)
);

-- =============================================================
-- SECAO 3 - ESPECIALIZACOES DE USUARIO
--
-- O papel do usuario NAO e uma coluna em Usuario: e a presenca de linha em
-- uma destas tres tabelas. A diferenca importa -- cada papel pode ganhar
-- atributos proprios sem poluir Usuario, e o login resolve o papel
-- consultando qual especializacao existe, nunca confiando no cliente.
-- =============================================================

CREATE TABLE ADM (
    ID_ADM     INT IDENTITY(1,1) NOT NULL,
    ID_Usuario INT NOT NULL,
    CONSTRAINT PK_ADM PRIMARY KEY (ID_ADM),
    CONSTRAINT FK_ADM_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario (ID_Usuario)
);

CREATE TABLE Tutor (
    ID_Tutor   INT IDENTITY(1,1) NOT NULL,
    ID_Usuario INT NOT NULL,
    CONSTRAINT PK_Tutor PRIMARY KEY (ID_Tutor),
    CONSTRAINT FK_Tutor_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario (ID_Usuario)
);

CREATE TABLE Estudante (
    ID_Estudante INT IDENTITY(1,1) NOT NULL,
    ID_Usuario   INT NOT NULL,
    CONSTRAINT PK_Estudante PRIMARY KEY (ID_Estudante),
    CONSTRAINT FK_Estudante_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario (ID_Usuario)
);

-- =============================================================
-- SECAO 4 - CURSOS E MATERIAIS
-- =============================================================

CREATE TABLE Curso (
    ID_Curso        INT IDENTITY(1,1) NOT NULL,
    Nome_Curso      VARCHAR(150) NOT NULL,
    Descricao_Curso VARCHAR(500),                -- 02
    Categoria_Curso VARCHAR(50),                 -- 02
    Nivel_Curso     VARCHAR(30),                 -- 02
    Duracao_Horas   INT,                         -- 02
    ID_Tutor_Criou  INT,
    ID_ADM_Avaliou  INT,
    ID_Prova        INT,
    -- Rascunho -> Pendente -> Publicado, com Rejeitado -> Pendente.
    -- Publicado e final: curso com aluno matriculado nao volta atras sem
    -- invalidar historico. A maquina de estados esta em
    -- FluxoPublicacaoCurso, no projeto de dominio.
    Status_Curso    VARCHAR(30) DEFAULT 'Rascunho',
    CONSTRAINT PK_Curso PRIMARY KEY (ID_Curso),
    CONSTRAINT FK_Curso_Tutor FOREIGN KEY (ID_Tutor_Criou) REFERENCES Tutor (ID_Tutor),
    CONSTRAINT FK_Curso_ADM   FOREIGN KEY (ID_ADM_Avaliou) REFERENCES ADM (ID_ADM),
    CONSTRAINT FK_Curso_Prova FOREIGN KEY (ID_Prova)       REFERENCES Prova (ID_Prova)
);

CREATE TABLE Material (
    ID_Material     INT IDENTITY(1,1) NOT NULL,
    ID_Curso        INT NOT NULL,
    Titulo_Material VARCHAR(150),
    -- 'texto' ou 'exercicio'. Nao existe 'pdf' nem 'video': a plataforma nao
    -- armazena arquivo nem tem player, e anunciar um tipo que o sistema nao
    -- entrega e promessa vazia.
    Tipo_Material   VARCHAR(50),
    -- 05: o texto que o aluno le. Sem esta coluna a tela de material so
    -- podia exibir conteudo escrito no HTML, igual para todos.
    -- NVARCHAR por causa da acentuacao e dos trechos de codigo.
    Conteudo        NVARCHAR(MAX),
    CONSTRAINT PK_Material PRIMARY KEY (ID_Material),
    CONSTRAINT FK_Material_Curso FOREIGN KEY (ID_Curso) REFERENCES Curso (ID_Curso)
);

-- =============================================================
-- SECAO 5 - QUESTOES
-- =============================================================

CREATE TABLE Questao (
    ID_Questao     INT IDENTITY(1,1) NOT NULL,
    ID_Prova       INT NOT NULL,
    Enunciado      VARCHAR(MAX) NOT NULL,
    Codigo_Exemplo VARCHAR(MAX),                 -- 02: bloco de codigo da questao
    Ordem_Questao  INT NOT NULL DEFAULT 1,       -- 02
    CONSTRAINT PK_Questao PRIMARY KEY (ID_Questao),
    CONSTRAINT FK_Questao_Prova FOREIGN KEY (ID_Prova) REFERENCES Prova (ID_Prova)
);

CREATE TABLE Alternativas (
    ID_Alternativa    INT IDENTITY(1,1) NOT NULL,
    ID_Questao        INT NOT NULL,
    Letra_Alternativa CHAR(1),                   -- 02
    Texto_Alternativa VARCHAR(500),
    -- Gabarito. Fica no banco, mas NUNCA e projetado no DTO que o estudante
    -- recebe -- e isso que torna a correcao no servidor significativa.
    Eh_Correta        BIT DEFAULT 0,
    CONSTRAINT PK_Alternativa PRIMARY KEY (ID_Alternativa),
    CONSTRAINT FK_Alternativa_Questao FOREIGN KEY (ID_Questao) REFERENCES Questao (ID_Questao)
);

-- =============================================================
-- SECAO 6 - INTERACOES E REGISTROS
-- =============================================================

CREATE TABLE Chamado (
    ID_Chamado      INT IDENTITY(1,1) NOT NULL,
    ID_Remetente    INT NOT NULL,
    -- NULL de proposito: chamado tecnico nasce SEM destinatario, para
    -- qualquer administrador poder assumir. Travar num especifico deixaria
    -- o chamado parado se ele estivesse ausente.
    ID_Destinatario INT,
    -- 04: a duvida educacional e sempre SOBRE alguma coisa. Sem o curso, o
    -- tutor recebe pergunta solta e o roteamento automatico nao funciona.
    -- NULL porque chamado tecnico nao tem curso.
    ID_Curso        INT,
    Tipo_Chamado    VARCHAR(20),
    Assunto         VARCHAR(100),
    Descricao       VARCHAR(MAX),
    Data_Abertura   DATETIME DEFAULT GETDATE(),
    Status_Chamado  VARCHAR(20) DEFAULT 'Aberto',
    CONSTRAINT PK_Chamado PRIMARY KEY (ID_Chamado),
    CONSTRAINT FK_Chamado_Remetente    FOREIGN KEY (ID_Remetente)    REFERENCES Usuario (ID_Usuario),
    CONSTRAINT FK_Chamado_Destinatario FOREIGN KEY (ID_Destinatario) REFERENCES Usuario (ID_Usuario),
    CONSTRAINT FK_Chamado_Curso        FOREIGN KEY (ID_Curso)        REFERENCES Curso (ID_Curso)
);

CREATE TABLE Log_Auditoria (
    ID_Log         INT IDENTITY(1,1) NOT NULL,
    ID_ADM         INT NOT NULL,
    Acao_Realizada VARCHAR(255),
    Data_Acao      DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Log_Auditoria PRIMARY KEY (ID_Log),
    CONSTRAINT FK_Log_ADM FOREIGN KEY (ID_ADM) REFERENCES ADM (ID_ADM)
);

CREATE TABLE Desempenho (
    ID_Desempenho   INT IDENTITY(1,1) NOT NULL,
    ID_Estudante    INT NOT NULL,
    ID_Prova        INT NOT NULL,
    Nota            DECIMAL(5,2),
    Data_Realizacao DATETIME DEFAULT GETDATE(),
    -- Uma LINHA por tentativa, e o numero dela nesta coluna. Nao existe
    -- "nota final do curso" em lugar nenhum: a nota e sempre de uma
    -- tentativa de uma prova.
    Tentativas      INT DEFAULT 1,
    CONSTRAINT PK_Desempenho PRIMARY KEY (ID_Desempenho),
    CONSTRAINT FK_Desempenho_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Desempenho_Prova     FOREIGN KEY (ID_Prova)     REFERENCES Prova (ID_Prova)
);

CREATE TABLE Historico (
    ID_Historico     INT IDENTITY(1,1) NOT NULL,
    ID_Estudante     INT NOT NULL,
    ID_Curso         INT NOT NULL,
    Status_Conclusao VARCHAR(20),
    Data_Conclusao   DATE,
    CONSTRAINT PK_Historico PRIMARY KEY (ID_Historico),
    CONSTRAINT FK_Historico_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Historico_Curso     FOREIGN KEY (ID_Curso)     REFERENCES Curso (ID_Curso)
);

CREATE TABLE Progresso (
    ID_Progresso          INT IDENTITY(1,1) NOT NULL,
    ID_Estudante          INT NOT NULL,
    ID_Material           INT NOT NULL,
    Status_Conclusao      BIT DEFAULT 0,
    Data_Visualizacao     DATETIME DEFAULT GETDATE(),
    Porcentagem_Assistida INT DEFAULT 0,
    CONSTRAINT PK_Progresso PRIMARY KEY (ID_Progresso),
    CONSTRAINT FK_Progresso_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Progresso_Material  FOREIGN KEY (ID_Material)  REFERENCES Material (ID_Material)
);

CREATE TABLE Conquista (
    ID_Conquista   INT IDENTITY(1,1) NOT NULL,
    ID_Estudante   INT NOT NULL,
    ID_Medalha     INT NOT NULL,
    Data_Conquista DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Conquista PRIMARY KEY (ID_Conquista),
    CONSTRAINT FK_Conquista_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Conquista_Medalha   FOREIGN KEY (ID_Medalha)   REFERENCES Medalha (ID_Medalha)
);

CREATE TABLE Certificado (
    ID_Certificado      INT IDENTITY(1,1) NOT NULL,
    ID_Historico        INT NOT NULL,
    -- Gerado pelo BANCO, nao pela aplicacao. Dois geradores para o mesmo
    -- identificador sao duas fontes da verdade. E a chave publica do
    -- certificado: e por ela que a validacao externa acontece.
    Codigo_Autenticacao UNIQUEIDENTIFIER DEFAULT NEWID(),
    Data_Emissao        DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Certificado PRIMARY KEY (ID_Certificado),
    CONSTRAINT FK_Certificado_Historico FOREIGN KEY (ID_Historico) REFERENCES Historico (ID_Historico)
);
GO

-- =============================================================
-- SECAO 7 - INDICES
-- Fora das chaves primarias e estrangeiras, so o que uma consulta real usa.
-- =============================================================

-- 04: "chamados deste curso", no painel do tutor.
CREATE INDEX IX_Chamado_Curso ON Chamado (ID_Curso);
GO

-- =============================================================
-- SECAO 8 - TRIGGER DE AUDITORIA
--
-- Registra em Log_Auditoria toda mudanca de status de usuario.
--
-- LIMITACAO CONHECIDA, e vale declarar em vez de esconder: o ID_ADM e
-- fixo em 1. A trigger nao tem como saber QUAL administrador fez a
-- alteracao, porque a conexao com o banco e unica para toda a aplicacao --
-- quem sabe o usuario autenticado e a API, pelo token. A solucao seria
-- SESSION_CONTEXT, com a API gravando o id do administrador na sessao
-- antes do UPDATE. Ficou como melhoria mapeada.
--
-- ATENCAO PARA O EF CORE: tabela com trigger exige HasTrigger() no
-- mapeamento. A partir da versao 7, o EF usa clausula OUTPUT nos comandos
-- de escrita, e o SQL Server nao aceita OUTPUT em tabela com trigger. Sem
-- essa declaracao, todo UPDATE em Usuario falha em tempo de execucao.
-- Ver TechQuestDbContext, mapeamento de Usuario.
-- =============================================================
CREATE TRIGGER TRG_Auditoria_StatusUsuario
ON Usuario
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(Status_Usuario)
    BEGIN
        INSERT INTO Log_Auditoria (ID_ADM, Acao_Realizada, Data_Acao)
        SELECT
            1,
            CONCAT('Status do Usuario ID ', i.ID_Usuario, ' alterado para: ', i.Status_Usuario),
            GETDATE()
        FROM inserted i;
    END
END;
GO

-- =============================================================
-- SECAO 9 - PROCEDURE DE RELATORIO
--
-- Historico de provas do estudante. A API a chama por FromSqlInterpolated e
-- recebe o resultado num tipo sem chave ([Keyless]) -- ela devolve um
-- relatorio, nao linhas de uma tabela, e nao faz sentido o EF rastrear.
-- =============================================================
CREATE PROCEDURE SP_RelatorioDesempenhoEstudante
    @ID_Estudante INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Nome_Usuario    AS Estudante,
        p.Titulo_Prova    AS Prova,
        d.Nota,
        d.Tentativas,
        d.Data_Realizacao
    FROM Desempenho d
    INNER JOIN Estudante e ON d.ID_Estudante = e.ID_Estudante
    INNER JOIN Usuario   u ON e.ID_Usuario   = u.ID_Usuario
    INNER JOIN Prova     p ON d.ID_Prova     = p.ID_Prova
    WHERE d.ID_Estudante = @ID_Estudante
    ORDER BY d.Data_Realizacao DESC;
END;
GO

-- =============================================================
-- CONFERENCIA
--
-- Compara o banco criado com o que a aplicacao espera. A lista de colunas
-- veio do mapeamento do TechQuestDbContext: se algo aparecer como FALTANDO,
-- a API quebra na primeira consulta aquela entidade.
--
-- Esperado: nenhuma linha em "faltando", 17 tabelas, 1 trigger, 1 procedure.
-- =============================================================
WITH esperado(Tabela, Coluna) AS (
    SELECT * FROM (VALUES
      ('Usuario','ID_Usuario'),('Usuario','Nome_Usuario'),('Usuario','Email_Usuario'),
      ('Usuario','Senha_Usuario'),('Usuario','Data_Cadastro'),('Usuario','Status_Usuario'),
      ('Usuario','Telefone_Usuario'),('Usuario','Data_Nascimento'),('Usuario','Cidade_Usuario'),
      ('ADM','ID_ADM'),('ADM','ID_Usuario'),
      ('Tutor','ID_Tutor'),('Tutor','ID_Usuario'),
      ('Estudante','ID_Estudante'),('Estudante','ID_Usuario'),
      ('Curso','ID_Curso'),('Curso','Nome_Curso'),('Curso','Descricao_Curso'),
      ('Curso','Categoria_Curso'),('Curso','Nivel_Curso'),('Curso','Duracao_Horas'),
      ('Curso','Status_Curso'),('Curso','ID_Tutor_Criou'),('Curso','ID_ADM_Avaliou'),
      ('Curso','ID_Prova'),
      ('Material','ID_Material'),('Material','ID_Curso'),('Material','Titulo_Material'),
      ('Material','Tipo_Material'),('Material','Conteudo'),
      ('Prova','ID_Prova'),('Prova','Titulo_Prova'),('Prova','Nota_Minima'),
      ('Prova','Tempo_Minutos'),
      ('Questao','ID_Questao'),('Questao','ID_Prova'),('Questao','Enunciado'),
      ('Questao','Codigo_Exemplo'),('Questao','Ordem_Questao'),
      ('Alternativas','ID_Alternativa'),('Alternativas','ID_Questao'),
      ('Alternativas','Letra_Alternativa'),('Alternativas','Texto_Alternativa'),
      ('Alternativas','Eh_Correta'),
      ('Desempenho','ID_Desempenho'),('Desempenho','ID_Estudante'),('Desempenho','ID_Prova'),
      ('Desempenho','Nota'),('Desempenho','Data_Realizacao'),('Desempenho','Tentativas'),
      ('Progresso','ID_Progresso'),('Progresso','ID_Estudante'),('Progresso','ID_Material'),
      ('Progresso','Status_Conclusao'),('Progresso','Data_Visualizacao'),
      ('Progresso','Porcentagem_Assistida'),
      ('Medalha','ID_Medalha'),('Medalha','Nome_Medalha'),('Medalha','Raridade'),
      ('Medalha','Descricao_Medalha'),
      ('Conquista','ID_Conquista'),('Conquista','ID_Estudante'),('Conquista','ID_Medalha'),
      ('Conquista','Data_Conquista'),
      ('Historico','ID_Historico'),('Historico','ID_Estudante'),('Historico','ID_Curso'),
      ('Historico','Status_Conclusao'),('Historico','Data_Conclusao'),
      ('Certificado','ID_Certificado'),('Certificado','ID_Historico'),
      ('Certificado','Codigo_Autenticacao'),('Certificado','Data_Emissao'),
      ('Chamado','ID_Chamado'),('Chamado','ID_Remetente'),('Chamado','ID_Destinatario'),
      ('Chamado','ID_Curso'),('Chamado','Tipo_Chamado'),('Chamado','Assunto'),
      ('Chamado','Descricao'),('Chamado','Data_Abertura'),('Chamado','Status_Chamado'),
      ('Log_Auditoria','ID_Log'),('Log_Auditoria','ID_ADM'),
      ('Log_Auditoria','Acao_Realizada'),('Log_Auditoria','Data_Acao')
    ) AS v(Tabela, Coluna)
)
SELECT  e.Tabela, e.Coluna, 'FALTANDO' AS Situacao
FROM    esperado e
WHERE   COL_LENGTH(e.Tabela, e.Coluna) IS NULL
ORDER BY e.Tabela, e.Coluna;

SELECT (SELECT COUNT(*) FROM sys.tables)                            AS Tabelas,
       (SELECT COUNT(*) FROM sys.triggers WHERE is_ms_shipped = 0)  AS Triggers,
       (SELECT COUNT(*) FROM sys.procedures)                        AS Procedures,
       (SELECT COUNT(*) FROM sys.foreign_keys)                      AS Chaves_Estrangeiras;
