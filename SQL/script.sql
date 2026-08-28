-- 1. CRIACAO DO BANCO DE DADOS
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TechQuestDB')
BEGIN
    CREATE DATABASE TechQuestDB;
END
GO

USE TechQuestDB;
GO

-- 2. TABELAS INDEPENDENTES (SEM FKs) OU TABELAS PAI

CREATE TABLE Usuario (
    ID_Usuario INT IDENTITY(1,1) NOT NULL,
    Nome_Usuario VARCHAR(100) NOT NULL,
    Email_Usuario VARCHAR(100) UNIQUE,
    Senha_Usuario VARCHAR(255), 
    Data_Cadastro DATETIME DEFAULT GETDATE(),
    Status_Usuario BIT DEFAULT 1, -- 1 para Ativo, 0 para Inativo
    CONSTRAINT PK_Usuario PRIMARY KEY (ID_Usuario)
);

CREATE TABLE Medalha (
    ID_Medalha INT IDENTITY(1,1) NOT NULL,
    Nome_Medalha VARCHAR(50) NOT NULL,
    Raridade VARCHAR(20),
    CONSTRAINT PK_Medalha PRIMARY KEY (ID_Medalha)
);

CREATE TABLE Prova (
    ID_Prova INT IDENTITY(1,1) NOT NULL,
    Titulo_Prova VARCHAR(100),
    CONSTRAINT PK_Prova PRIMARY KEY (ID_Prova)
);

-- 3. ESPECIALIZACOES DE USUARIO

CREATE TABLE ADM (
    ID_ADM INT IDENTITY(1,1) NOT NULL,
    ID_Usuario INT NOT NULL,
    CONSTRAINT PK_ADM PRIMARY KEY (ID_ADM),
    CONSTRAINT FK_ADM_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario (ID_Usuario)
);

CREATE TABLE Tutor (
    ID_Tutor INT IDENTITY(1,1) NOT NULL,
    ID_Usuario INT NOT NULL,
    CONSTRAINT PK_Tutor PRIMARY KEY (ID_Tutor),
    CONSTRAINT FK_Tutor_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario (ID_Usuario)
);

CREATE TABLE Estudante (
    ID_Estudante INT IDENTITY(1,1) NOT NULL,
    ID_Usuario INT NOT NULL,
    CONSTRAINT PK_Estudante PRIMARY KEY (ID_Estudante),
    CONSTRAINT FK_Estudante_Usuario FOREIGN KEY (ID_Usuario) REFERENCES Usuario (ID_Usuario)
);

-- 4. ESTRUTURA DE CURSOS E MATERIAIS

CREATE TABLE Curso (
    ID_Curso INT IDENTITY(1,1) NOT NULL,
    Nome_Curso VARCHAR(150) NOT NULL,
    ID_Tutor_Criou INT,
    ID_ADM_Avaliou INT,
    ID_Prova INT,
    Status_Curso VARCHAR(30) DEFAULT 'Rascunho',
    CONSTRAINT PK_Curso PRIMARY KEY (ID_Curso),
    CONSTRAINT FK_Curso_Tutor FOREIGN KEY (ID_Tutor_Criou) REFERENCES Tutor (ID_Tutor),
    CONSTRAINT FK_Curso_ADM FOREIGN KEY (ID_ADM_Avaliou) REFERENCES ADM (ID_ADM),
    CONSTRAINT FK_Curso_Prova FOREIGN KEY (ID_Prova) REFERENCES Prova (ID_Prova)
);

CREATE TABLE Material (
    ID_Material INT IDENTITY(1,1) NOT NULL,
    ID_Curso INT NOT NULL,
    Titulo_Material VARCHAR(150),
    Tipo_Material VARCHAR(50),
    CONSTRAINT PK_Material PRIMARY KEY (ID_Material),
    CONSTRAINT FK_Material_Curso FOREIGN KEY (ID_Curso) REFERENCES Curso (ID_Curso)
);

-- 5. PROVAS E QUESTOES

CREATE TABLE Questao (
    ID_Questao INT IDENTITY(1,1) NOT NULL,
    ID_Prova INT NOT NULL,
    Enunciado VARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_Questao PRIMARY KEY (ID_Questao),
    CONSTRAINT FK_Questao_Prova FOREIGN KEY (ID_Prova) REFERENCES Prova (ID_Prova)
);

CREATE TABLE Alternativas (
    ID_Alternativa INT IDENTITY(1,1) NOT NULL,
    ID_Questao INT NOT NULL,
    Texto_Alternativa VARCHAR(500),
    Eh_Correta BIT DEFAULT 0,
    CONSTRAINT PK_Alternativa PRIMARY KEY (ID_Alternativa),
    CONSTRAINT FK_Alternativa_Questao FOREIGN KEY (ID_Questao) REFERENCES Questao (ID_Questao)
);

-- 6. INTERACOES E REGISTROS (CHAMADOS, LOGS, DESEMPENHO)

CREATE TABLE Chamado (
    ID_Chamado INT IDENTITY(1,1) NOT NULL,
    ID_Remetente INT NOT NULL,
    ID_Destinatario INT,
    Tipo_Chamado VARCHAR(20),
    Assunto VARCHAR(100),
    Descricao VARCHAR(MAX),
    Data_Abertura DATETIME DEFAULT GETDATE(),
    Status_Chamado VARCHAR(20) DEFAULT 'Aberto',
    CONSTRAINT PK_Chamado PRIMARY KEY (ID_Chamado),
    CONSTRAINT FK_Chamado_Remetente FOREIGN KEY (ID_Remetente) REFERENCES Usuario (ID_Usuario),
    CONSTRAINT FK_Chamado_Destinatario FOREIGN KEY (ID_Destinatario) REFERENCES Usuario (ID_Usuario)
);

CREATE TABLE Log_Auditoria (
    ID_Log INT IDENTITY(1,1) NOT NULL,
    ID_ADM INT NOT NULL,
    Acao_Realizada VARCHAR(255),
    Data_Acao DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Log_Auditoria PRIMARY KEY (ID_Log),
    CONSTRAINT FK_Log_ADM FOREIGN KEY (ID_ADM) REFERENCES ADM (ID_ADM)
);

CREATE TABLE Desempenho (
    ID_Desempenho INT IDENTITY(1,1) NOT NULL,
    ID_Estudante INT NOT NULL,
    ID_Prova INT NOT NULL,
    Nota DECIMAL(5,2),
    Data_Realizacao DATETIME DEFAULT GETDATE(),
    Tentativas INT DEFAULT 1,
    CONSTRAINT PK_Desempenho PRIMARY KEY (ID_Desempenho),
    CONSTRAINT FK_Desempenho_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Desempenho_Prova FOREIGN KEY (ID_Prova) REFERENCES Prova (ID_Prova)
);

CREATE TABLE Historico (
    ID_Historico INT IDENTITY(1,1) NOT NULL,
    ID_Estudante INT NOT NULL,
    ID_Curso INT NOT NULL,
    Status_Conclusao VARCHAR(20),
    Data_Conclusao DATE,
    CONSTRAINT PK_Historico PRIMARY KEY (ID_Historico),
    CONSTRAINT FK_Historico_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Historico_Curso FOREIGN KEY (ID_Curso) REFERENCES Curso (ID_Curso)
);

CREATE TABLE Progresso (
    ID_Progresso INT IDENTITY(1,1) NOT NULL,
    ID_Estudante INT NOT NULL,
    ID_Material INT NOT NULL,
    Status_Conclusao BIT DEFAULT 0,
    Data_Visualizacao DATETIME DEFAULT GETDATE(),
    Porcentagem_Assistida INT DEFAULT 0,
    CONSTRAINT PK_Progresso PRIMARY KEY (ID_Progresso),
    CONSTRAINT FK_Progresso_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Progresso_Material FOREIGN KEY (ID_Material) REFERENCES Material (ID_Material)
);

CREATE TABLE Conquista (
    ID_Conquista INT IDENTITY(1,1) NOT NULL,
    ID_Estudante INT NOT NULL,
    ID_Medalha INT NOT NULL,
    Data_Conquista DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Conquista PRIMARY KEY (ID_Conquista),
    CONSTRAINT FK_Conquista_Estudante FOREIGN KEY (ID_Estudante) REFERENCES Estudante (ID_Estudante),
    CONSTRAINT FK_Conquista_Medalha FOREIGN KEY (ID_Medalha) REFERENCES Medalha (ID_Medalha)
);

CREATE TABLE Certificado (
    ID_Certificado INT IDENTITY(1,1) NOT NULL,
    ID_Historico INT NOT NULL,
    Codigo_Autenticacao UNIQUEIDENTIFIER DEFAULT NEWID(),
    Data_Emissao DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Certificado PRIMARY KEY (ID_Certificado),
    CONSTRAINT FK_Certificado_Historico FOREIGN KEY (ID_Historico) REFERENCES Historico (ID_Historico)
);
GO

GO
-- 7. Trigger: Registrar auditoria automática quando o status do usuário for alterado
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
            1, -- ID_ADM padrão/sistema ou capturado na sessão
            CONCAT('Status do Usuario ID ', i.ID_Usuario, ' alterado para: ', i.Status_Usuario),
            GETDATE()
        FROM inserted i;
    END
END;
GO

GO
-- 8. Procedure: Gerar Relatório de Desempenho do Estudante por Prova
CREATE PROCEDURE SP_RelatorioDesempenhoEstudante
    @ID_Estudante INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.Nome_Usuario AS Estudante,
        p.Titulo_Prova AS Prova,
        d.Nota,
        d.Tentativas,
        d.Data_Realizacao
    FROM Desempenho d
    INNER JOIN Estudante e ON d.ID_Estudante = e.ID_Estudante
    INNER JOIN Usuario u ON e.ID_Usuario = u.ID_Usuario
    INNER JOIN Prova p ON d.ID_Prova = p.ID_Prova
    WHERE d.ID_Estudante = @ID_Estudante
    ORDER BY d.Data_Realizacao DESC;
END;
GO