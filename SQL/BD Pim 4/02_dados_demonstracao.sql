-- =============================================================
-- 02_dados_demonstracao.sql  |  Tech Quest - carga de demonstracao
--
-- Roda DEPOIS de 01_estrutura.sql (banco novo) ou de
-- 03_preparar_banco_existente.sql (banco que ja esta no Azure).
--
-- GERADO por script (gerar_carga.py), nao digitado. O gerador simula a
-- linha do tempo de cada estudante e aplica as MESMAS regras do back-end:
-- conclusao de curso (RegrasConclusaoCurso), aprovacao pela Nota_Minima da
-- prova, medalhas (RegrasMedalhas, avaliadas apos cada acao e depois da
-- conclusao), cursos so recebem alunos depois de publicados, prova so depois
-- de todos os materiais e chamados com o roteamento do ChamadoService.
-- XP e nivel nao aparecem aqui porque nao sao gravados: a API calcula a
-- partir de Progresso, Desempenho e Conquista (RegrasGamificacao).
--
-- VOLUME: 1 administrador, 5 tutores, 20 estudantes, 15 cursos
-- (13 publicados, 1 pendente, 1 rejeitado), 63 materiais, 150 questoes
-- (10 por prova), 25 medalhas (12 gerais + 1 por curso novo),
-- 102 matriculas, 68 certificados, 94 tentativas de prova,
-- 137 conquistas e 22 chamados.
--
-- QUESTOES DOS TOMOS: 38 das 150 questoes vem dos Tomos de questoes
-- (Material Instrucional Especifico de ADS, CQA/UNIP), com o enunciado, as
-- alternativas e o gabarito oficiais; o enunciado comeca pela origem, por
-- exemplo "(Enade 2017)". As aulas ganharam paragrafos para cobrir o que
-- essas questoes cobram (gerador/cursos_tomos.py).
--
-- TUDO OU NADA: a carga roda numa transacao. Se o banco nao estiver vazio,
-- ou se qualquer insercao falhar, nada e gravado.
--
-- IDs EXPLICITOS (IDENTITY_INSERT): as linhas se referenciam por id, entao
-- os ids sao fixos e o arquivo pode ser lido e conferido. Por isso o banco
-- precisa estar vazio e com os contadores zerados (o 03 faz isso).
--
-- CONTAS DE TESTE (mesmas senhas de antes, hashes BCrypt fator 11):
--   Carlos Mendes   admin123@gmail.com   senha: admin123@   (administrador)
--   Ana Souza       tutor123@gmail.com   senha: tutor123@   (tutora)
--   Felipe Almeida  felipe123@gmail.com  senha: aluno123@   (estudante)
-- DEMAIS USUARIOS: e-mail nome.sobrenome@techquest.dev, senha: techquest123@
--
-- ACENTUACAO: arquivo UTF-8; todos os literais usam o prefixo N.
-- =============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM Usuario) OR EXISTS (SELECT 1 FROM Curso) OR EXISTS (SELECT 1 FROM Medalha)
        THROW 50001, 'O banco nao esta vazio. Rode 03_preparar_banco_existente.sql antes desta carga.', 1;

    -- ---------------------------------------------------------
    -- 1. Usuarios e especializacoes (o papel e a linha em ADM, Tutor ou Estudante)
    -- ---------------------------------------------------------
    SET IDENTITY_INSERT Usuario ON;
    INSERT INTO Usuario (ID_Usuario, Nome_Usuario, Email_Usuario, Senha_Usuario, Data_Cadastro, Status_Usuario, Telefone_Usuario, Data_Nascimento, Cidade_Usuario) VALUES
        (1, N'Carlos Mendes', N'admin123@gmail.com', N'$2b$11$951xpD5760ZdZQ/MQF5jEuvnGVsaIAgxU6Kq7m1jM1NHHsFx4YszG', '2026-08-03T13:54:50', 1, N'(11) 94291-6397', '1987-08-17', N'Jundiaí'),
        (2, N'Ana Souza', N'tutor123@gmail.com', N'$2b$11$oMMqH6O271RgwpJ7n9NEoehkyw7qTuzczcSsSlKFBOoQctzJMCzNu', '2026-08-05T12:53:14', 1, N'(11) 91917-5466', '1993-10-28', N'Santos'),
        (3, N'Bruno Carvalho', N'bruno.carvalho@techquest.dev', N'$2b$11$dRb1br0nHY2iz2WxrDFAUePeO2EJg0/TMGvMwQQVr9u9h3garamXG', '2026-08-06T22:34:32', 1, N'(11) 91804-4925', '1989-11-04', N'Campinas'),
        (4, N'Juliana Ferreira', N'juliana.ferreira@techquest.dev', N'$2b$11$wabrWq7Ip8NE/ifQO5M74OjJsB89vMXaxNs9CQcGqhnrajdcsyMz6', '2026-08-06T20:50:59', 1, N'(11) 94640-4474', '2002-09-03', N'Santo André'),
        (5, N'Rafael Lima', N'rafael.lima@techquest.dev', N'$2b$11$gXoZZ7RvJ.Qk3wJNMtryl.dMFhntJcJki4DIqYzLIg4SDutxaIUdW', '2026-08-10T12:43:57', 1, N'(11) 98949-1005', '1987-09-04', N'Santo André'),
        (6, N'Mariana Costa', N'mariana.costa@techquest.dev', N'$2b$11$Bav4C3ydweAU0GDPpzglkORm3qsLROyByO4Ai8dgzIClHO5xhNvoC', '2026-08-12T13:50:05', 1, N'(11) 91700-1527', '1995-09-23', N'Campinas'),
        (7, N'Felipe Almeida', N'felipe123@gmail.com', N'$2b$11$hJGB7As2N0z0dNAyqzta.errVRKmhjkRo5I96pyLTJQn5YjCFiE7C', '2026-08-20T15:06:36', 1, N'(11) 91948-2296', '1999-09-08', N'Santo André'),
        (8, N'Beatriz Nogueira', N'beatriz.nogueira@techquest.dev', N'$2b$11$bkybz3CHc9hBu2GJ9yxc0u08BjNywmB4yL34zprsmcao4MNWiJLzC', '2026-08-25T18:31:32', 1, N'(11) 98056-9505', '1991-01-24', N'Santo André'),
        (9, N'Lucas Pereira', N'lucas.pereira@techquest.dev', N'$2b$11$8ulQoc8hMg1tcrJgBJyb.ejJXtNi7fF4J3CnVP6XytbvhcRR8J/Vu', '2026-08-27T18:36:43', 1, N'(11) 94273-4236', '1985-03-17', N'Campinas'),
        (10, N'Gabriela Rocha', N'gabriela.rocha@techquest.dev', N'$2b$11$LnTlV3qpcdtodD5ba6d.Ju0W9Er0qSBkP7FKqrNDogJvHQ0ubZz6m', '2026-08-28T15:25:57', 1, N'(11) 94291-1617', '1985-06-12', N'Santo André'),
        (11, N'Pedro Henrique Santos', N'pedro.santos@techquest.dev', N'$2b$11$I0fik12fWl.q8y18dxQWHOHZI4bhnq.Yz0JvQ6hp68n4UaxWGvQWa', '2026-08-30T18:13:49', 1, N'(11) 92896-2248', '2004-04-27', N'Santos'),
        (12, N'Larissa Oliveira', N'larissa.oliveira@techquest.dev', N'$2b$11$k2D1hCvLU6nEsB/WsccwpuUoMXQK0YMixelHumWNMlcCtIuWiQYfi', '2026-09-01T14:32:39', 1, N'(11) 92309-9578', '2006-06-19', N'Piracicaba'),
        (13, N'Matheus Ribeiro', N'matheus.ribeiro@techquest.dev', N'$2b$11$tyNBij.fR72wqM4we6KfQ.sjY/mqh0PWkTih8kPGnmrXnZoW140H2', '2026-09-02T14:26:32', 1, N'(11) 97545-3887', '2004-08-15', N'Sorocaba'),
        (14, N'Camila Duarte', N'camila.duarte@techquest.dev', N'$2b$11$XgiN8UpbrZNAjjUywZlkoe8OeaJ4OT18ZluqJ19umAgLrYlw2Zu/G', '2026-09-03T19:16:41', 1, N'(11) 97227-8692', '1995-02-01', N'Sorocaba'),
        (15, N'Gustavo Teixeira', N'gustavo.teixeira@techquest.dev', N'$2b$11$P8JYAW8j5SmL.FXzfe4f9OjbnrB7twdjZaJGTK7YscC9ukkGJy10q', '2026-09-05T14:02:32', 1, N'(11) 96252-3193', '1993-07-02', N'Osasco'),
        (16, N'Isabela Martins', N'isabela.martins@techquest.dev', N'$2b$11$Jg0XWtBcd0js7mwcHECbXOmr.hTNNHvjsgF5IUILsCrhxlRVJ3V3K', '2026-09-06T19:25:34', 1, N'(11) 97672-7191', '2001-04-26', N'São José dos Campos'),
        (17, N'Rodrigo Barbosa', N'rodrigo.barbosa@techquest.dev', N'$2b$11$iITNLcoz7Hg8VWAPLNLbROhlKPW8UzFFfFnaeImE611/GxbZNMwLC', '2026-09-08T12:35:28', 1, N'(11) 94671-3537', '1989-02-14', N'Sorocaba'),
        (18, N'Fernanda Lopes', N'fernanda.lopes@techquest.dev', N'$2b$11$TrIMJg.pZh9DtV9MDv8IrOwfw2bnwGJIRpK/d56qxsYYEQZOmu4O6', '2026-09-09T13:10:50', 1, N'(11) 91513-1795', '2004-12-13', N'Santos'),
        (19, N'Vinícius Araújo', N'vinicius.araujo@techquest.dev', N'$2b$11$duFKpOUdP2rqOb/fgIhwne/65jTGnm54K1t9MYT67fBqK81GH1LG2', '2026-09-11T15:39:47', 1, N'(11) 95120-7257', '1993-07-11', N'Jundiaí'),
        (20, N'Juliana Campos', N'juliana.campos@techquest.dev', N'$2b$11$LmzWduxLX3mPgyhBTGnfCO2.AGXyaHhcvlbAlbx7oC4twVt35/QOe', '2026-09-13T20:24:47', 1, N'(11) 94303-1923', '2003-03-19', N'Piracicaba'),
        (21, N'Thiago Moreira', N'thiago.moreira@techquest.dev', N'$2b$11$PqrxI1NEmv2nT4xCJjgApuvlV6FnJ1/v7frTjwqXKr4fd3dGAzmCi', '2026-09-15T20:20:39', 1, N'(11) 95801-1641', '2006-09-11', N'Santo André'),
        (22, N'Amanda Correia', N'amanda.correia@techquest.dev', N'$2b$11$lUh7Pk4wSc.02y1hrpPRROAzYI8m0ht7YMpaRz8qvdr9UDN19tpRy', '2026-09-18T11:29:44', 1, N'(11) 94326-4432', '1989-04-23', N'Piracicaba'),
        (23, N'Daniel Freitas', N'daniel.freitas@techquest.dev', N'$2b$11$lkw/snoriL8MBF25kEEB1uaTZShqVUgxew6KzdlAlT71wMqEAM1EC', '2026-09-22T22:37:22', 1, N'(11) 97472-1885', '1989-05-09', N'São José dos Campos'),
        (24, N'Letícia Cardoso', N'leticia.cardoso@techquest.dev', N'$2b$11$9J5f/3Bg5i0J7QDVuWSUIeFk9p4VgUHqVQ4WpkKMd6QKBSR0CSiJq', '2026-09-26T15:28:03', 1, N'(11) 95213-9873', '1985-04-05', N'Santo André'),
        (25, N'Eduardo Pinto', N'eduardo.pinto@techquest.dev', N'$2b$11$FiXZbt1X0KhfgDsIdl3GSO94EDriHzesN0Fcls4wHlxkgh7lwZlY.', '2026-10-01T19:53:39', 1, N'(11) 91896-1552', '2003-12-03', N'São Paulo'),
        (26, N'Renata Vieira', N'renata.vieira@techquest.dev', N'$2b$11$PJ9OpSgoDcD/XlTzB7772OCyuyvlnQmxVYLY4t/X2WAvO7.hqZ97W', '2026-09-04T21:48:38', 1, N'(11) 99573-5461', '1993-03-04', N'Guarulhos');
    SET IDENTITY_INSERT Usuario OFF;

    SET IDENTITY_INSERT ADM ON;
    INSERT INTO ADM (ID_ADM, ID_Usuario) VALUES
        (1, 1);
    SET IDENTITY_INSERT ADM OFF;

    SET IDENTITY_INSERT Tutor ON;
    INSERT INTO Tutor (ID_Tutor, ID_Usuario) VALUES
        (1, 2),
        (2, 3),
        (3, 4),
        (4, 5),
        (5, 6);
    SET IDENTITY_INSERT Tutor OFF;

    SET IDENTITY_INSERT Estudante ON;
    INSERT INTO Estudante (ID_Estudante, ID_Usuario) VALUES
        (1, 7),
        (2, 8),
        (3, 9),
        (4, 10),
        (5, 11),
        (6, 12),
        (7, 13),
        (8, 14),
        (9, 15),
        (10, 16),
        (11, 17),
        (12, 18),
        (13, 19),
        (14, 20),
        (15, 21),
        (16, 22),
        (17, 23),
        (18, 24),
        (19, 25),
        (20, 26);
    SET IDENTITY_INSERT Estudante OFF;

    -- ---------------------------------------------------------
    -- 2. Provas (antes do curso: a FK esta em Curso.ID_Prova) e cursos
    -- ---------------------------------------------------------
    SET IDENTITY_INSERT Prova ON;
    INSERT INTO Prova (ID_Prova, Titulo_Prova, Nota_Minima, Tempo_Minutos) VALUES
        (1, N'Prova: Arrays e Listas em C#', 7.00, 30),
        (2, N'Prova: Análise de Sistemas e Banco de Dados', 7.00, 30),
        (3, N'Prova: Lógica de Programação com Python', 7.00, 30),
        (4, N'Prova: HTML e CSS Responsivo', 7.00, 30),
        (5, N'Prova: Git e GitHub na Prática', 7.00, 30),
        (6, N'Prova: Engenharia de Requisitos e UML', 7.00, 30),
        (7, N'Prova: Redes de Computadores', 7.00, 30),
        (8, N'Prova: Orientação a Objetos com Java', 7.00, 30),
        (9, N'Prova: JavaScript Moderno', 7.00, 30),
        (10, N'Prova: Testes Automatizados', 7.00, 30),
        (11, N'Prova: Fundamentos de Cloud Computing', 7.00, 30),
        (12, N'Prova: Segurança da Informação', 7.00, 30),
        (13, N'Prova: Estruturas de Dados e Algoritmos', 6.00, 30),
        (14, N'Prova: React para Iniciantes', 7.00, 30),
        (15, N'Prova: SQL Avançado e Otimização de Consultas', 6.00, 30);
    SET IDENTITY_INSERT Prova OFF;

    SET IDENTITY_INSERT Curso ON;
    INSERT INTO Curso (ID_Curso, Nome_Curso, Descricao_Curso, Categoria_Curso, Nivel_Curso, Duracao_Horas, ID_Tutor_Criou, ID_ADM_Avaliou, ID_Prova, Status_Curso) VALUES
        (1, N'Fundamentos de C#', N'Domine as bases da linguagem mais poderosa da Microsoft. Do zero ao seu primeiro software profissional com orientacao a objetos e boas praticas.', N'Programacao', N'Iniciante', 12, 1, 1, 1, N'Publicado'),
        (2, N'Banco de Dados e Modelagem de Sistemas', N'Domine modelagem de dados, SQL e os fundamentos de analise de sistemas, RUP e tecnicas de elicitacao de requisitos.', N'Dados', N'Intermediario', 18, 1, 1, 2, N'Publicado'),
        (3, N'Lógica de Programação com Python', N'Aprenda a pensar como programador: variáveis, decisões, repetições e funções, com Python como ferramenta para transformar problemas em algoritmos.', N'Programacao', N'Iniciante', 10, 2, 1, 3, N'Publicado'),
        (4, N'HTML e CSS Responsivo', N'Estruture páginas com HTML semântico e crie layouts que se adaptam do celular ao desktop com CSS, Flexbox, Grid e media queries.', N'Web', N'Iniciante', 12, 3, 1, 4, N'Publicado'),
        (5, N'Git e GitHub na Prática', N'Controle de versão do zero: commits, branches, merge, resolução de conflitos e colaboração com pull requests no GitHub.', N'Engenharia de Software', N'Iniciante', 8, 4, 1, 5, N'Publicado'),
        (6, N'Engenharia de Requisitos e UML', N'Levante e documente requisitos e represente sistemas com os principais diagramas UML: casos de uso, classes e sequência.', N'Engenharia de Software', N'Iniciante', 10, 4, 1, 6, N'Publicado'),
        (7, N'Redes de Computadores', N'Entenda como os dados viajam: modelo TCP/IP, endereçamento IP, DNS, HTTP e os equipamentos que conectam redes locais à internet.', N'Infraestrutura', N'Iniciante', 12, 5, 1, 7, N'Publicado'),
        (8, N'Orientação a Objetos com Java', N'Classes, objetos, encapsulamento, herança, polimorfismo e interfaces na prática, com Java e exemplos de sistemas reais.', N'Programacao', N'Intermediario', 16, 2, 1, 8, N'Publicado'),
        (9, N'JavaScript Moderno', N'Do básico ao assíncrono: tipos, funções, manipulação do DOM, arrays, Promises e consumo de APIs com fetch e async/await.', N'Web', N'Intermediario', 16, 3, 1, 9, N'Publicado'),
        (10, N'Testes Automatizados', N'Escreva testes de unidade e de integração, entenda a pirâmide de testes e use dublês para isolar dependências e evitar regressões.', N'Engenharia de Software', N'Intermediario', 12, 4, 1, 10, N'Publicado'),
        (11, N'Fundamentos de Cloud Computing', N'Modelos de serviço e de implantação, elasticidade, custos e os serviços essenciais de computação, armazenamento e banco de dados na nuvem.', N'Infraestrutura', N'Intermediario', 12, 5, 1, 11, N'Publicado'),
        (12, N'Segurança da Informação', N'Pilares da segurança, autenticação, criptografia, as vulnerabilidades mais comuns em aplicações web e boas práticas de desenvolvimento seguro.', N'Seguranca', N'Intermediario', 14, 5, 1, 12, N'Publicado'),
        (13, N'Estruturas de Dados e Algoritmos', N'Pilhas, filas, listas encadeadas, árvores, ordenação e busca, com análise de complexidade para escolher a estrutura certa para cada problema.', N'Programacao', N'Avancado', 24, 2, 1, 13, N'Publicado'),
        (14, N'React para Iniciantes', N'Componentes, props, estado e efeitos: construa interfaces reativas com React e entenda como os dados fluem em uma aplicação de página única.', N'Web', N'Intermediario', 14, 3, NULL, 14, N'Pendente'),
        (15, N'SQL Avançado e Otimização de Consultas', N'JOINs, subconsultas, funções de janela, índices e plano de execução para escrever consultas corretas e rápidas no SQL Server.', N'Dados', N'Avancado', 20, 1, 1, 15, N'Rejeitado');
    SET IDENTITY_INSERT Curso OFF;

    -- ---------------------------------------------------------
    -- 3. Medalhas: 12 gerais + 1 por curso (ID_Curso preenchido = medalha de
    --    conclusao daquele curso, concedida automaticamente por RegrasMedalhas)
    -- ---------------------------------------------------------
    SET IDENTITY_INSERT Medalha ON;
    INSERT INTO Medalha (ID_Medalha, Nome_Medalha, Raridade, Descricao_Medalha, ID_Curso) VALUES
        (1, N'Primeiro Passo', N'comum', N'Conclua seu primeiro material', NULL),
        (2, N'Estudante Dedicado', N'comum', N'Conclua 10 materiais', NULL),
        (3, N'Bibliotecário', N'raro', N'Conclua 20 materiais', NULL),
        (4, N'Quiz Champion', N'comum', N'Seja aprovado em uma prova', NULL),
        (5, N'Pontuação Perfeita', N'épico', N'Tire nota 10 em uma prova', NULL),
        (6, N'Mestre em C#', N'raro', N'Conclua o curso Fundamentos de C#', 1),
        (7, N'DBA Iniciante', N'raro', N'Conclua o curso Banco de Dados e Modelagem de Sistemas', 2),
        (8, N'Lenda da Tech Quest', N'lendário', N'Conclua todos os cursos publicados', NULL),
        (9, N'Mestre em Arrays', N'épico', N'Concedida pelo tutor', NULL),
        (10, N'Maratonista', N'épico', N'Concedida pelo tutor', NULL),
        (11, N'POO Master', N'épico', N'Concedida pelo tutor', NULL),
        (12, N'Arquiteto SOLID', N'épico', N'Concedida pelo tutor', NULL),
        (13, N'Pythonista', N'comum', N'Conclua o curso Lógica de Programação com Python', 3),
        (14, N'Designer Responsivo', N'comum', N'Conclua o curso HTML e CSS Responsivo', 4),
        (15, N'Guardião do Versionamento', N'comum', N'Conclua o curso Git e GitHub na Prática', 5),
        (16, N'Modelador UML', N'comum', N'Conclua o curso Engenharia de Requisitos e UML', 6),
        (17, N'Conectado', N'comum', N'Conclua o curso Redes de Computadores', 7),
        (18, N'Mestre em Java', N'raro', N'Conclua o curso Orientação a Objetos com Java', 8),
        (19, N'Ninja do JavaScript', N'raro', N'Conclua o curso JavaScript Moderno', 9),
        (20, N'Caçador de Bugs', N'raro', N'Conclua o curso Testes Automatizados', 10),
        (21, N'Nas Nuvens', N'raro', N'Conclua o curso Fundamentos de Cloud Computing', 11),
        (22, N'Guardião da Segurança', N'raro', N'Conclua o curso Segurança da Informação', 12),
        (23, N'Mestre dos Algoritmos', N'épico', N'Conclua o curso Estruturas de Dados e Algoritmos', 13),
        (24, N'Componente Mestre', N'raro', N'Conclua o curso React para Iniciantes', 14),
        (25, N'Otimizador de Consultas', N'épico', N'Conclua o curso SQL Avançado e Otimização de Consultas', 15);
    SET IDENTITY_INSERT Medalha OFF;

    -- ---------------------------------------------------------
    -- 4. Materiais
    -- ---------------------------------------------------------
    SET IDENTITY_INSERT Material ON;
    INSERT INTO Material (ID_Material, ID_Curso, Titulo_Material, Tipo_Material, Conteudo) VALUES
        (1, 1, N'Introducao ao C# e ao .NET', N'texto', N'C# e uma linguagem de programacao orientada a objetos criada pela Microsoft e executada sobre a plataforma .NET. O codigo que voce escreve nao vai direto para o processador: ele e compilado para uma linguagem intermediaria (IL) e so no momento da execucao o runtime traduz essa IL para instrucoes da maquina.

Essa etapa extra explica duas caracteristicas da linguagem. A primeira e a portabilidade: o mesmo programa roda em Windows, Linux e macOS porque quem conhece o sistema operacional e o runtime, nao o seu codigo. A segunda e a verificacao de tipos antes da execucao, que transforma em erro de compilacao o que em linguagens dinamicas so apareceria com o programa rodando.

O .NET nao e so o runtime: e tambem a biblioteca padrao. Colecoes, acesso a arquivos, rede e acesso a banco de dados ja vem prontos, e e dessa biblioteca que vem o Console.WriteLine que voce usa no primeiro programa.'),
        (2, 1, N'Variaveis e Tipos de Dados', N'texto', N'Toda variavel em C# tem um tipo declarado, e esse tipo nao muda. Escrever int idade = 25 cria um espaco que guarda numeros inteiros e so numeros inteiros; atribuir um texto ali e erro de compilacao, nao erro em tempo de execucao.

Os tipos se dividem em dois grupos com comportamentos diferentes. Tipos de VALOR (int, double, decimal, bool, char, e structs) guardam o dado em si; copiar a variavel copia o conteudo. Tipos de REFERENCIA (string, arrays, classes) guardam o endereco de onde o dado esta; copiar a variavel copia o endereco, e as duas passam a apontar para o mesmo objeto. Confundir os dois e a origem de uma classe inteira de bugs.

Sobre decimal e double: os dois guardam numeros com virgula, mas double usa base binaria e introduz pequenos erros de arredondamento, enquanto decimal usa base decimal. Para dinheiro, use decimal -- um centavo perdido por arredondamento e um centavo a explicar.

A palavra var nao cria tipo dinamico: ela pede ao compilador que descubra o tipo a partir do valor atribuido. var nome = "Ana" e exatamente string nome = "Ana".'),
        (3, 1, N'Estruturas de Controle', N'texto', N'Estruturas de controle sao o que permite ao programa tomar caminhos diferentes. O if avalia uma condicao booleana e executa um bloco ou outro. O switch compara um mesmo valor contra varias possibilidades e e mais legivel que uma sequencia de if/else quando a comparacao e sempre sobre a mesma variavel.

Para repeticao, a escolha depende do que se sabe antes de comecar. Use for quando a quantidade de voltas e conhecida (percorrer dez posicoes de um array). Use while quando a repeticao depende de uma condicao que pode nem ser verdadeira na primeira vez. Use do/while quando o bloco precisa executar ao menos uma vez antes do teste. Use foreach para percorrer uma colecao inteira sem controlar o indice -- e a forma mais segura, porque nao da para errar o limite.

Dois comandos alteram o fluxo de dentro do laco: break encerra a repeticao e continue pula para a proxima volta. Ambos resolvem casos legitimos, mas um laco cheio deles costuma ser sinal de que a condicao poderia ser escrita melhor.'),
        (4, 1, N'Arrays e Listas em C#', N'texto', N'Arrays sao colecoes de TAMANHO FIXO que armazenam elementos do mesmo tipo. Ao declarar int[] numeros = new int[5] voce reserva exatamente cinco posicoes: nao e possivel acrescentar a sexta. O acesso e por indice comecando em zero, e pedir uma posicao fora do intervalo lanca IndexOutOfRangeException.

List<T>, do namespace System.Collections.Generic, resolve a limitacao do tamanho. Internamente ela mantem um array e, quando o espaco acaba, aloca um array maior e copia os elementos. Isso e invisivel para quem usa: Add, Remove, Contains e Count funcionam sem que voce precise pensar em capacidade.

O preco dessa flexibilidade e a copia ocasional. Para uma quantidade conhecida e grande de elementos, o array e mais economico. Na pratica da maioria dos programas, List<T> e a escolha padrao, e o array aparece quando o tamanho e realmente fixo.

Exemplo:

    List<string> linguagens = new List<string>();
    linguagens.Add("C#");
    linguagens.Add("Java");

    foreach (string lang in linguagens)
    {
        Console.WriteLine(lang);
    }

O <T> e um parametro de tipo: List<string> so aceita texto, List<int> so aceita inteiros. E o compilador que garante isso, o que evita a conversao de tipo que as colecoes antigas exigiam.

Matrizes (arrays de duas dimensoes) guardam dados em linhas e colunas: int[,] notas = new int[3, 5] cria 3 linhas com 5 colunas. Para percorrer uma matriz usam-se dois lacos aninhados: o externo passa pelas linhas e o interno pelas colunas. Um padrao comum e acumular a soma de cada linha num vetor auxiliar e, ao fim de cada linha, somar esse resultado num total geral -- exatamente o tipo de rastreio de execucao que aparece em provas de algoritmos.'),
        (5, 1, N'Orientacao a Objetos: Classes e Objetos', N'texto', N'Uma classe e a definicao de um tipo: quais dados ele guarda e o que ele sabe fazer. Um objeto e uma instancia dessa definicao. A classe Aluno existe uma vez no codigo; os objetos Aluno existem um por aluno, cada um com seus proprios valores.

Encapsulamento e o primeiro dos quatro pilares, e o mais pratico: os campos ficam privados e o acesso passa por propriedades. Isso permite validar na entrada -- rejeitar uma nota fora de 0 a 10, por exemplo -- em um unico lugar, em vez de confiar que todo codigo que mexe no objeto vai lembrar da regra.

Heranca permite que uma classe reaproveite outra: Tutor e Estudante sao Usuario e herdam nome e e-mail. Polimorfismo permite que o mesmo metodo se comporte de forma diferente em cada classe filha. Abstracao e o resultado dos tres: quem usa a classe trabalha com o que ela faz, nao com como ela faz.

Heranca e menos usada do que parece. Quando a relacao nao e claramente "e um tipo de", composicao -- a classe conter outra como campo -- costuma produzir um desenho mais facil de mudar.

Sobre heranca, tres pontos costumam ser cobrados. Primeiro, ela existe para facilitar o reuso. Segundo, a subclasse herda o que a superclasse tem, mas pode e costuma acrescentar atributos e metodos proprios -- e por isso que ela e uma "extensao" da classe mae (em Java, a palavra-chave e justamente extends; em C#, os dois-pontos). Terceiro, heranca multipla e uma classe ter MAIS DE UMA superclasse direta, e nao uma superclasse com varias filhas; C# e Java nao permitem heranca multipla de classes, apenas a implementacao de varias interfaces.'),
        (6, 1, N'Exercicios do Modulo 3', N'exercicio', N'Lista de exercicios sobre arrays, listas e estruturas de repeticao. Resolva no seu proprio ambiente antes de fazer a prova.

1. Declare um array de dez inteiros, preencha com os numeros de 1 a 10 e imprima a soma.

2. Dada uma List<string> com nomes, imprima apenas os que comecam com a letra A, sem usar LINQ.

3. Escreva um metodo que receba uma List<int> e devolva o maior valor sem usar Max(). Trate o caso da lista vazia.

4. Explique, em duas ou tres linhas, por que o codigo abaixo lanca excecao:

    int[] n = new int[3];
    for (int i = 0; i <= 3; i++) { n[i] = i; }

5. Converta um array int[] para List<int> e depois de volta para array. Diga o que acontece com a memoria em cada conversao.

6. Crie uma List<string>, adicione cinco itens, remova o terceiro e imprima Count antes e depois. Explique o resultado.

As respostas nao sao entregues pela plataforma: a verificacao e a prova do curso, que cobre os mesmos conceitos.'),
        (7, 2, N'Modelo Entidade-Relacionamento', N'texto', N'O Modelo Entidade-Relacionamento (MER) descreve O QUE o sistema precisa guardar, antes de decidir COMO guardar. Ele tem tres elementos: entidades (as coisas sobre as quais se guarda informacao -- Usuario, Curso, Material), atributos (o que se sabe sobre cada uma -- nome, titulo, data) e relacionamentos (como elas se ligam -- um Curso TEM varios Materiais).

A cardinalidade do relacionamento e a parte que mais decide o desenho final. Um-para-muitos (1:N) resolve-se colocando a chave estrangeira no lado N: Material guarda ID_Curso, e nao o contrario, porque um material pertence a um curso so. Muitos-para-muitos (N:N) nao tem como ser resolvido com uma chave estrangeira em nenhum dos lados -- precisa de uma tabela intermediaria. No Tech Quest, "estudante faz cursos" e N:N, e e por isso que existe a tabela Historico: ela nao e uma tabela auxiliar, e a materializacao do relacionamento, e ganha atributos proprios (data de conclusao, situacao).

O MER e conceitual: nao fala de tipo de dado, de indice nem de banco especifico. A traducao para tabelas, colunas e chaves e o modelo LOGICO, e so depois vem o fisico, com tipos e indices. Pular a etapa conceitual e comum e custa caro: erro de cardinalidade descoberto depois que ha dado gravado exige migracao, nao edicao.'),
        (8, 2, N'Normalizacao de Dados', N'texto', N'Normalizar e organizar as colunas de forma que cada fato fique guardado em um lugar so. O objetivo nao e estetico: e eliminar tres anomalias concretas.

Anomalia de atualizacao: se o nome do tutor estivesse repetido em cada curso dele, mudar o nome exigiria acertar todas as linhas -- e esquecer uma deixa o banco se contradizendo. Anomalia de insercao: se os dados do curso estivessem na mesma tabela da matricula, nao daria para cadastrar um curso antes do primeiro aluno. Anomalia de exclusao: apagar a ultima matricula apagaria junto o curso.

As tres primeiras formas normais resolvem a maioria dos casos. 1FN: cada coluna guarda um valor unico, sem lista dentro de um campo (nada de "C#, SQL, UML" numa coluna so). 2FN: estando na 1FN, nenhuma coluna depende de apenas parte de uma chave composta. 3FN: estando na 2FN, nenhuma coluna depende de outra coluna que nao seja a chave -- se Cidade e Estado estao na mesma tabela, Estado depende de Cidade, nao da chave, e pede tabela propria.

Desnormalizar e aceitavel, mas e uma troca consciente: repete-se um dado para evitar um JOIN caro, assumindo a responsabilidade de manter as copias em sincronia. Fazer isso sem ter normalizado antes nao e otimizacao, e desorganizacao.'),
        (9, 2, N'SQL: DDL e DML', N'texto', N'SQL divide-se em subconjuntos pelo que cada comando faz. Os dois do dia a dia sao DDL e DML, e confundi-los e a origem de acidentes serios.

DDL (Data Definition Language) mexe na ESTRUTURA: CREATE cria tabela, ALTER acrescenta ou muda coluna, DROP remove objeto, TRUNCATE esvazia a tabela. No SQL Server esses comandos fazem commit implicito -- um DROP nao espera ROLLBACK para valer.

DML (Data Manipulation Language) mexe no CONTEUDO: INSERT grava, UPDATE altera, DELETE apaga, SELECT consulta. Esses participam de transacao, e e por isso que um UPDATE sem WHERE ainda pode ser desfeito antes do commit, enquanto um TRUNCATE nao.

    -- DDL: cria a estrutura
    CREATE TABLE Curso (
        ID_Curso   INT IDENTITY(1,1) PRIMARY KEY,
        Nome_Curso VARCHAR(150) NOT NULL,
        Status     VARCHAR(30)  NOT NULL
    );

    -- DML: coloca e consulta dados
    INSERT INTO Curso (Nome_Curso, Status)
    VALUES (''Fundamentos de C#'', ''Publicado'');

    SELECT Nome_Curso FROM Curso WHERE Status = ''Publicado'';

Ha ainda DCL (GRANT e REVOKE, permissoes) e TCL (BEGIN TRANSACTION, COMMIT, ROLLBACK). O TCL e o que garante que duas gravacoes que precisam valer juntas -- gravar a nota e conceder a medalha, por exemplo -- nao deixem o banco pela metade se a segunda falhar.

Alem do tipo de dado, o banco impoe restricoes de integridade. Restricao de dominio: o valor precisa pertencer ao tipo e ao intervalo da coluna. Integridade da entidade: a chave primaria nao pode ser nula nem repetida. Integridade referencial: a chave estrangeira precisa apontar para uma linha que existe. Restricoes de integridade SEMANTICA expressam regras do negocio que nao cabem nas anteriores, como "o salario do empregado nao pode passar o do supervisor"; elas sao implementadas com CHECK ou com triggers. O Tech Quest usa uma trigger para auditar a mudanca de status do usuario.'),
        (10, 2, N'Elicitacao de Requisitos e RUP', N'texto', N'Elicitar requisitos e descobrir o que o sistema precisa fazer. "Descobrir" e a palavra certa: o cliente raramente sabe enunciar o que precisa, e o que ele pede costuma ser uma solucao que ele imaginou, nao o problema que tem.

Requisitos funcionais dizem O QUE o sistema faz ("o tutor submete o curso para aprovacao"). Nao funcionais dizem COMO ele se comporta -- desempenho, seguranca, disponibilidade, usabilidade ("a senha e armazenada com hash"). Os nao funcionais sao os mais esquecidos e os mais caros de acrescentar depois, porque costumam atravessar a arquitetura inteira.

As tecnicas de elicitacao se combinam: entrevista (profundidade, pouca abrangencia), questionario (o inverso), observacao do trabalho real (revela o que ninguem lembra de contar), analise de documentos e prototipacao (que transforma a discussao abstrata em algo que o usuario aponta e corrige).

O RUP (Rational Unified Process) organiza isso em quatro fases -- Concepcao, Elaboracao, Construcao e Transicao -- percorridas em iteracoes, nao em sequencia unica. A diferenca para o modelo cascata e essa: em todas as fases se levanta requisito, se modela, se programa e se testa; o que muda e a PROPORCAO. A Concepcao e majoritariamente levantamento; a Construcao, majoritariamente programacao. Entregar algo executavel cedo e proposital: e o que permite descobrir requisito errado enquanto corrigir ainda e barato.

Duas outras tecnicas aparecem com frequencia. O workshop de requisitos (ou JAD, Joint Application Design) e uma reuniao estruturada e com tempo delimitado entre analistas e representantes do cliente. O brainstorming junta o grupo para gerar o maior numero de ideias sem critica imediata, e so depois as ideias sao avaliadas e votadas. A entrevista pode ser aberta (conversa livre) ou fechada (perguntas definidas). No ciclo de vida, o modelo sequencial sem retorno (cascata) dificulta que o cliente valide o sistema durante o desenvolvimento; o prototipo resolve duvidas de comunicacao, mas pode estimular pedidos de funcionalidades nao prioritarias; e o processo iterativo entrega versoes progressivamente mais completas para avaliacao. No RUP, a maior parte da analise de requisitos acontece na fase de Elaboracao.');
    INSERT INTO Material (ID_Material, ID_Curso, Titulo_Material, Tipo_Material, Conteudo) VALUES
        (11, 2, N'Estudo de Caso: Modelagem Completa', N'exercicio', N'Exercicio de modelagem usando a propria plataforma Tech Quest como objeto de estudo. Resolva no papel antes de olhar o banco.

1. Liste as entidades necessarias para que um estudante se matricule num curso, estude os materiais, faca a prova e receba certificado. Para cada uma, diga quais atributos ela guarda.

2. Para cada par de entidades relacionadas, determine a cardinalidade e diga onde fica a chave estrangeira. Justifique: por que a chave de Curso fica em Material, e nao o contrario?

3. "Estudante faz cursos" e um relacionamento muitos-para-muitos. Modele a tabela que o materializa e identifique quais atributos pertencem ao RELACIONAMENTO, e nao a nenhuma das duas entidades.

4. Um usuario pode ser estudante, tutor ou administrador. Compare duas solucoes: (a) uma coluna Tipo na tabela Usuario; (b) uma tabela de especializacao por papel, com chave estrangeira para Usuario. Diga qual permite guardar atributos proprios de cada papel e o que acontece, em cada uma, quando aparece um quarto papel.

5. A nota da prova fica em qual tabela, e por que ela nao pode ficar em Estudante nem em Prova?

6. Verifique se o seu modelo esta na 3FN. Se alguma coluna depende de outra coluna que nao e chave, aponte qual e separe.

7. Escreva o DDL das tabelas do item 1, com chaves primarias, estrangeiras e as restricoes NOT NULL que voce considerar necessarias.

Depois de resolver, compare com o script de criacao do banco do projeto. Onde o seu modelo divergir, a pergunta certa nao e qual esta certo -- e qual decisao cada um tomou, e o que ela custa.'),
        (12, 3, N'Algoritmos e o primeiro programa', N'texto', N'Um algoritmo é uma sequência finita de passos que resolve um problema. Antes de qualquer linguagem, programar é decompor: entender a entrada, definir a saída e descrever, sem ambiguidade, o caminho entre as duas.

Python é uma boa primeira linguagem porque a sintaxe atrapalha pouco esse raciocínio. Não há ponto e vírgula nem chaves: os blocos são definidos pela indentação, o que obriga o código a ter a mesma forma visual da lógica.

    nome = input("Qual é o seu nome? ")
    print("Olá,", nome)

Esse programa já tem as três partes de todo algoritmo: entrada (input), processamento (guardar o valor em uma variável) e saída (print). Python é interpretado: cada linha é executada na ordem, e um erro só aparece quando a linha que o contém é alcançada.'),
        (13, 3, N'Variáveis, tipos e operadores', N'texto', N'Em Python, uma variável é um nome associado a um valor. O tipo pertence ao valor, não ao nome: idade = 20 cria um int, e a mesma variável pode depois receber um texto. Essa liberdade é a tipagem dinâmica, e o preço dela é descobrir alguns erros só na execução.

Os tipos básicos são int (inteiros), float (números com casas decimais), str (texto) e bool (True ou False). A função input sempre devolve str, por isso é comum converter: idade = int(input("Idade: ")).

Os operadores aritméticos seguem a matemática, com dois detalhes. A barra simples (/) sempre devolve float: 7 / 2 é 3.5. A barra dupla (//) faz divisão inteira: 7 // 2 é 3. O operador % devolve o resto, e é a forma clássica de testar se um número é par: n % 2 == 0.

Comparações (==, !=, <, >, <=, >=) produzem bool, e podem ser combinadas com and, or e not.

Os operadores lógicos seguem a lógica proposicional. A prioridade é: not, depois and, depois or (na notação booleana: NÃO, depois E (*), depois OU (+)). A tabela verdade lista todas as combinações de V e F das variáveis e o resultado da expressão, e é a forma de verificar se duas expressões são equivalentes. Um caso clássico: a condicional "se p então q" só é FALSA quando p é verdadeiro e q é falso. Por isso, se alguém descobre que a frase "se o time X venceu, então o time Y é campeão" é falsa, tem certeza de que o time X venceu e de que o time Y não é campeão.'),
        (14, 3, N'Decisões, repetições e funções', N'texto', N'O if executa um bloco apenas quando a condição é verdadeira; elif testa uma nova condição quando a anterior falhou; else cobre o que sobrou. Só um dos blocos de uma cadeia if/elif/else é executado.

Para repetir, o while continua enquanto a condição for verdadeira, e o for percorre uma sequência. A função range gera sequências de números: range(5) produz 0, 1, 2, 3 e 4, ou seja, para antes do valor informado.

    def media(notas):
        return sum(notas) / len(notas)

    print(media([7, 8, 9]))   # 8.0

Funções dão nome a um trecho de lógica e permitem reutilizá-lo. Os parâmetros são as entradas; return devolve o resultado e encerra a função. Uma função sem return devolve None. Bons nomes de função descrevem o que ela faz, não como faz.'),
        (15, 3, N'Exercícios de lógica', N'exercicio', N'Resolva no seu computador antes de fazer a prova.

1. Leia dois números e mostre o maior deles, ou a mensagem "iguais".

2. Leia um número inteiro e informe se ele é par ou ímpar usando o operador %.

3. Mostre a tabuada de um número informado pelo usuário, de 1 a 10, usando for e range.

4. Escreva uma função eh_primo(n) que devolva True se n for primo.

5. Leia notas até o usuário digitar -1 e mostre a média das notas válidas.

6. Explique por que range(1, 10) não inclui o número 10.'),
        (16, 4, N'HTML semântico', N'texto', N'HTML descreve o significado do conteúdo, não a aparência. Uma página bem estruturada usa a tag certa para cada parte: header para o cabeçalho, nav para a navegação, main para o conteúdo principal, article para um conteúdo independente, section para agrupar um tema, aside para conteúdo complementar e footer para o rodapé.

Usar div para tudo funciona visualmente, mas perde informação. Leitores de tela usam as tags semânticas para navegar, e buscadores usam a mesma estrutura para entender a página. Títulos seguem hierarquia: um h1 por página, e h2, h3 em ordem, sem pular níveis por causa do tamanho da fonte.

Formulários também têm semântica: cada input deve ter um label associado pelo atributo for, e o type correto (email, number, date) faz o celular abrir o teclado adequado.

Toda imagem que transmite informação precisa do atributo alt, com um texto que a descreva. O leitor de tela lê esse texto para pessoas com deficiência visual, e o navegador o exibe se a imagem não carregar. Quando a imagem é um link, o alt descreve o destino do link. Isso atende o requisito de ACESSIBILIDADE: oferecer condições de igualdade na interação com o sistema a pessoas com deficiência.'),
        (17, 4, N'CSS: seletores, box model e Flexbox', N'texto', N'Todo elemento é uma caixa formada por conteúdo, padding (espaço interno), border e margin (espaço externo). Com box-sizing: border-box, a largura declarada inclui padding e borda, o que torna os cálculos de layout previsíveis.

Quando duas regras atingem o mesmo elemento, vence a de maior especificidade: id vale mais que classe, que vale mais que tag. Em empate, vence a regra declarada por último.

Flexbox organiza itens em uma dimensão. No contêiner, display: flex ativa o modelo; justify-content alinha no eixo principal; align-items alinha no eixo cruzado; flex-wrap permite quebrar linha. Nos itens, flex: 1 faz cada um ocupar uma parte igual do espaço livre.

    .cartoes {
        display: flex;
        gap: 16px;
        flex-wrap: wrap;
    }'),
        (18, 4, N'Grid, media queries e mobile-first', N'texto', N'CSS Grid organiza em duas dimensões, linhas e colunas ao mesmo tempo. grid-template-columns: repeat(3, 1fr) cria três colunas iguais; a unidade fr divide o espaço disponível.

Mobile-first significa escrever o CSS base para a tela pequena e acrescentar regras para telas maiores com media queries de largura mínima:

    .grade { display: grid; grid-template-columns: 1fr; }

    @media (min-width: 768px) {
        .grade { grid-template-columns: repeat(2, 1fr); }
    }

A ordem importa porque o celular baixa e aplica só o essencial, e o layout cresce de forma progressiva. Sem a meta tag viewport no head da página, o navegador do celular simula uma tela larga e as media queries não funcionam como esperado.'),
        (19, 4, N'Projeto: página de perfil responsiva', N'exercicio', N'1. Monte uma página de perfil com header, main e footer usando apenas tags semânticas.

2. Crie uma grade de cartões com 1 coluna no celular, 2 a partir de 768px e 3 a partir de 1200px.

3. Use Flexbox para alinhar foto e nome lado a lado no cabeçalho.

4. Inclua um formulário de contato com labels associados e tipos de input adequados.

5. Teste a página nas ferramentas de desenvolvedor do navegador simulando três tamanhos de tela.'),
        (20, 5, N'Repositório, commits e histórico', N'texto', N'Git é um sistema de controle de versão distribuído: cada cópia do repositório tem o histórico completo, e não apenas a versão atual dos arquivos.

Um arquivo passa por três áreas. O diretório de trabalho é onde você edita. A área de preparação (staging) recebe, com git add, as mudanças que farão parte do próximo commit. O commit grava essas mudanças no histórico, com autor, data e mensagem.

    git init
    git add index.html
    git commit -m "Cria página inicial"
    git log --oneline

Bons commits são pequenos e coesos, e a mensagem diz o que mudou e por quê. O arquivo .gitignore lista o que não deve ser versionado: dependências baixadas, arquivos compilados e, principalmente, senhas e chaves.

O Git é uma ferramenta de GERÊNCIA DE CONFIGURAÇÃO de software, a disciplina que controla a evolução do software. Os conceitos dela aparecem com outros nomes em provas: item de configuração é cada elemento unitário que compõe o software e deve ser gerenciado (um arquivo de código, um diagrama); linha de base (baseline) é um marco de referência a partir do qual as mudanças são controladas, como a versão 1.0; a auditoria de configuração verifica se a linha de base e os requisitos foram atendidos; o relatório de status informa o que mudou. As atividades clássicas são: identificação dos itens, controle de versão, controle de alterações, auditoria e relatório de status. A gerência de configuração começa junto com o projeto, e não só depois da entrega.');
    INSERT INTO Material (ID_Material, ID_Curso, Titulo_Material, Tipo_Material, Conteudo) VALUES
        (21, 5, N'Branches, merge e conflitos', N'texto', N'Uma branch é uma linha de desenvolvimento independente. Criar uma branch para cada funcionalidade permite trabalhar sem quebrar a versão principal.

    git switch -c nova-tela
    # ... commits ...
    git switch main
    git merge nova-tela

O merge junta o histórico das duas branches. Quando as duas alteraram as mesmas linhas de um arquivo, o Git não decide sozinho: marca o conflito com <<<<<<<, ======= e >>>>>>>, e cabe a quem faz o merge editar o arquivo, escolher o conteúdo correto, remover os marcadores e concluir com git add e git commit.

git rebase reaplica os commits de uma branch sobre outra e produz um histórico linear, mas reescreve commits. Nunca faça rebase de commits que outras pessoas já baixaram.

No vocabulário geral de controle de versão, checkout é baixar os arquivos do repositório para começar a trabalhar, commit é gravar as mudanças e conflito é quando duas pessoas alteraram o mesmo trecho: ele se resolve com merge, e não com tag. A tag marca uma versão (por exemplo, v1.0.0), e a branch é uma ramificação, recomendada quando uma funcionalidade nova altera vários arquivos. O ramo principal (main, ou trunk) é a linha de desenvolvimento que vai do início do projeto até hoje; versão estável é outra coisa: é uma versão marcada desse histórico.'),
        (22, 5, N'GitHub e trabalho em equipe', N'texto', N'O GitHub hospeda repositórios remotos. git push envia commits locais para o remoto; git pull baixa e integra as mudanças dos colegas; git clone cria a cópia local de um repositório existente.

O fluxo mais comum em equipe é: criar uma branch, fazer commits, enviar com push e abrir um pull request. O pull request é um pedido para integrar a branch na principal, e é ali que acontece a revisão de código: colegas comentam linhas, sugerem mudanças e aprovam.

Proteger a branch main, exigindo pull request revisado e testes passando, evita que um erro chegue direto à versão principal. Issues registram tarefas e defeitos, e podem ser fechadas automaticamente por um commit que mencione o número delas.'),
        (23, 5, N'Prática guiada de Git', N'exercicio', N'1. Crie um repositório, faça três commits e mostre o histórico com git log --oneline.

2. Crie um .gitignore que ignore a pasta node_modules e arquivos .env.

3. Crie duas branches que alterem a mesma linha de um arquivo e resolva o conflito no merge.

4. Publique o repositório no GitHub e abra um pull request de uma branch nova.

5. Explique por que não se deve versionar senhas mesmo em repositório privado.'),
        (24, 6, N'Requisitos funcionais, não funcionais e regras de negócio', N'texto', N'Requisito funcional descreve o que o sistema faz: "o estudante realiza a prova do curso". Requisito não funcional descreve como o sistema se comporta: segurança, desempenho, usabilidade, disponibilidade. Regra de negócio é uma política do domínio que o sistema precisa respeitar, independente da tecnologia: "o aluno é aprovado com nota maior ou igual à mínima da prova".

Um bom requisito é verificável. "O sistema deve ser rápido" não é; "a listagem de cursos deve responder em até 2 segundos" é. Também deve ser rastreável: cada requisito liga-se aos casos de uso, às telas e aos testes que o implementam.

Requisitos mudam. Registrar a origem de cada um (quem pediu, em que reunião) ajuda a negociar a mudança e a entender o impacto.

Exemplos típicos de requisito não funcional: usabilidade, segurança, desempenho (tempo de resposta), confiabilidade e portabilidade (levar o sistema para outro sistema operacional). "O sistema deve emitir relatórios de compras a cada quinze dias" é funcional; "o tempo de resposta não deve passar de 30 segundos" é não funcional. A engenharia de requisitos tem quatro subprocessos: estudo de viabilidade, elicitação (com artefatos como o registro das entrevistas), especificação (por exemplo, casos de uso para os requisitos funcionais) e validação (por exemplo, protótipos de tela mostrados ao cliente).'),
        (25, 6, N'Diagrama de casos de uso', N'texto', N'O diagrama de casos de uso mostra o que o sistema oferece e quem usa cada funcionalidade. Atores são papéis externos, pessoas ou outros sistemas; casos de uso são objetivos que o ator alcança com o sistema, nomeados com verbo no infinitivo ("Realizar prova").

Dois relacionamentos entre casos de uso costumam ser confundidos. include indica um comportamento obrigatório, sempre executado: "Realizar prova" inclui "Corrigir prova". extend indica um comportamento opcional, que acontece só sob uma condição: "Emitir certificado" estende "Realizar prova" quando o aluno é aprovado e concluiu o curso.

O diagrama é um índice. O detalhe fica na descrição de cada caso de uso: pré-condições, fluxo principal, fluxos alternativos e pós-condições.

Entre casos de uso também existe GENERALIZAÇÃO: o caso de uso filho herda o comportamento do pai e pode acrescentar ou sobrescrever partes dele, como acontece entre classes. No extend, o comportamento estendido é opcional e só ocorre sob certas condições; o caso de uso que estende nunca fica isolado, ele só existe como parte do caso de uso base.'),
        (26, 6, N'Diagramas de classes e de sequência', N'texto', N'O diagrama de classes mostra a estrutura: classes, atributos, métodos e relacionamentos. Associação liga classes que se conhecem; agregação é um "tem um" em que a parte existe sem o todo; composição é um "tem um" em que a parte morre com o todo; generalização é a herança. A multiplicidade (1, 0..1, *, 1..*) diz quantos objetos participam da relação.

O diagrama de sequência mostra o comportamento no tempo: as linhas de vida dos objetos lado a lado e as mensagens trocadas de cima para baixo. É o diagrama certo para explicar um fluxo, como o login: a tela envia as credenciais ao controlador, que chama o serviço de autenticação, que consulta o repositório e devolve o token.

Diagramas servem para comunicar e decidir. Um diagrama que ninguém consegue ler por excesso de detalhes falhou no objetivo.'),
        (27, 6, N'Estudo de caso: modelagem de uma biblioteca', N'exercicio', N'1. Liste cinco requisitos funcionais e três não funcionais para um sistema de biblioteca.

2. Escreva duas regras de negócio, como o limite de livros por leitor.

3. Desenhe o diagrama de casos de uso com os atores Leitor e Bibliotecário.

4. Identifique um include e um extend no seu diagrama e justifique cada um.

5. Desenhe o diagrama de sequência do caso de uso "Emprestar livro".'),
        (28, 7, N'Modelo TCP/IP e camadas', N'texto', N'Redes são organizadas em camadas, e cada camada resolve um problema usando o serviço da camada de baixo. O modelo TCP/IP tem quatro: acesso à rede (o meio físico e os quadros, como Ethernet e Wi-Fi), internet (endereçamento e roteamento, com o protocolo IP), transporte (comunicação entre processos, com TCP e UDP) e aplicação (os protocolos que os programas usam, como HTTP, DNS e SMTP).

O modelo OSI, de sete camadas, é usado como referência didática e divide melhor as responsabilidades, mas a internet funciona sobre TCP/IP.

TCP é orientado a conexão: estabelece a conexão com o three-way handshake, garante entrega e ordem e retransmite o que se perde. UDP não tem conexão nem garantia, e por isso é mais leve: serve a streaming, jogos e DNS, em que chegar rápido importa mais que chegar tudo.

TCP/IP são dois protocolos: o TCP controla a comunicação e a qualidade da entrega entre origem e destino, e o IP cuida do endereçamento. Por ser um padrão comum, ele permite que sistemas operacionais e hardwares diferentes conversem entre si.'),
        (29, 7, N'Endereçamento IP e sub-redes', N'texto', N'Um endereço IPv4 tem 32 bits, escritos como quatro números de 0 a 255 (192.168.0.10). A máscara de sub-rede separa a parte que identifica a rede da parte que identifica o host. Na notação CIDR, /24 indica 24 bits de rede: em 192.168.0.0/24, sobram 8 bits para hosts, ou 254 endereços utilizáveis, porque o primeiro identifica a rede e o último é o de broadcast.

As faixas 10.0.0.0/8, 172.16.0.0/12 e 192.168.0.0/16 são privadas: não são roteadas na internet. O roteador doméstico usa NAT para que vários dispositivos com IP privado compartilhem um único IP público.

O IPv6 usa 128 bits e foi criado porque os endereços IPv4 se esgotaram. O DHCP distribui endereços automaticamente, para que ninguém precise configurar IP à mão em cada dispositivo.

A máscara define quem está na mesma rede. Com 255.255.0.0, os dois primeiros octetos identificam a rede: 175.16.10.200 e 175.16.30.200 estão na mesma rede, 175.16.0.0. Com 255.255.255.0, contam os três primeiros octetos: 175.16.10.200 passa a estar na rede 175.16.10.0 e deixa de enxergar diretamente 175.16.30.200. Máscaras diferentes na mesma rede local são uma causa comum de "a impressora compartilhada não responde".'),
        (30, 7, N'DNS, HTTP e equipamentos de rede', N'texto', N'O DNS traduz nomes (techquest.com.br) em endereços IP. A consulta passa pelo resolvedor do provedor, que pergunta aos servidores raiz, depois aos do domínio de topo (.br) e por fim ao servidor autoritativo do domínio. As respostas ficam em cache pelo tempo definido no TTL.

O HTTP é o protocolo da web: o cliente envia uma requisição com método (GET, POST, PUT, DELETE), caminho e cabeçalhos, e o servidor responde com um código de status (200 OK, 404 Not Found, 500 Internal Server Error) e o corpo. O HTTPS é o HTTP dentro de uma conexão cifrada com TLS, na porta 443.

Equipamentos: o switch conecta dispositivos da mesma rede local usando endereços MAC; o roteador conecta redes diferentes usando endereços IP; o access point leva a rede local ao Wi-Fi.

O servidor DNS que não é autoritativo para um nome atua como cliente: pergunta a outros servidores e guarda a resposta em cache, para responder às próximas consultas sem recorrer a ninguém. O DNS é, no fundo, um banco de dados distribuído pelo mundo, e continua necessário no IPv6. Sobre equipamentos: o hub repete cada quadro para todas as portas, então todos os computadores ligados a ele disputam o mesmo meio e as colisões crescem com o número de máquinas; o switch encaminha cada quadro só para a porta de destino, o que reduz as colisões e melhora o desempenho.');
    INSERT INTO Material (ID_Material, ID_Curso, Titulo_Material, Tipo_Material, Conteudo) VALUES
        (31, 7, N'Exercícios de redes', N'exercicio', N'1. Calcule quantos hosts utilizáveis existem nas redes /24, /26 e /28.

2. Diga se os endereços 10.1.2.3, 172.20.0.1, 200.10.5.4 e 192.168.1.1 são públicos ou privados.

3. Use o comando nslookup para descobrir o IP de um site e explique cada etapa da resolução.

4. Abra as ferramentas de desenvolvedor do navegador e identifique o método, o status e três cabeçalhos de uma requisição.

5. Explique por que um jogo online costuma preferir UDP a TCP.'),
        (32, 8, N'Classes, objetos e construtores', N'texto', N'Uma classe descreve um tipo: os atributos que cada objeto guarda e os métodos que ele executa. O objeto é a instância concreta, criada com new. Cada objeto tem seu próprio estado, mas todos compartilham o mesmo comportamento definido na classe.

    public class Conta {
        private double saldo;

        public Conta(double saldoInicial) {
            this.saldo = saldoInicial;
        }

        public void depositar(double valor) {
            if (valor <= 0) throw new IllegalArgumentException("Valor inválido");
            saldo += valor;
        }
    }

O construtor tem o mesmo nome da classe e não tem tipo de retorno. Ele garante que o objeto já nasça em um estado válido. A palavra this diferencia o atributo do parâmetro quando os dois têm o mesmo nome.

Uma classe pode ter vários construtores, desde que com parâmetros diferentes: é a sobrecarga de construtores, usada para oferecer formas diferentes de criar o objeto. Uma classe abstrata não pode ser instanciada com new, mas pode ser usada como TIPO: Impressora[] imp = new Impressora[3] cria um vetor de referências, e cada posição recebe um objeto de uma subclasse concreta.'),
        (33, 8, N'Encapsulamento e modificadores de acesso', N'texto', N'Encapsular é esconder o estado e expor apenas operações que mantêm o objeto coerente. No exemplo da conta, saldo é private: ninguém de fora consegue atribuir um saldo negativo, porque a única porta de entrada é depositar, que valida o valor.

Java tem quatro níveis de acesso. private: visível só dentro da classe. Sem modificador (package-private): visível no mesmo pacote. protected: visível no pacote e nas subclasses. public: visível em qualquer lugar.

Getters e setters não são encapsulamento por si só. Um setter que aceita qualquer valor é um atributo público com mais passos. O ganho vem quando o método aplica a regra de negócio, ou quando não existe setter porque o valor não deve mudar depois da criação.'),
        (34, 8, N'Herança, polimorfismo e interfaces', N'texto', N'Herança (extends) faz uma classe reaproveitar atributos e métodos de outra. Java permite herdar de uma única classe, mas implementar várias interfaces (implements).

Polimorfismo é a capacidade de uma referência do tipo da superclasse executar o método da subclasse. Se Forma tem area() e Circulo e Quadrado sobrescrevem esse método com @Override, uma lista de Forma calcula a área correta de cada item sem nenhum if por tipo.

Uma interface declara um contrato: o que deve ser feito, sem dizer como. Uma classe abstrata pode ter estado e métodos prontos, mas não pode ser instanciada. Na dúvida, prefira interfaces: elas acoplam menos.

Sobrecarga (overload) é outra coisa: métodos com o mesmo nome e parâmetros diferentes na mesma classe, resolvidos em tempo de compilação.'),
        (35, 8, N'Exercícios de modelagem orientada a objetos', N'exercicio', N'1. Crie a classe Produto com nome e preço privados e um construtor que rejeite preço negativo.

2. Crie a interface Pagavel com o método double valorAPagar() e implemente-a em Boleto e CartaoCredito.

3. Crie a classe abstrata Funcionario com o método abstrato calcularSalario() e duas subclasses com regras diferentes.

4. Monte uma List<Funcionario> com objetos das duas subclasses e some os salários sem usar instanceof.

5. Explique a diferença entre sobrecarga e sobrescrita com um exemplo de cada.'),
        (36, 9, N'Variáveis, escopo e funções', N'texto', N'Use const para valores que não serão reatribuídos e let para os que serão. Ambas têm escopo de bloco: uma variável declarada dentro de um if não existe fora dele. O antigo var tem escopo de função e é içado (hoisting), o que gera comportamentos confusos; em código novo, evite-o.

Funções são valores em JavaScript: podem ser guardadas em variáveis, passadas como argumento e devolvidas por outras funções. A arrow function é uma forma curta:

    const dobro = n => n * 2;

Comparações: == converte os tipos antes de comparar ("5" == 5 é true), enquanto === compara valor e tipo ("5" === 5 é false). Prefira sempre === para não depender das regras de conversão automática.'),
        (37, 9, N'Arrays e manipulação do DOM', N'texto', N'Os métodos de array substituem boa parte dos laços. map transforma cada item e devolve um novo array do mesmo tamanho; filter devolve só os itens que passam no teste; reduce acumula tudo em um único valor; find devolve o primeiro item que satisfaz a condição.

    const aprovados = notas.filter(n => n >= 7);
    const media = notas.reduce((s, n) => s + n, 0) / notas.length;

O DOM é a árvore de objetos que representa a página. document.querySelector encontra um elemento por seletor CSS; textContent altera o texto; classList.add e classList.remove mudam classes; addEventListener reage a cliques, digitação e envio de formulário.

Ao inserir texto vindo do usuário, prefira textContent a innerHTML: innerHTML interpreta o texto como HTML e abre espaço para injeção de script (XSS).'),
        (38, 9, N'Programação assíncrona e fetch', N'texto', N'Operações demoradas, como requisições de rede, não bloqueiam a página: elas devolvem uma Promise, que representa um valor futuro. A Promise termina resolvida (sucesso) ou rejeitada (erro).

async/await permite escrever código assíncrono com cara de sequencial:

    async function carregarCursos() {
        try {
            const resposta = await fetch("/api/cursos");
            if (!resposta.ok) throw new Error("HTTP " + resposta.status);
            return await resposta.json();
        } catch (erro) {
            console.error(erro);
            return [];
        }
    }

Um detalhe importante: fetch só rejeita a Promise em falha de rede. Uma resposta 404 ou 500 chega como sucesso, e por isso é preciso conferir resposta.ok. await só pode ser usado dentro de funções declaradas com async.'),
        (39, 9, N'Exercícios de JavaScript', N'exercicio', N'1. A partir de um array de alunos com nome e nota, gere com map uma lista só com os nomes.

2. Use filter e reduce para calcular a média apenas dos aprovados.

3. Crie uma lista de tarefas em que o usuário adiciona itens por um input e remove clicando neles.

4. Consuma uma API pública com fetch e async/await, mostrando uma mensagem amigável quando a resposta não for ok.

5. Explique a diferença de resultado entre "10" == 10 e "10" === 10.'),
        (40, 10, N'Por que testar e a pirâmide de testes', N'texto', N'Testes automatizados verificam o comportamento do sistema toda vez que o código muda. O ganho principal não é encontrar o erro de hoje, e sim impedir que um erro corrigido volte (regressão) sem ninguém perceber.

A pirâmide de testes organiza os tipos pela quantidade ideal. Na base, muitos testes de unidade: rápidos, isolados, verificam uma regra de cada vez. No meio, testes de integração: verificam se as partes funcionam juntas, como serviço e banco de dados. No topo, poucos testes ponta a ponta: simulam o usuário usando o sistema inteiro, são lentos e frágeis.

Inverter a pirâmide, com muitos testes de interface e poucos de unidade, produz uma suíte lenta, que falha por motivos alheios ao que se quer verificar e que a equipe acaba deixando de rodar.

Além da divisão por nível (unidade, integração, ponta a ponta), os testes se dividem pelo objetivo. Teste funcional: verifica se o software faz o que os requisitos especificam. Teste de desempenho: mede tempo de resposta e comportamento sob carga. Teste de aceitação: feito pelo cliente, para validar se o que foi entregue é o que foi pedido. Teste de instalação: verifica se o software instala e funciona no ambiente previsto. A verificação pergunta "construímos o produto do jeito certo?"; a validação pergunta "construímos o produto certo?".');
    INSERT INTO Material (ID_Material, ID_Curso, Titulo_Material, Tipo_Material, Conteudo) VALUES
        (41, 10, N'Testes de unidade: estrutura e boas práticas', N'texto', N'Um teste de unidade segue o padrão AAA: Arrange (preparar os dados), Act (executar a ação) e Assert (verificar o resultado).

    [Fact]
    public void Nota_igual_a_minima_aprova()
    {
        var prova = new Prova { NotaMinima = 7m };   // Arrange
        var aprovado = prova.Aprovado(7m);           // Act
        Assert.True(aprovado);                        // Assert
    }

Bons testes são determinísticos (mesmo resultado em toda execução), independentes entre si e com nomes que descrevem o comportamento esperado. Os valores de fronteira merecem atenção especial: se a regra é "nota maior ou igual a 7", teste 6.99, 7 e 7.01.

Cobertura de código mede quais linhas foram executadas pelos testes, não se o comportamento foi verificado. Uma cobertura alta com asserções fracas dá falsa segurança.'),
        (42, 10, N'Dublês de teste e testes de integração', N'texto', N'Para testar uma regra de negócio sem depender do banco de dados, substitui-se a dependência real por um dublê. Stub devolve respostas prontas. Fake é uma implementação simplificada que funciona, como um repositório em memória. Mock verifica se uma interação aconteceu, por exemplo se o método de salvar foi chamado uma vez.

Dublês só são possíveis quando o código depende de abstrações (interfaces) e as recebe por injeção de dependência. Uma classe que cria a própria conexão com o banco dentro do método não pode ser testada isoladamente.

Testes de integração usam as dependências reais, ou próximas disso, como um banco criado só para o teste. Eles pegam erros que os dublês escondem: mapeamento errado de coluna, consulta SQL inválida, configuração ausente.

Os métodos ágeis levam os testes para o centro do processo. Na XP (Extreme Programming), os testes automatizados são escritos antes do código da funcionalidade (TDD), e a programação em pares melhora a qualidade com padrões de código combinados entre os dois. No Scrum, o Product Owner gerencia o product backlog, e o quadro Kanban mostra visualmente o andamento das tarefas.'),
        (43, 10, N'Exercícios de testes', N'exercicio', N'1. Escreva testes para uma função que calcula desconto, cobrindo valores de fronteira.

2. Reescreva um teste com nome genérico (Teste1) para que o nome descreva o comportamento.

3. Crie um repositório falso em memória e teste um serviço que depende dele.

4. Identifique, em um código de exemplo, o que impede o teste de unidade e refatore usando injeção de dependência.

5. Explique por que 100% de cobertura não garante ausência de defeitos.'),
        (44, 11, N'O que é computação em nuvem', N'texto', N'Computação em nuvem é o fornecimento de recursos de TI (servidores, armazenamento, bancos de dados, redes) sob demanda, pela internet, com pagamento pelo uso. As características que a definem são autoatendimento, acesso pela rede, recursos compartilhados entre clientes, elasticidade e serviço medido.

Elasticidade é a capacidade de aumentar ou reduzir recursos conforme a carga. Escalar verticalmente é dar mais CPU e memória à mesma máquina; escalar horizontalmente é acrescentar mais instâncias atrás de um balanceador de carga. A escala horizontal tem limite bem maior, mas exige que a aplicação não guarde estado na memória do servidor.

O modelo de custo muda de investimento (comprar servidores) para despesa operacional (pagar pelo uso). Isso favorece quem está começando, mas exige acompanhar o consumo: recursos esquecidos ligados continuam sendo cobrados.

A tecnologia que torna a nuvem possível é a VIRTUALIZAÇÃO: uma camada de software, o hipervisor (Virtual Machine Monitor), divide um servidor físico em várias máquinas virtuais isoladas. Ela permite consolidar servidores, reduzir custos, aumentar a disponibilidade, recuperar desastres e simplificar testes, e é a tecnologia central de um data center. Os tipos mais citados são a virtualização completa e a paravirtualização. Uma máquina virtual executa os programas num ambiente controlado, o que melhora o controle de segurança, mas não a torna imune a vírus nem economiza CPU e memória em relação à máquina real.'),
        (45, 11, N'IaaS, PaaS, SaaS e modelos de implantação', N'texto', N'Os modelos de serviço dizem quanto o provedor gerencia. Em IaaS (infraestrutura como serviço), o provedor entrega máquinas virtuais e rede; sistema operacional, atualizações e aplicação ficam com o cliente. Em PaaS (plataforma como serviço), o provedor cuida também do sistema operacional e do ambiente de execução, e o cliente publica apenas a aplicação, como no Azure App Service. Em SaaS (software como serviço), o cliente só usa o software pronto, como um e-mail corporativo.

Quanto mais acima nessa escala, menos controle e menos trabalho de manutenção.

Os modelos de implantação dizem onde a nuvem está: pública (recursos do provedor compartilhados entre clientes), privada (dedicada a uma organização) e híbrida (as duas integradas, por exemplo com dados sensíveis no ambiente próprio e o restante na nuvem pública).

Contratar nuvem é uma forma de terceirização (outsourcing) de TI: a empresa deixa de manter a infraestrutura, e a equipe de TI pode se dedicar a melhorias e inovações ligadas ao negócio.'),
        (46, 11, N'Serviços essenciais, regiões e responsabilidade compartilhada', N'texto', N'Os provedores organizam a infraestrutura em regiões (localizações geográficas, como Brazil South) e zonas de disponibilidade (datacenters independentes dentro da região). Escolher a região mais próxima dos usuários reduz a latência e pode ser exigência legal para dados pessoais.

Os serviços essenciais são: computação (máquinas virtuais, contêineres, funções serverless), armazenamento (objetos, discos, arquivos) e bancos de dados gerenciados, em que backup, atualização e alta disponibilidade ficam com o provedor.

No modelo de responsabilidade compartilhada, o provedor protege a infraestrutura física e os serviços que oferece; o cliente continua responsável pelos próprios dados, pelas contas de acesso e pela configuração. Um banco de dados liberado para qualquer IP na internet é falha do cliente, não do provedor.'),
        (47, 11, N'Exercícios de nuvem', N'exercicio', N'1. Classifique como IaaS, PaaS ou SaaS: máquina virtual, App Service, Microsoft 365, banco de dados gerenciado.

2. Explique a diferença entre escalar verticalmente e horizontalmente com um exemplo.

3. Liste três responsabilidades do cliente no modelo de responsabilidade compartilhada.

4. Escolha uma região para uma aplicação com usuários no Brasil e justifique.

5. Descreva duas formas de evitar custos inesperados em uma conta de nuvem.'),
        (48, 12, N'Pilares da segurança e gestão de riscos', N'texto', N'A segurança da informação se apoia em três pilares, a tríade CID. Confidencialidade: só quem tem permissão acessa a informação. Integridade: a informação não é alterada de forma indevida. Disponibilidade: a informação está acessível quando necessária. A eles somam-se autenticidade (garantia de quem é o autor) e não repúdio (o autor não pode negar a ação).

Risco é a combinação entre a probabilidade de uma ameaça explorar uma vulnerabilidade e o impacto que isso causa. Gerir riscos é decidir, para cada um, se ele será mitigado, transferido (um seguro, por exemplo), evitado ou aceito.

A LGPD (Lei 13.709/2018) estabelece regras para o tratamento de dados pessoais no Brasil: finalidade definida, coleta mínima, segurança adequada e direitos do titular, como acesso e exclusão dos próprios dados.

A norma ABNT NBR ISO/IEC 27001 define como montar um Sistema de Gestão da Segurança da Informação (SGSI), que preserva confidencialidade, integridade e disponibilidade por meio da gestão de riscos. A base dele é a POLÍTICA DE SEGURANÇA: um conjunto formal de normas e procedimentos, divulgado a todos os usuários. Contra ransomware, que criptografa os dados e exige resgate, as medidas centrais são treinar os usuários (e-mails e sites suspeitos são a porta de entrada) e manter cópias de segurança com um plano de recuperação de desastres.'),
        (49, 12, N'Autenticação, autorização e criptografia', N'texto', N'Autenticação responde "quem é você?"; autorização responde "o que você pode fazer?". A autenticação multifator combina algo que o usuário sabe (senha), algo que tem (celular) e algo que é (biometria), e reduz muito o efeito de uma senha vazada.

Senhas nunca são guardadas em texto nem criptografadas de forma reversível: guarda-se o hash. Algoritmos rápidos como MD5 e SHA-1 são inadequados, porque permitem testar bilhões de tentativas por segundo. Algoritmos próprios para senha, como BCrypt e Argon2, são lentos de propósito e usam sal, um valor aleatório que faz duas senhas iguais gerarem hashes diferentes.

Criptografia simétrica usa a mesma chave para cifrar e decifrar (AES) e é rápida. A assimétrica usa um par de chaves, pública e privada (RSA), e resolve a troca de chaves. O TLS, por trás do HTTPS, usa as duas: a assimétrica para combinar uma chave, e a simétrica para cifrar a conversa.'),
        (50, 12, N'Vulnerabilidades web e desenvolvimento seguro', N'texto', N'O OWASP Top 10 lista os riscos mais críticos em aplicações web. Três deles aparecem com frequência em projetos de estudantes.

Injeção de SQL: a aplicação monta a consulta concatenando texto digitado pelo usuário, e ele consegue alterar o comando. A prevenção é usar consultas parametrizadas, que tratam a entrada sempre como dado.

Cross-site scripting (XSS): a aplicação exibe conteúdo do usuário como HTML, e um script malicioso roda no navegador de outras pessoas. A prevenção é escapar a saída e evitar inserir texto do usuário com innerHTML.

Controle de acesso quebrado: a API confia no id que vem na URL ou no corpo e permite que um usuário acesse dados de outro. A prevenção é obter a identidade do token validado no servidor e verificar a permissão a cada requisição. Segredos, como chaves e senhas de banco, ficam fora do código-fonte.');
    INSERT INTO Material (ID_Material, ID_Curso, Titulo_Material, Tipo_Material, Conteudo) VALUES
        (51, 12, N'Exercícios de segurança', N'exercicio', N'1. Classifique incidentes de exemplo conforme o pilar da tríade CID que foi violado.

2. Reescreva uma consulta vulnerável a injeção de SQL usando parâmetros.

3. Explique por que o sal impede o uso de tabelas pré-calculadas de hashes.

4. Identifique, em uma rota de API de exemplo, onde há risco de controle de acesso quebrado.

5. Liste três dados pessoais que uma plataforma de cursos coleta e a finalidade de cada um, segundo a LGPD.'),
        (52, 13, N'Complexidade de algoritmos e notação Big-O', N'texto', N'A notação Big-O descreve como o custo de um algoritmo cresce quando a entrada cresce. Ela ignora constantes e foca na tendência: um algoritmo que faz 3n + 10 operações é O(n).

As classes mais comuns, da mais rápida para a mais lenta: O(1) constante (acessar um índice de array), O(log n) logarítmica (busca binária), O(n) linear (percorrer uma lista), O(n log n) (bons algoritmos de ordenação), O(n²) quadrática (dois laços aninhados sobre a mesma entrada) e O(2^n) exponencial.

A diferença só aparece com volume. Para 10 itens, O(n²) faz 100 operações, o que é irrelevante. Para 1 milhão de itens, faz 10¹² operações, enquanto O(n log n) faz cerca de 20 milhões. Escolher a estrutura de dados certa costuma mudar a classe de complexidade, e isso pesa mais do que qualquer micro-otimização.'),
        (53, 13, N'Pilhas, filas e listas encadeadas', N'texto', N'Pilha (stack) segue LIFO: o último a entrar é o primeiro a sair. As operações são push (empilhar) e pop (desempilhar), ambas O(1). O botão "voltar" do navegador e a pilha de chamadas de funções são pilhas.

Fila (queue) segue FIFO: o primeiro a entrar é o primeiro a sair. Enqueue insere no fim, dequeue remove do início. Filas modelam atendimento, impressão e processamento de mensagens.

Lista encadeada guarda cada elemento em um nó que aponta para o próximo. Inserir ou remover no início é O(1), porque basta trocar ponteiros, mas acessar o elemento da posição k é O(n), porque é preciso caminhar nó a nó. No array é o contrário: acesso por índice O(1) e inserção no meio O(n).

Numa pilha encadeada, o push cria um nó novo, faz esse nó apontar para o topo atual e torna o nó novo o topo: novo.prox = topo; topo = novo. Assim o empilhamento é O(1), sem percorrer a lista. Para acompanhar operações misturadas de pilha e fila, vale desenhar as duas estruturas e atualizá-las a cada linha: top() lê o topo sem remover; pop() remove; enqueue() insere no fim da fila; dequeue() remove do início.'),
        (54, 13, N'Árvores, ordenação e busca', N'texto', N'Uma árvore binária de busca (BST) guarda, em cada nó, valores menores à esquerda e maiores à direita. Buscar, inserir e remover custam O(log n) quando a árvore está balanceada, mas degradam para O(n) se os dados chegarem já ordenados e a árvore virar uma "lista". Árvores AVL e rubro-negras se rebalanceiam para evitar isso.

A busca binária, em um array ordenado, compara o alvo com o elemento do meio e descarta metade do intervalo a cada passo: O(log n).

Ordenação: bubble sort e insertion sort são O(n²) no pior caso; merge sort é O(n log n) sempre, ao custo de memória extra; quicksort é O(n log n) em média e O(n²) no pior caso, mas costuma ser o mais rápido na prática por usar bem a memória cache.

Uma árvore binária pode ser percorrida em três ordens clássicas, todas recursivas: pré-ordem (visita a raiz, depois a subárvore esquerda, depois a direita), em ordem (esquerda, raiz, direita; numa BST, devolve os valores ordenados) e pós-ordem (esquerda, direita, raiz). Recursão é uma função que chama a si mesma até atingir uma condição de parada. A versão recursiva de Fibonacci é elegante, mas ineficiente: cada chamada gera outras duas, e o número de chamadas explode. Na ordenação por inserção, cada elemento é comparado com os anteriores, da direita para a esquerda, e os maiores são deslocados uma posição (vetor[j+1] <- vetor[j]) até abrir espaço para ele.'),
        (55, 13, N'Lista de problemas de algoritmos', N'exercicio', N'1. Implemente uma pilha usando um array e verifique se uma expressão tem parênteses balanceados.

2. Simule uma fila de atendimento com prioridade para idosos usando duas filas.

3. Implemente a busca binária de forma iterativa e de forma recursiva.

4. Insira os valores 50, 30, 70, 20, 40, 60, 80 em uma BST e escreva o percurso em ordem.

5. Ordene um vetor com merge sort e conte quantas comparações foram feitas.

6. Para cada item acima, informe a complexidade de tempo no pior caso.'),
        (56, 14, N'Componentes e JSX', N'texto', N'Em React, a interface é dividida em componentes: funções que recebem dados e devolvem o que deve aparecer na tela. JSX é a sintaxe que mistura marcação e JavaScript; ela é convertida em chamadas de função antes de chegar ao navegador.

    function Saudacao({ nome }) {
        return <h1>Olá, {nome}!</h1>;
    }

Expressões JavaScript entram entre chaves. Como class é palavra reservada em JavaScript, no JSX o atributo de classe CSS é className. Um componente precisa devolver um único elemento raiz; para agrupar sem criar uma div extra, use o fragmento <>...</>.

O nome de um componente começa com letra maiúscula: é assim que o React diferencia componentes próprios de tags HTML.'),
        (57, 14, N'Props e estado', N'texto', N'Props são os dados que o componente pai passa ao filho. Elas são somente leitura: o filho nunca altera as props que recebe.

Estado é o dado que o próprio componente controla e que muda com o tempo. O hook useState devolve o valor atual e uma função para alterá-lo:

    const [contador, setContador] = useState(0);

Chamar setContador agenda uma nova renderização com o valor novo. Alterar a variável diretamente (contador = 5) não atualiza a tela, porque o React não fica sabendo da mudança.

O fluxo de dados é de cima para baixo. Quando dois componentes irmãos precisam do mesmo dado, o estado sobe para o pai comum, que o repassa por props. Ao renderizar listas com map, cada item precisa de uma key única e estável, para o React saber qual item mudou.'),
        (58, 14, N'Efeitos e consumo de API', N'texto', N'useEffect executa código depois da renderização, e é o lugar de efeitos colaterais: buscar dados, assinar eventos, configurar temporizadores.

    useEffect(() => {
        fetch("/api/cursos")
            .then(r => r.json())
            .then(setCursos);
    }, []);

O segundo argumento é a lista de dependências. Com um array vazio, o efeito roda só na montagem do componente. Com [idCurso], roda de novo sempre que idCurso mudar. Sem o array, roda após toda renderização, o que costuma gerar laços de requisições.

Se o efeito cria algo que precisa ser desfeito, como um intervalo, ele devolve uma função de limpeza, que o React chama antes de desmontar o componente.'),
        (59, 14, N'Exercícios de React', N'exercicio', N'1. Crie um componente CartaoCurso que receba nome e nível por props.

2. Faça um contador com botões de somar e subtrair usando useState.

3. Liste cursos vindos de um array com map, usando o id como key.

4. Busque dados de uma API com useEffect e mostre "Carregando..." enquanto a resposta não chega.

5. Explique o que acontece se o array de dependências do useEffect for omitido.'),
        (60, 15, N'JOINs e subconsultas', N'texto', N'INNER JOIN devolve só as linhas que têm correspondência nas duas tabelas. LEFT JOIN devolve todas as linhas da tabela da esquerda e preenche com NULL o que não tiver par à direita; é o JOIN para perguntas como "alunos que ainda não fizeram nenhuma prova", combinado com WHERE d.ID_Desempenho IS NULL.

Subconsultas podem aparecer no WHERE (com IN ou EXISTS), no FROM (como tabela derivada) ou no SELECT (como valor escalar). EXISTS para assim que encontra a primeira linha, e costuma ser a forma mais clara de perguntar "existe algum?".

    SELECT u.Nome_Usuario
    FROM Estudante e
    JOIN Usuario u ON u.ID_Usuario = e.ID_Usuario
    WHERE NOT EXISTS (SELECT 1 FROM Historico h
                      WHERE h.ID_Estudante = e.ID_Estudante);');
    INSERT INTO Material (ID_Material, ID_Curso, Titulo_Material, Tipo_Material, Conteudo) VALUES
        (61, 15, N'Agregações e funções de janela', N'texto', N'GROUP BY agrupa linhas e permite funções de agregação como COUNT, SUM, AVG, MIN e MAX. WHERE filtra linhas antes do agrupamento; HAVING filtra grupos depois dele.

Funções de janela calculam sobre um conjunto de linhas relacionadas sem colapsá-las em uma só. Com OVER, cada linha mantém seus dados e ganha o valor calculado:

    SELECT ID_Estudante, Nota,
           RANK() OVER (ORDER BY Nota DESC) AS Posicao,
           AVG(Nota) OVER (PARTITION BY ID_Prova) AS MediaDaProva
    FROM Desempenho;

PARTITION BY divide as linhas em grupos para o cálculo, como um GROUP BY que não junta as linhas. ROW_NUMBER numera sem empates; RANK repete a posição em empates e pula a seguinte; DENSE_RANK repete sem pular.

Quando a consulta mistura colunas e agregações, toda coluna do SELECT que não está dentro de uma função de agregação precisa estar no GROUP BY, e na mesma forma: se o SELECT mostra YEAR(dt_pedido), o agrupamento é por YEAR(dt_pedido), e não pela data inteira, senão cada dia vira um grupo. COUNT(DISTINCT id_pedido) conta pedidos sem repetir quando o JOIN com os itens multiplica as linhas, e ORDER BY AVG(...) DESC ordena pelo valor agregado.'),
        (62, 15, N'Índices e plano de execução', N'texto', N'Um índice é uma estrutura ordenada (uma árvore B+) que permite encontrar linhas sem ler a tabela inteira. O índice clusterizado define a ordem física das linhas, por isso só existe um por tabela; no SQL Server, a chave primária cria um por padrão. Índices não clusterizados são estruturas separadas que apontam para as linhas.

Índices aceleram leituras e encarecem escritas, porque cada INSERT, UPDATE e DELETE também atualiza os índices. Criar índice para toda coluna é tão ruim quanto não criar nenhum.

O plano de execução mostra como o banco resolveu a consulta. Um Table Scan ou Clustered Index Scan em tabela grande, quando a consulta filtra poucas linhas, é sinal de índice faltando. Funções aplicadas à coluna no WHERE, como YEAR(Data_Realizacao) = 2026, impedem o uso do índice; prefira um intervalo: Data_Realizacao >= ''2026-01-01'' AND Data_Realizacao < ''2027-01-01''.'),
        (63, 15, N'Laboratório de consultas', N'exercicio', N'Use o banco do Tech Quest como base.

1. Liste os estudantes que se matricularam em algum curso e nunca fizeram prova.

2. Mostre, para cada prova, a média das notas e a quantidade de tentativas.

3. Use RANK para montar o ranking dos estudantes pela maior nota em cada prova.

4. Encontre os cursos com mais de 5 alunos concluintes usando HAVING.

5. Compare o plano de execução de uma busca por e-mail antes e depois de criar um índice na coluna.');
    SET IDENTITY_INSERT Material OFF;

    -- ---------------------------------------------------------
    -- 5. Questoes (10 por prova) e alternativas (exatamente 1 correta)
    -- ---------------------------------------------------------
    SET IDENTITY_INSERT Questao ON;
    INSERT INTO Questao (ID_Questao, ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao) VALUES
        (1, 1, N'Considere o código abaixo. Qual será a saída ao executá-lo?', N'List<int> numeros = new List<int> { 5, 10, 15, 20, 25 };
int soma = 0;
foreach (int n in numeros) {
    if (n > 10) soma += n;
}
Console.WriteLine(soma);', 1),
        (2, 1, N'Qual a diferença fundamental entre Array e List<T> em C#?', NULL, 2),
        (3, 1, N'Qual método é utilizado para adicionar um elemento ao final de uma List<T> em C#?', NULL, 3),
        (4, 1, N'Em C#, qual a forma correta de declarar um array de strings com 5 posições?', NULL, 4),
        (5, 1, N'(Enade 2021) Leia o texto a seguir.

Um algoritmo é qualquer procedimento computacional bem definido que toma algum valor ou conjunto de valores como entrada e produz algum valor ou conjunto de valores como saída.

CORMEN, T. H. et al. Algoritmos: teoria e prática. Rio de Janeiro: Elsevier, 2002 (com adaptações).

Considere o algoritmo abaixo. No fim da execução do código apresentado, será exibido o valor', N'Algoritmo Calcular
var
    mat [1..3][1..5] de inteiro =
        {{1, 2, -1, 2, 3}, {1, -3, 4, 2, 0}, {-3, 5, 2, 3, 4}}
    sl[1..3] de inteiro = {0, 0, 0}
    x, i, j : inteiro
    x <- 0
início
    para i <- 1 até 3 faça
        para j <- 1 até 5 faça
            sl[i] <- sl[i] + mat[i][j]
        fimpara
        x <- x + sl[i]
    fimpara
    imprima x
fim', 5),
        (6, 1, N'Qual instrução em C# percorre todos os elementos de uma coleção sem precisar de índice numérico?', NULL, 6),
        (7, 1, N'Qual namespace deve ser importado para usar List<T> em C#?', NULL, 7),
        (8, 1, N'(Enade 2011) O paradigma de programação orientado a objetos tem sido largamente utilizado no desenvolvimento de sistemas. Considerando o conceito de herança, avalie as afirmativas.

I. Herança é uma propriedade que facilita a implementação de reuso.

II. Quando uma subclasse é criada, ela herda todas as características da superclasse, não podendo possuir propriedades e métodos próprios.

III. Herança múltipla é uma propriedade na qual uma superclasse possui diversas subclasses.

IV. Extensão é uma das formas de se implementar herança.

É correto apenas o que se afirma em', NULL, 8),
        (9, 1, N'Em C#, qual método remove o primeiro elemento que satisfaz a condição em uma List<T>?', NULL, 9),
        (10, 1, N'Qual a forma mais idiomática (recomendada) em C# para verificar se uma lista está vazia?', NULL, 10),
        (11, 2, N'(Enade 2011) O levantamento de requisitos é uma etapa fundamental do projeto de sistemas. Dependendo da situação encontrada, uma ou mais técnicas podem ser utilizadas para a elicitação dos requisitos. A respeito dessas técnicas, avalie as afirmativas.

I. Workshop de requisitos consiste na realização de reuniões estruturadas e delimitadas entre os analistas de requisitos do projeto e representantes do cliente.

II. Cenário consiste na observação das ações do funcionário na realização de uma determinada tarefa, para verificar os passos necessários para sua conclusão.

III. As entrevistas são realizadas com os stakeholders e podem ser abertas ou fechadas.

IV. A prototipagem é uma versão inicial do sistema, baseado em requisitos levantados em outros sistemas da organização.

É correto apenas o que se afirma em', NULL, 1),
        (12, 2, N'(Enade 2008) Um analista foi contratado para desenvolver um sistema de pesquisa de DVDs em lojas virtuais. O sistema deverá solicitar ao usuário um título de DVD, que será usado para realizar a pesquisa nas bases de dados das lojas conveniadas. Ao detectar a disponibilidade do DVD solicitado, o sistema armazenará temporariamente os dados das lojas (nome, preço, data prevista para entrega do produto) e exibirá as informações ordenadas por preço. Após analisar as informações, o cliente poderá efetuar a compra. O contratante deverá testar algumas operações do sistema antes de ele ser finalizado. Há tempo suficiente para que o analista atenda a essa solicitação e efetue eventuais modificações exigidas pelo contratante. Com relação a essa situação, julgue os itens a seguir quanto ao modelo de ciclo de vida.

I. O entendimento do sistema como um todo e a execução sequencial das fases sem retorno produzem um sistema que pode ser validado pelo contratante.

II. A elaboração do protótipo pode ser utilizada para resolver dúvidas de comunicação, o que aumenta os riscos de inclusão de novas funcionalidades não prioritárias.

III. A definição das restrições deve ser a segunda fase a ser realizada no desenvolvimento do projeto, correspondendo à etapa de engenharia.

IV. Um processo iterativo permite que versões progressivas mais completas do sistema sejam construídas e avaliadas.

São corretos apenas os itens', NULL, 2),
        (13, 2, N'(Enade 2008) Com relação à forma como o RUP trata a análise de requisitos, assinale a opção correta.', NULL, 3),
        (14, 2, N'(Enade 2008) Os alunos de uma disciplina deveriam escolher um sistema de média complexidade, contendo no mínimo 100 funcionalidades, para ser modelado em UML e codificado em uma linguagem orientada a objetos. Um dos grupos de alunos estabeleceu a seguinte estratégia para identificação e seleção do sistema.

I. Cada integrante do grupo deveria criar um nickname (apelido) em um software de chat.

II. O grupo deveria se reunir em um horário predeterminado.

III. Durante o chat, os seguintes procedimentos deveriam ser realizados:

• cada integrante deveria sugerir um ou mais sistemas e justificar sua escolha, e não poderia criticar as ideias dos outros;

• à medida que as ideias fossem digitadas, o líder deveria copiá-las para um editor de texto e controlar o tempo de sugestão;

• quando o limite de tempo fosse atingido, o líder disponibilizaria todas as sugestões para serem analisadas pelo grupo;

• as 5 melhores ideias seriam selecionadas e colocadas em votação para a escolha da melhor ideia, segundo critérios predefinidos.

Nessa situação, a estratégia utilizada pelo grupo de alunos é uma adaptação da técnica de levantamento e elicitação de requisitos denominada', NULL, 4),
        (15, 2, N'(Enade 2014) Leia o texto a seguir.

O modelo relacional representa o banco de dados como uma coleção de relações (tabelas). Na terminologia formal do modelo relacional, uma linha é chamada de "tupla", o título da coluna é denominado "atributo" e a tabela é chamada de "relação". O tipo de dado que descreve os tipos de valores que podem aparecer em cada coluna é denominado "domínio". Um banco de dados relacional pode impor vários tipos de restrições nos dados armazenados.

ELMASRI, R. NAVATHE, S. B. Sistema de Banco de Dados: Fundamentos e Aplicações. Rio de Janeiro: LTC, 2002.

Restrições que permitem controlar situações como, por exemplo, "o salário de um empregado não deve exceder o salário do supervisor do empregado" e utilizam mecanismos chamados triggers (gatilhos) na sua implementação, são do tipo', NULL, 5),
        (16, 2, N'Em modelagem de banco de dados relacional, o que é uma chave primária?', NULL, 6),
        (17, 2, N'Qual cláusula SQL é utilizada para filtrar registros baseados em uma condição?', NULL, 7),
        (18, 2, N'No modelo Entidade-Relacionamento (ER), o que representa um relacionamento N:M (muitos para muitos)?', NULL, 8),
        (19, 2, N'Em qual forma normal está uma tabela que já está na 2FN e na qual nenhuma coluna depende de outra coluna que não seja a chave?', NULL, 9),
        (20, 2, N'Qual comando SQL pertence à DDL (Data Definition Language)?', NULL, 10),
        (21, 3, N'Qual é a saída do código abaixo?', N'print(7 // 2)', 1),
        (22, 3, N'Qual é o tipo do valor devolvido pela função input() em Python?', NULL, 2),
        (23, 3, N'Quais números são gerados por range(2, 6)?', NULL, 3),
        (24, 3, N'Qual expressão verifica corretamente se a variável n é par?', NULL, 4),
        (25, 3, N'(Enade 2014) José discutindo futebol com João declarou que se o time X venceu o último jogo, então o time Y é campeão. João depois de algumas horas descobriu que a declaração era falsa. Em consequência, teve certeza de que é verdade que', NULL, 5),
        (26, 3, N'Qual é a saída do código abaixo?', N'x = 10
if x > 5:
    print("A")
elif x > 8:
    print("B")
else:
    print("C")', 6),
        (27, 3, N'O que uma função devolve quando não possui a instrução return?', NULL, 7),
        (28, 3, N'Quantas vezes a palavra "oi" será impressa?', N'i = 0
while i < 3:
    print("oi")
    i += 1', 8),
        (29, 3, N'(Enade 2017) A álgebra booleana apresenta um operador unário ~, conhecido como NÃO, e os operadores binários * e +, conhecidos como E e OU, respectivamente. A tabela verdade é utilizada para validar uma fórmula composta de operadores da álgebra booleana. A seguir, é apresentada a tabela verdade para as proposições p, q e r diante da fórmula G, em que V representa uma proposição verdadeira e F uma proposição falsa.

Considerando as informações contidas na tabela e a ordem de precedência dos operadores booleanos, a fórmula G apresenta a mesma tabela verdade da proposição', N'p  q  r  |  G
V  V  V  |  V
V  V  F  |  F
V  F  V  |  F
V  F  F  |  F
F  V  V  |  V
F  V  F  |  V
F  F  V  |  V
F  F  F  |  V', 9),
        (30, 3, N'Qual afirmação descreve a tipagem de Python?', NULL, 10);
    INSERT INTO Questao (ID_Questao, ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao) VALUES
        (31, 4, N'Qual tag HTML representa o conteúdo principal e único de uma página?', NULL, 1),
        (32, 4, N'No box model, qual propriedade define o espaço interno entre o conteúdo e a borda?', NULL, 2),
        (33, 4, N'O que box-sizing: border-box altera?', NULL, 3),
        (34, 4, N'Qual seletor tem maior especificidade?', NULL, 4),
        (35, 4, N'Em mobile-first, que tipo de media query é usado para acrescentar regras de telas maiores?', NULL, 5),
        (36, 4, N'Qual propriedade do Flexbox alinha os itens no eixo principal?', NULL, 6),
        (37, 4, N'O que a declaração abaixo produz?', N'.grade {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
}', 7),
        (38, 4, N'(Enade 2017) Em um sistema web para venda de ingressos para shows musicais, a equipe de marketing solicitou a um web designer que, na página inicial, os links para a área de venda de ingressos de cada show fossem imagens, para tornar a navegação pelos conteúdos da página mais atrativa e para que o próprio link já servisse como material de divulgação.

Usando HTML, o web designer optou por implementar cada link de forma que a imagem forneça um texto informativo (por meio do atributo alt) para usuários que usem leitores de tela, conforme o código abaixo.

Considerando a situação apresentada, o uso do atributo alt e aspectos da interação humano-computador, pode-se afirmar que o web designer atendeu, em sua implementação, o requisito de', N'<a href="link-area-venda-ingresso">
    <img src="imagem-show.jpg" alt="identificação do show" />
</a>', 8),
        (39, 4, N'Sem a meta tag viewport, o que acontece no navegador do celular?', NULL, 9),
        (40, 4, N'Qual é a diferença essencial entre Flexbox e Grid?', NULL, 10),
        (41, 5, N'Qual comando envia mudanças para a área de preparação (staging)?', NULL, 1),
        (42, 5, N'Qual comando cria uma nova branch e já muda para ela?', NULL, 2),
        (43, 5, N'O que acontece quando duas branches alteram a mesma linha de um arquivo e são unidas?', NULL, 3),
        (44, 5, N'Para que serve o arquivo .gitignore?', NULL, 4),
        (45, 5, N'Qual comando baixa e integra as mudanças do repositório remoto?', NULL, 5),
        (46, 5, N'O que é um pull request?', NULL, 6),
        (47, 5, N'(Enade 2017) Um desenvolvedor de software recém-formado foi contratado para a implementação de um projeto em uma empresa e, em reunião, recebeu várias explicações sobre como a gerência de configuração funcionava. Considerando essa situação, avalie as afirmativas, referentes às informações dadas ao desenvolvedor.

I. Inicialmente, para ter acesso à base de desenvolvimento, o profissional deve realizar uma operação de checkout para baixar os arquivos do projeto que estão armazenados no servidor.

II. Na situação em que mais de um desenvolvedor estiver modificando um mesmo documento, ao se tentar realizar uma operação de commit, pode ser necessário realizar uma operação de tag(release) para resolução do conflito entre a versão local e a versão mais recente no repositório, caso algum desenvolvedor tenha submetido uma mudança no documento previamente.

III. No desenvolvimento de um novo caso de uso, em que diversos arquivos sejam modificados, é recomendada a criação de uma ramificação (branch).

IV. A versão estável é o ramo principal de desenvolvimento, que segue do começo do desenvolvimento até o momento presente.

É correto apenas o que se afirma em', NULL, 7),
        (48, 5, N'Qual é o risco de fazer rebase de commits já compartilhados?', NULL, 8),
        (49, 5, N'(Enade 2021) Um ponto importante para fábricas de software é a garantia de que seus produtos e/ou serviços acompanhem as diversas mudanças ocasionadas pelo mundo corporativo. As restrições de orçamento ou cronograma, reestruturação do negócio, modificações de regras de negócio ou novas necessidades do cliente, geram mudanças inevitáveis. Nesse cenário, a utilização de um processo de Gerência de Configuração de Software (GCS) que contemple características necessárias para gerenciar e controlar a evolução de um software, por meio do controle formal de versão e solicitação de mudanças, é extremamente importante. Sobre o processo de Gerência de Configuração de Software (GCS), avalie as afirmativas.

I. O controle de versão possibilita o compartilhamento de dados e a edição colaborativa, permitindo a gestão de diferentes ramos de desenvolvimento e possibilitando a existência de diferentes versões de forma simultânea.

II. A auditoria de configuração é uma atividade de garantia de qualidade do sistema que tem por objetivo assegurar que a qualidade do software seja mantida quando feitas alterações requisitadas.

III. O relatório de status de configuração possui informações sobre o GCS, tendo por objetivo manter o cliente informado sobre as alterações, e deve ser gerado ao final do processo.

IV. A GCS proporciona um conjunto de atividades de acompanhamento e controle de mudanças que é iniciado logo depois que o software for fornecido ao cliente e colocado em operação.

É correto apenas o que se afirma em', NULL, 9),
        (50, 5, N'(Enade 2014) A gerência de configuração de software, também chamada de gerência de mudança e configuração é o processo que mantém atualizadas as informações dos elementos de configuração. A esse respeito, avalie as afirmativas.

I. A auditoria de software é um processo que consiste em verificar se a linha de base foi atendida ou se os requisitos do sistema foram atendidos.

II. A linha de base é o marco de referência, a partir do qual serão feitos os controles de mudança, e pode ser a versão 1.0 de um software.

III. Um item de configuração é um elemento unitário, que compõe aquele software e deve ser gerenciado.

É correto o que se afirma em', NULL, 10),
        (51, 6, N'(Enade 2017) Leia o texto a seguir.

A engenharia de requisitos, do ponto de vista do processo de software, é uma ação de engenharia de software importante, que se inicia durante a atividade de comunicação e continua na de modelagem. Ela deve ser adaptada às necessidades do processo, do projeto, do produto e das pessoas que estão realizando o trabalho.

PRESSMAN, R. S. Engenharia de software: uma abordagem profissional. 8. ed. Porto Alegre: AMGH, 2016 (com adaptações).

Considere os requisitos, a seguir, de um sistema para uma universidade, na qual se pretenda gerenciar o setor acadêmico.

● R1: o sistema deve permitir que cada professor realize o lançamento de notas das turmas nas quais lecionou;

● R2: o sistema deverá ser desenvolvido de forma a possibilitar seu transporte para outro sistema operacional em, no máximo, sessenta dias;

● R3: o sistema deve permitir que um estudante realize a sua matrícula nas disciplinas oferecidas em semestre letivo;

● R4: o sistema atualiza a nota do estudante, permitindo sua visualização, em até dois segundos depois do momento que o professor a registra;

● R5: o sistema deve permitir que o auxiliar de serviços acadêmicos realize o cadastro de um estudante em não mais do que dez minutos de orientação.

Nessa situação, representam descrições de requisitos não funcionais apenas os requisitos', NULL, 1),
        (52, 6, N'Qual característica torna um requisito bem escrito?', NULL, 2),
        (53, 6, N'No diagrama de casos de uso, o que representa um ator?', NULL, 3),
        (54, 6, N'(Enade 2014) Leia o texto a seguir.

Casos de uso podem ser organizados agrupando-os em pacotes do mesmo modo como são organizadas as classes. Também podem ser organizados pela especificação de relacionamentos de generalização, inclusão e extensão, existentes entre eles.

JACOBSON, I.; BOOCH, G.; RUMBAUGH, J. UML - Guia do Usuário. Campus, 2006 (com adaptações).

Considerando os relacionamentos existentes entre os casos de uso, avalie as afirmativas a seguir.

I. Para casos de uso, a generalização significa que o caso de uso filho herda o comportamento e o significado do caso de uso pai e no caso de uso filho deverá acrescentar ou sobrescrever o comportamento de seu pai.

II. Um relacionamento de inclusão entre casos de uso significa que o caso de uso base incorpora explicitamente o comportamento de outro caso de uso em uma localização especificada. O caso de uso base poderá permanecer isolado, mas, sob certas condições, seu comportamento poderá ser incluído pelo comportamento de outro caso de uso.

III. Um relacionamento estendido entre casos de uso significa que o caso de uso base incorpora implicitamente o comportamento de outro caso de uso em um local especificado indiretamente pelo caso de uso estendido. O caso de uso estendido nunca permanece isolado, mas é apenas instanciado como parte de alguma base maior que o estende.

IV. Um relacionamento estendido é utilizado para a modelagem de parte de um caso de uso que o usuário poderá considerar como um comportamento opcional do sistema e para a modelagem de um subfluxo separado, que é executado somente sob determinadas condições.

É correto apenas o que se afirma em', NULL, 4),
        (55, 6, N'Qual relacionamento indica que a parte deixa de existir quando o todo é destruído?', NULL, 5),
        (56, 6, N'Qual diagrama é mais indicado para mostrar a troca de mensagens de um fluxo ao longo do tempo?', NULL, 6),
        (57, 6, N'O que a multiplicidade 1..* indica em uma associação?', NULL, 7),
        (58, 6, N'"O aluno é aprovado com nota maior ou igual à mínima da prova" é melhor classificado como:', NULL, 8),
        (59, 6, N'(Enade 2021) Leia o texto a seguir.

A engenharia de requisitos é uma área que inclui quatro subprocessos relacionados de alto nível. Esses subprocessos são: 1) avaliação se o sistema será útil para a empresa (estudo de viabilidade); 2) obtenção de requisitos (elicitação de requisitos); 3) conversão desses requisitos em alguma forma padrão (especificação); 4) verificação se os requisitos realmente definem o sistema que o cliente deseja (validação).

SOMMERVILLE, I. Engenharia de Software. São Paulo: Pearson Addison-Wesley, 2017 (com adaptações).

Uma equipe de Tecnologia da Informação (TI) de uma empresa de consultoria desenvolverá um software de Suporte Técnico para uma grande empresa fornecedora de equipamentos eletrônicos. O estudo de viabilidade do software já foi realizado e aprovado. A equipe de Tecnologia da Informação seguirá os três subprocessos seguintes de alto nível de engenharia de requisitos descritos no texto de Sommerville, ou seja, os subprocessos de elicitação de requisitos, especificação e validação. Para esses três subprocessos, quais são os artefatos que podem ser utilizados por essa equipe de Tecnologia da Informação?', NULL, 9),
        (60, 6, N'(Enade 2021) Leia o texto a seguir.

A etapa de definição de requisitos é voltada para estabelecer quais as funções são requeridas pelo sistema e as restrições sobre a operação e o desenvolvimento do software. Os requisitos de software podem ser classificados como requisitos funcionais e não funcionais.

SOMMERVILLE, I. Engenharia de Software, 10. ed. São Paulo: Pearson Education, 2019 (com adaptações).

Considerando as informações do texto, assinale a alternativa em que o item é um requisito funcional.', NULL, 10);
    INSERT INTO Questao (ID_Questao, ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao) VALUES
        (61, 7, N'(Enade 2017) Leia o texto a seguir.

Nas organizações, as redes de computadores e os equipamentos são soluções tecnológicas planejadas e construídas com base em padrões. Um dos padrões responsáveis pela comunicação e pelo endereçamento em redes é o TCP/IP (Transmission Control Protocol/Internet Protocol), que permite a comunicação entre diferentes computadores, sistemas operacionais e aplicativos que aceitam esse protocolo.

SOUSA, L. B. Projetos e implementação de redes: fundamentos, soluções, arquitetura e planejamento. São Paulo: Erica, 2013 (com adaptações).

Com base no protocolo de comunicação de dados em rede TCP/IP, mencionado no texto acima, avalie as afirmativas.

I. Os programas que controlam a comunicação dos dados entre equipamentos e aplicativos em redes são chamados de protocolos de armazenamento de dados.

II. Dois sistemas operacionais com plataforma de hardware distintas comunicam-se entre si por meio de um protocolo comum, o TCP/IP.

III. O nome TCP/IP se refere a dois protocolos: o TCP, protocolo responsável pelo controle e qualidade da comunicação entre a origem e o destino, e o IP, protocolo responsável pelo endereçamento nas redes.

É correto o que se afirma em', NULL, 1),
        (62, 7, N'Qual protocolo de transporte garante entrega e ordem dos dados?', NULL, 2),
        (63, 7, N'Quantos hosts utilizáveis existem em uma rede /24?', NULL, 3),
        (64, 7, N'Qual destes endereços pertence a uma faixa privada?', NULL, 4),
        (65, 7, N'(Enade 2014) Leia o texto a seguir.

O serviço DNS (Domain Name System) traduz nomes alfanuméricos de hosts em endereços numéricos, de acordo com o protocolo IP (Internet Protocol). Essa ação é comumente chamada de resolução de endereço.

TANENBAUM, A. S. Redes de Computadores. Rio de Janeiro: Campus, 2003 (com adaptações).

Considere um conjunto de computadores conectados em uma rede local, os quais têm à sua disposição um servidor DNS capaz de resolver endereços, sejam eles internos ou externos. Nesse contexto, avalie as afirmativas a seguir.

I. O servidor DNS também executa funções de cliente DNS quando não é autoritativo para determinado endereço.

II. A adoção do IPv6 (Internet Protocol, versão 6) dispensará serviços de DNS, pois suas funções serão incorporadas pelo próprio protocolo IP.

III. O cache DNS permite que determinada requisição do cliente DNS possa ser resolvida sem que seja necessário recorrer a outro serviço DNS.

IV. O protocolo DNS depende de um banco de dados distribuído.

É correto apenas o que se afirma em', NULL, 5),
        (66, 7, N'Qual código de status HTTP indica que o recurso não foi encontrado?', NULL, 6),
        (67, 7, N'(Enade 2008) As comunicações de um software cliente com o respectivo servidor instalado em um equipamento em uma LAN (local area network) apresentam lentidão. Após várias reclamações dos usuários, o responsável pelo software e o gerente de redes foram convocados para analisar o problema. A partir de uma análise da topologia da rede, foi identificado que o departamento possuía 45 microcomputadores clientes na mesma LAN, interligados ao servidor por meio de um único hub. Concluiu-se que o problema de lentidão estava relacionado ao elevado número de colisões na LAN.

A solução correta para esse problema hipotético é a substituição do hub por', NULL, 7),
        (68, 7, N'(Enade 2021) Suponha que sua faculdade tendo, em sua rede de computadores, máquinas que compartilham arquivos e serviços, contratou você para uma consultoria de TI. Atualmente, na sala dos professores, existem 3 máquinas ligadas à rede, com as configurações mostradas no quadro abaixo. Porém, a estação de trabalho "PROFESSORES01" não consegue enviar documentos para impressão em uma impressora, corretamente instalada e compartilhada na estação de trabalho "PROFESSORES03".

Nesse caso, que alteração deve ser realizada para que o problema seja resolvido?', N'PROFESSORES01  IP: 175.16.10.200  Máscara: 255.255.255.0  Gateway: 175.16.1.1
PROFESSORES02  IP: 175.16.20.200  Máscara: 255.255.0.0    Gateway: 175.16.1.1
PROFESSORES03  IP: 175.16.30.200  Máscara: 255.255.0.0    Gateway: 175.16.1.1', 8),
        (69, 7, N'O que o NAT permite em uma rede doméstica?', NULL, 9),
        (70, 7, N'Qual é a porta padrão do HTTPS?', NULL, 10),
        (71, 8, N'Qual palavra-chave cria uma nova instância de uma classe em Java?', NULL, 1),
        (72, 8, N'(Enade 2008) Com relação a conceitos de orientação a objetos, julgue os itens.

I. As variáveis ou métodos declarados com modificador de acesso private só são acessíveis a métodos da classe em que são declarados.

II. Uma classe deve possuir uma única declaração de método construtor.

III. Uma instância de uma classe abstrata herda atributos e métodos de sua superclasse direta.

IV. O polimorfismo permite substituir a lógica condicional múltipla (lógica switch ou faça caso).

São corretos apenas os itens', NULL, 2),
        (73, 8, N'Quantas classes uma classe Java pode estender diretamente?', NULL, 3),
        (74, 8, N'(Enade 2017) A área de desenvolvimento de software está se tornando cada vez mais complexa. Para lidar com essa realidade, os desenvolvedores contam com linguagens de programação baseadas no paradigma de orientação a objetos, cujos pilares são abstração, encapsulamento, herança e polimorfismo. No código a seguir, observa-se a implementação de classes relacionadas.

Com base nas informações do texto e no código apresentado, avalie as afirmativas a seguir.

I. A execução do código, via classe Main, resulta na seguinte saída: "Imprimindo na Laser", "Imprimindo na Matricial", "Imprimindo na Jato de tinta".

II. O código faz uso da técnica denominada polimorfismo.

III. O código não será compilado, pois o vetor imp foi instanciado por meio da classe abstrata Impressora.

É correto o que se afirma em', N'public abstract class Impressora {
    String nome;
    Impressora() {}
    Impressora(String n) {
        this.nome = n;
    }
    public void imprimir() {}
}

public class Laser extends Impressora {
    public Laser() {}
    public void imprimir() {
        System.out.println("Imprimindo na Laser");
    }
}

class Matricial extends Impressora {
    public Matricial() {}
    public void imprimir() {
        System.out.println("Imprimindo na Matricial");
    }
}

public class JatoDeTinta extends Impressora {
    public JatoDeTinta() {}
    public void imprimir() {
        System.out.println("Imprimindo na Jato de tinta");
    }
}

public class Main {
    public static void main(String args[]) {
        Impressora imp[] = new Impressora[3];
        imp[0] = new Laser();
        imp[1] = new JatoDeTinta();
        imp[2] = new Matricial();
        for (int i = imp.length - 1; i >= 0; i--) {
            imp[i].imprimir();
        }
    }
}', 4),
        (75, 8, N'Qual é a principal diferença entre interface e classe abstrata?', NULL, 5),
        (76, 8, N'O que o código abaixo imprime?', N'class Animal { String som() { return "..."; } }
class Gato extends Animal {
    @Override String som() { return "Miau"; }
}
Animal a = new Gato();
System.out.println(a.som());', 6),
        (77, 8, N'Métodos com o mesmo nome e parâmetros diferentes na mesma classe caracterizam:', NULL, 7),
        (78, 8, N'Qual é a função de um construtor?', NULL, 8),
        (79, 8, N'Por que um setter que aceita qualquer valor não garante encapsulamento?', NULL, 9),
        (80, 8, N'Qual anotação indica que um método sobrescreve um método da superclasse?', NULL, 10),
        (81, 9, N'Qual é o resultado de "5" === 5 em JavaScript?', NULL, 1),
        (82, 9, N'Qual palavra-chave declara uma variável que não pode ser reatribuída?', NULL, 2),
        (83, 9, N'O que o código abaixo imprime?', N'const nums = [1, 2, 3, 4];
console.log(nums.map(n => n * 2));', 3),
        (84, 9, N'Qual método de array devolve apenas os elementos que satisfazem uma condição?', NULL, 4),
        (85, 9, N'Quando fetch rejeita a Promise?', NULL, 5),
        (86, 9, N'Por que preferir textContent a innerHTML para exibir texto digitado pelo usuário?', NULL, 6),
        (87, 9, N'Onde a palavra await pode ser usada (fora de módulos)?', NULL, 7),
        (88, 9, N'Qual é o escopo de uma variável declarada com let dentro de um bloco if?', NULL, 8),
        (89, 9, N'Qual é o valor final de total?', N'const total = [10, 20, 30].reduce((s, n) => s + n, 0);', 9),
        (90, 9, N'Qual método associa uma função ao clique em um botão?', NULL, 10);
    INSERT INTO Questao (ID_Questao, ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao) VALUES
        (91, 10, N'(Enade 2017) Um software com defeito pode ser consequência de problemas no levantamento dos requisitos, uma vez que o requisito pode ser ambíguo porque o cliente não estava convicto de sua real necessidade ou porque a equipe o interpretou mal e registrou uma especificação de forma incorreta. Por esses motivos, as verificações, as validações e os testes são fundamentais para se certificar da qualidade do software resultante. Considerando esse contexto, avalie as afirmativas.

I. O teste funcional certifica se o software desempenha as funções especificadas nos requisitos.

II. O teste de desempenho valida a conformidade da especificação do processo de desenvolvimento de software.

III. O teste de aceitação é realizado pelo cliente a fim de validar se aquilo que foi implementado é o que foi solicitado.

IV. O teste de instalação, invariavelmente, é executado no local determinado pelo cliente para instalação do software.

V. As técnicas de verificação e validação de software asseguram que o sistema que está sendo desenvolvido seja adequado ao seu propósito.

É correto apenas o que se afirma em', NULL, 1),
        (92, 10, N'Na pirâmide de testes, qual tipo deve existir em maior quantidade?', NULL, 2),
        (93, 10, N'O que significa o padrão AAA?', NULL, 3),
        (94, 10, N'Para a regra "aprovado com nota maior ou igual a 7", quais valores testam a fronteira?', NULL, 4),
        (95, 10, N'Qual dublê é uma implementação simplificada que funciona, como um repositório em memória?', NULL, 5),
        (96, 10, N'O que impede um teste de unidade isolado na classe abaixo?', N'public class RelatorioService {
    public int Total() {
        var con = new SqlConnection("...");
        // consulta o banco
    }
}', 6),
        (97, 10, N'O que a cobertura de código mede?', NULL, 7),
        (98, 10, N'Qual característica um bom teste de unidade deve ter?', NULL, 8),
        (99, 10, N'Que tipo de erro os testes de integração pegam e os dublês escondem?', NULL, 9),
        (100, 10, N'(Enade 2021) Uma fábrica de software está realizando entrevistas para contratação de um profissional que esteja alinhado às exigências do atual mundo corporativo. Sabe-se que processos ágeis de desenvolvimento têm se tornado essenciais para empresas que desejam realizar entregas rápidas e frequentes de produtos e/ou serviços de software. Essa empresa possui uma equipe de desenvolvimento que faz uso de processos ágeis como o Scrum e eXtreme Programming (XP) e o acompanhamento por meio do quadro Kanban. Sendo assim, um conjunto de características deve ser verificado durante a entrevista para garantir que o candidato a ser contratado possua conhecimentos necessários para atuar juntamente a esta equipe. Com base no texto e nos processos ágeis de desenvolvimento de software, avalie as afirmativas.

I. Métodos ágeis são baseados em ciclos iterativo e incremental que se concentram no desenvolvimento rápido e na flexibilidade às mudanças, com a participação do cliente no processo de software.

II. Uma forte característica da XP é a garantia da qualidade do código produzido e, para isso, os desenvolvedores produzem testes automatizados antes mesmo de codificar uma funcionalidade.

III. O planejamento no Scrum é baseado na elaboração dos itens do product backlog, que é uma lista de funcionalidades desejadas pelo cliente, sendo o Scrum Master o responsável por gerenciá-lo.

IV. O quadro Kanban permite monitorar a evolução das tarefas necessárias durante o processo ágil de desenvolvimento de software, possibilitando um acompanhamento de forma visual das atividades em construção.

É correto apenas o que se afirma em', NULL, 10),
        (101, 11, N'Em qual modelo de serviço o cliente publica apenas a aplicação, sem gerenciar o sistema operacional?', NULL, 1),
        (102, 11, N'Um e-mail corporativo pronto para uso, acessado pelo navegador, é exemplo de:', NULL, 2),
        (103, 11, N'O que é escalar horizontalmente?', NULL, 3),
        (104, 11, N'Qual característica permite aumentar ou reduzir recursos conforme a carga?', NULL, 4),
        (105, 11, N'O que define uma nuvem híbrida?', NULL, 5),
        (106, 11, N'No modelo de responsabilidade compartilhada, quem responde por um banco de dados liberado para qualquer IP?', NULL, 6),
        (107, 11, N'Por que escolher a região mais próxima dos usuários?', NULL, 7),
        (108, 11, N'(Enade 2014) Quando o artigo Time Sharing Processing in Large Fast Computers foi publicado na Conferência Internacional de Processamento de Informação, realizada em Nova York, no ano de 1959, surgiu a ideia da virtualização. No trabalho apresentado, o foco era o uso da multiprogramação em tempo compartilhado, assim estabelecendo um novo conceito na utilização dos computadores de grande porte.

VERAS, M. Virtualização de servidores. Rio de Janeiro: RNP, 2011 (com adaptações).

Em relação aos conceitos e características de virtualização e o seu uso como tecnologia emergente atualmente nas empresas, avalie as afirmativas.

I. A virtualização pode ser utilizada para algumas finalidades, tais como: implementar a consolidação de servidores, reduzir custos, prover alta disponibilidade, implementar uma solução para recuperação de desastres e simplificar testes no desenvolvimento de softwares.

II. Os serviços de data centers oferecem segurança, economia e produtividade para empresas que precisam de infraestrutura atualizada para seus servidores. A virtualização é a tecnologia central de um data center.

III. A camada de virtualização de servidores mais conhecida é o Hipervisor ou Monitor de Máquina Virtual (Virtual Machine Monitor).

IV. São exemplos de tipos de virtualização: virtualização completa, paravirtualização e virtualização incompleta.

É correto apenas o que se afirma em', NULL, 8),
        (109, 11, N'(Enade 2017) Leia o texto a seguir.

Ao terceirizarem o ambiente de Tecnologia da Informação (TI), as empresas ficam livres da complexidade operacional para se concentrarem naquilo que sabem, ou seja, nas ações voltadas à consecução de sua atividade fim. Desse modo, a terceirização desse ambiente permite que a equipe de TI da instituição possa se dedicar à melhoria e à inovação das ações típicas da instituição, tornando-se parte importante do negócio, uma vez que deixam de ter como principal atribuição a manutenção da infraestrutura de TI.

Disponível em <http://www.exame.abril.com.br/tecnologia>. Acesso em 24 jul. 2017 (com adaptações).

A partir do conceito de terceirização (outsourcing) aplicável à área de TI, avalie as asserções e a relação proposta entre elas.

I. A computação em nuvem é uma ferramenta importante para o outsourcing de Tecnologia de Informação.

PORQUE

II. O outsourcing de cloud coloca computadores de ponta à disposição das empresas, o que favorece a agilidade operacional e garante que a área de TI promova inovações estratégicas e transformadoras.

A respeito dessas asserções, assinale a opção correta.', NULL, 9),
        (110, 11, N'(Enade 2008) O conceito de máquina virtual (MV) foi usado, na década de 1970, no sistema operacional IBM System 370. Atualmente, centros de dados (datacenters) usam MVs para migrar tarefas entre servidores conectados em rede e, assim, equilibrar carga de processamento. Além disso, plataformas atuais de desenvolvimento de software empregam MVs (Java,.NET). Uma MV pode ser construída para emular um processador ou um computador completo. Um código desenvolvido para uma máquina real pode ser executado de forma transparente em uma MV. Com relação a essas informações, assinale a opção correta.', NULL, 10),
        (111, 12, N'(Tomo 12 - CQA/UNIP) Leia o texto a seguir.

A norma internacional ABNT NBR ISO/IEC 27001:2013 [substituta da norma 17799:2005, citada no texto original], define práticas para a análise e elaboração de um projeto para a gestão da segurança da informação. Os conceitos mais importantes consistem em confiabilidade, integridade, disponibilidade e autenticidade, entendidos também como práticas essenciais para criar sistemas que evitam perda de dados e deixam as informações mais seguras. A aplicação dessas práticas garante a drástica redução dos riscos de uma organização sofrer um ataque.

LYRA, M. R. Segurança e auditoria em sistemas de informação. São Paulo: Ciência Moderna, 2009 (com adaptações).

Com base no texto, avalie as afirmativas.

I. A confidencialidade é a capacidade de um sistema permitir que usuários autorizados acessem determinadas informações e impedir o acesso a elas aos demais usuários.

II. A autenticidade e a integridade garantem que a informação esteja correta, seja verdadeira e não esteja corrompida.

III. A disponibilidade garante que a informação esteja disponível para todos os usuários que precisarem dela, sempre que necessário.

É correto o que se afirma em', NULL, 1),
        (112, 12, N'Qual é a diferença entre autenticação e autorização?', NULL, 2),
        (113, 12, N'Por que MD5 é inadequado para armazenar senhas?', NULL, 3),
        (114, 12, N'Qual é a função do sal no hash de senhas?', NULL, 4),
        (115, 12, N'Qual é a principal prevenção contra injeção de SQL?', NULL, 5),
        (116, 12, N'O que o trecho abaixo tem de inseguro?', N'var sql = "SELECT * FROM Usuario WHERE Email = ''" + email + "''";', 6),
        (117, 12, N'(Enade 2017) Uma empresa de médio porte sofreu um ataque hacker e alguns de seus sistemas foram comprometidos. Depois de alguns dias, a equipe de tecnologia de informação (TI) dessa empresa conseguiu restabelecer os sistemas, tendo a gestora de TI apresentado um plano para a diretoria com o intuito de criar um setor especializado em segurança da informação para evitar novos ataques. Após a análise dos riscos e benefícios do plano, a diretoria aprovou a implantação do referido setor de segurança da informação. A partir do cenário descrito, avalie as asserções e a relação proposta entre elas.

I. A implantação de um setor de segurança da informação está associada ao desenvolvimento de uma política de segurança de informação que é um conjunto de normas, métodos e procedimentos utilizados para a manutenção da segurança da informação, devendo ser formalizada e divulgada a todos os usuários.

PORQUE

II. A implantação de um sistema de gestão da segurança da informação (SGSI) preserva a confidencialidade, a integridade e a disponibilidade da informação, por meio de um processo de gestão de riscos, e fornece, para as partes interessadas, a confiança de que os riscos são adequadamente gerenciados, sendo importante que o SGSI esteja integrado aos processos da organização e à sua estrutura global.

A respeito dessas asserções, assinale a opção correta.', NULL, 7),
        (118, 12, N'Como uma API deve identificar o usuário de uma requisição autenticada?', NULL, 8),
        (119, 12, N'A LGPD regula principalmente:', NULL, 9),
        (120, 12, N'(Enade 2017) Leia o texto a seguir.

Recentemente, variantes de ransomware provocaram uma onda de sequestros digitais, que afetou hospitais, operadoras de telecomunicações e usuários residenciais. Os ransomwares são utilizados com o objetivo de entrar nos sistemas corporativos por meio da rede e criptografar todos os dados da empresa, de modo que possam exigir um resgate financeiro para liberá-los.

Disponível em <http://www.junipernetworks.com.br>. Acesso em 10 jul. 2017 (com adaptações).

Considerando as informações do texto, avalie as afirmativas a seguir, em relação às medidas de prevenção que devem compor a política de segurança corporativa contra esse tipo de malware.

I. Deve-se manter o treinamento constante dos usuários em relação ao uso de e-mail e sítios seguros, suspeitos ou falsos.

II. Deve-se garantir a existência de uma boa política de combate e recuperação contra desastres em relação às cópias de segurança.

III. Deve-se prevenir a contaminação por meio de ferramentas capazes de detectar ataques de força bruta, característicos desses worms, e cujo objetivo é contaminar os computadores da rede.

É correto o que se afirma em', NULL, 10);
    INSERT INTO Questao (ID_Questao, ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao) VALUES
        (121, 13, N'Qual é a complexidade da busca binária em um array ordenado?', NULL, 1),
        (122, 13, N'(Enade 2021) Considere que as variáveis pilha e fila correspondem, respectivamente, às estruturas de dados do tipo Pilha e Fila. Para testar as duas estruturas, um programador realizou a série de operações a seguir.

Após essas operações, ao imprimir o conteúdo de pilha e fila, respectivamente, serão exibidos os seguintes resultados:', N'Pilha pilha = new Pilha();
Fila fila = new Fila();
pilha.push(''A'');
pilha.push(''B'');
pilha.push(''C'');
fila.enqueue(pilha.top());
fila.enqueue(pilha.top());
fila.enqueue(''D'');
pilha.push(fila.dequeue());
fila.enqueue(fila.dequeue());
fila.enqueue(pilha.pop());
pilha.push(''E'');
fila.enqueue(''E'');
pilha.pop();', 2),
        (123, 13, N'Qual é a complexidade do código abaixo em função de n?', N'for (int i = 0; i < n; i++)
    for (int j = 0; j < n; j++)
        soma += i * j;', 3),
        (124, 13, N'(Enade 2021) O código Java, a seguir, contém a implementação de uma Pilha utilizando a estratégia encadeada.

Com base no exposto, assinale a opção que possui a implementação correta do método push para essa Pilha.', N'public class No {
    public int dado;
    public No prox;
    public No(int dado) {
        this.dado = dado;
    }
}

public class PilhaEncadeada {
    private No topo;

    public int pop() {
        if (topo == null)
            return -1;
        No lixo = topo;
        topo = topo.prox;
        lixo.prox = null;
        return lixo.dado;
    }

    public int top() {
        if (topo == null)
            return -1;
        return (topo.dado);
    }
}', 4),
        (125, 13, N'Qual algoritmo de ordenação tem complexidade O(n log n) em todos os casos?', NULL, 5),
        (126, 13, N'O que acontece com uma árvore binária de busca quando os valores são inseridos já em ordem crescente?', NULL, 6),
        (127, 13, N'(Enade 2017) O fragmento de código C++, a seguir, contém a implementação da inserção de um nó em uma árvore binária.

Com base no exposto, assinale a opção que apresenta o fragmento de programa para imprimir os valores contidos nos nós, percorrendo a árvore em pré-ordem.', N'struct noArvore {
    int dado;
    noArvore *esquerda;
    noArvore *direita;
};

noArvore *insere(noArvore *arvore, int valor) {
    if (arvore == NULL) {
        arvore = new noArvore;
        arvore->esquerda = NULL;
        arvore->direita = NULL;
        arvore->dado = valor;
    } else if (valor < arvore->dado) {
        arvore->esquerda = insere(arvore->esquerda, valor);
    } else {
        arvore->direita = insere(arvore->direita, valor);
    }
    return (arvore);
}', 7),
        (128, 13, N'(Enade 2017) Leia o texto a seguir.

A ordenação por inserção funciona de modo semelhante à forma como algumas pessoas ordenam cartas de baralho. Inicia-se com a mão esquerda vazia e as cartas empilhadas na mesa. Remove-se da pilha uma carta de cada vez, inserindo-a na posição correta na mão esquerda. Para se identificar a posição correta de uma carta, deve-se compará-la com as cartas presentes na mão esquerda, no sentido da direita para a esquerda. Em todos os momentos, as cartas na mão esquerda estão ordenadas, tendo sido obtidas no topo da pilha da mesa.

CORMEN, T. H. et al. Introduction to algorithms. 3. ed. Cambridge: The MIT Press, 2009 (com adaptações).

Um programador implementou um algoritmo de ordenação semelhante à forma de ordenação de cartas descrita no texto. Ao realizar um teste com um vetor de nove posições (vetor[1..9]), verificou que o algoritmo não funcionava corretamente.

Com base nessas informações, assinale a opção em que se apresentam a linha e o respectivo comando a ser substituído, para que o algoritmo ordene corretamente um vetor de inteiros de forma crescente.', N'01 para i <- 2 até 9 faça
02     valor <- vetor[i]
03     j <- i - 1
04     enquanto ((j >= 1) e (valor < vetor[j])) faça
05         vetor[i] <- vetor[j]
06         j <- j - 1
07         se (j = 0) então
08             interrompa
09         fim se
10     fim enquanto
11     vetor[j + 1] <- valor
12 fim para', 8),
        (129, 13, N'Em notação Big-O, como é classificado um algoritmo que faz 5n + 20 operações?', NULL, 9),
        (130, 13, N'(Enade 2008) Os termos da sequência de Fibonacci são definidos por: Fibonacci(0) = 0; Fibonacci(1) = 1; Fibonacci(n) = Fibonacci(n-1) + Fibonacci(n-2). Uma solução recursiva para o cálculo do i-ésimo termo da sequência é dada pela função a seguir.

Acerca da execução recursiva dessa função, assinale a opção INCORRETA.', N'1 funcao fibonacci(inteiro longo n)
2     se ((n = 0) OU (n = 1)) entao
3         retorne n
4     senao
5         retorne fibonacci(n-1) + fibonacci(n-2)
6     fim se
7 fim', 10),
        (131, 14, N'Em JSX, qual atributo define a classe CSS de um elemento?', NULL, 1),
        (132, 14, N'O que são props em React?', NULL, 2),
        (133, 14, N'Qual hook é usado para guardar estado em um componente de função?', NULL, 3),
        (134, 14, N'Por que alterar a variável de estado diretamente (contador = 5) não atualiza a tela?', NULL, 4),
        (135, 14, N'Com o array de dependências vazio, quando o useEffect executa?', NULL, 5),
        (136, 14, N'Para que serve a key ao renderizar listas com map?', NULL, 6),
        (137, 14, N'Quando dois componentes irmãos precisam do mesmo dado, a prática recomendada é:', NULL, 7),
        (138, 14, N'O que o componente abaixo exibe quando recebe nome="Ana"?', N'function Saudacao({ nome }) {
    return <h1>Olá, {nome}!</h1>;
}', 8),
        (139, 14, N'O que deve devolver a função retornada dentro de um useEffect?', NULL, 9),
        (140, 14, N'Por que o nome de um componente começa com letra maiúscula?', NULL, 10),
        (141, 15, N'Qual JOIN devolve todas as linhas da tabela da esquerda, mesmo sem correspondência à direita?', NULL, 1),
        (142, 15, N'(Enade 2017) Considere o modelo de dados a seguir.

A empresa que se utilizou do modelo de dados apresentado desejava obter um relatório com as quantidades e o total de vendas por vendedor, a cada ano, no formato abaixo.

Ano | Vendedor | Quantidade de vendas | Total de vendas
2015 | Vendedor A | 53 | 54 658
2015 | Vendedor B | 57 | 33 874
2015 | Vendedor C | 65 | 85 489
2016 | Vendedor A | 80 | 45 987
2016 | Vendedor B | 73 | 86 458
2016 | Vendedor C | 49 | 89 452
2016 | Vendedor D | 95 | 78 698

Com base nessas informações, assinale a opção com a consulta SQL que gera corretamente os dados do relatório solicitado pela empresa.', N'-- Modelo de dados (no original, apresentado como figura)
tb_vendedor (id_vendedor, ds_nome)
tb_pedido (id_pedido, id_vendedor -> tb_vendedor, dt_pedido)
tb_item_pedido (id_pedido -> tb_pedido, qt_produto, vl_unitario)', 2),
        (143, 15, N'Quantos índices clusterizados uma tabela pode ter no SQL Server?', NULL, 3),
        (144, 15, N'Qual é o efeito colateral de criar muitos índices em uma tabela?', NULL, 4),
        (145, 15, N'Qual função de janela repete a posição em empates e pula a posição seguinte?', NULL, 5),
        (146, 15, N'Por que o filtro abaixo pode impedir o uso de um índice em Data_Realizacao?', N'SELECT * FROM Desempenho
WHERE YEAR(Data_Realizacao) = 2026;', 6),
        (147, 15, N'O que PARTITION BY faz em uma função de janela?', NULL, 7),
        (148, 15, N'Qual operador costuma ser o mais claro para perguntar se existe ao menos uma linha relacionada?', NULL, 8),
        (149, 15, N'No plano de execução, o que um Table Scan em tabela grande, com filtro que devolve poucas linhas, costuma indicar?', NULL, 9),
        (150, 15, N'(Enade 2021) O modelo de dados relacional a seguir representa de forma simplificada o banco de dados para um sistema de informação bancário.

Usando a linguagem SQL (Structured Query Language), indique a consulta que recupera os nomes dos clientes e o valor médio movimentado por eles em sua conta até o momento. Primeiro devem ser exibidos os clientes com maior média de valor movimentado.', N'-- Modelo de dados (no original, apresentado como figura)
CLIENTE (COD_CLIENTE, NOME)
CONTA (NUM_CONTA, COD_CLIENTE -> CLIENTE)
HISTORICO_MOVIMENTACAO (COD_HISTORICO, NUM_CONTA -> CONTA, VAL_MOVIMENTADO)', 10);
    SET IDENTITY_INSERT Questao OFF;

    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (1, N'A', N'50', 0),
        (1, N'B', N'60', 1),
        (1, N'C', N'45', 0),
        (1, N'D', N'15', 0),
        (1, N'E', N'75', 0),
        (2, N'A', N'Array tem tamanho fixo definido na criação; List<T> é dinâmica e cresce conforme necessário.', 1),
        (2, N'B', N'Array é sempre mais rápida em qualquer operação.', 0),
        (2, N'C', N'List<T> só aceita tipos primitivos.', 0),
        (2, N'D', N'Não há diferença, são sinônimos.', 0),
        (2, N'E', N'Array não permite tipos genéricos como List<T>.', 0),
        (3, N'A', N'Insert()', 0),
        (3, N'B', N'Push()', 0),
        (3, N'C', N'Add()', 1),
        (3, N'D', N'Append()', 0),
        (3, N'E', N'Set()', 0),
        (4, N'A', N'string[5] nomes;', 0),
        (4, N'B', N'string[] nomes = new string[5];', 1),
        (4, N'C', N'Array<string> nomes(5);', 0),
        (4, N'D', N'string nomes[5] = new string;', 0),
        (4, N'E', N'List<string>(5) nomes;', 0),
        (5, N'A', N'4.', 0),
        (5, N'B', N'7.', 0),
        (5, N'C', N'11.', 0),
        (5, N'D', N'22.', 1),
        (5, N'E', N'30.', 0),
        (6, N'A', N'for', 0),
        (6, N'B', N'while', 0),
        (6, N'C', N'do-while', 0),
        (6, N'D', N'foreach', 1),
        (6, N'E', N'switch', 0),
        (7, N'A', N'System.Linq', 0),
        (7, N'B', N'System.IO', 0),
        (7, N'C', N'System.Collections.Generic', 1),
        (7, N'D', N'System.Text', 0),
        (7, N'E', N'System.Data', 0),
        (8, N'A', N'I.', 0),
        (8, N'B', N'III.', 0),
        (8, N'C', N'I e IV.', 1),
        (8, N'D', N'II e III.', 0),
        (8, N'E', N'II e IV.', 0),
        (9, N'A', N'Remove()', 1),
        (9, N'B', N'Delete()', 0),
        (9, N'C', N'Clear()', 0),
        (9, N'D', N'Pop()', 0),
        (9, N'E', N'Drop()', 0),
        (10, N'A', N'if (lista == null)', 0),
        (10, N'B', N'if (lista.Length == 0)', 0),
        (10, N'C', N'if (lista.Count == 0)', 1),
        (10, N'D', N'if (lista.Size() == 0)', 0),
        (10, N'E', N'if (lista.isEmpty())', 0),
        (11, N'A', N'I e II.', 0),
        (11, N'B', N'I e III.', 1),
        (11, N'C', N'II e IV.', 0),
        (11, N'D', N'I, III e IV.', 0),
        (11, N'E', N'II, III e IV.', 0),
        (12, N'A', N'I e II.', 0),
        (12, N'B', N'I e III.', 0),
        (12, N'C', N'II e III.', 0),
        (12, N'D', N'II e IV.', 1),
        (12, N'E', N'III e IV.', 0),
        (13, N'A', N'A análise de requisitos ocorre na fase de construção, quando são descritos todos os casos de uso, e em seguida modelados por meio de diagramas de casos de uso UML.', 0),
        (13, N'B', N'A análise de requisitos ocorre na fase de elaboração, em que são feitas entrevistas com usuários e definição do escopo do projeto.', 0),
        (13, N'C', N'A maior parte da análise de requisitos ocorre durante a fase de elaboração.', 1),
        (13, N'D', N'Por se tratar de um processo iterativo e evolutivo, a análise de requisitos ocorre na fase de construção juntamente com a programação, o que permite que os requisitos sejam revistos.', 0),
        (13, N'E', N'A análise de requisitos deve acontecer antes da programação e testes do sistema, não podendo sofrer alterações a partir do momento que estejam definidos.', 0),
        (14, N'A', N'joint application design.', 0),
        (14, N'B', N'PIECES (perfomance informação/dados economia controle eficiência e serviços).', 0),
        (14, N'C', N'facilitaded application specification techniques.', 0),
        (14, N'D', N'entrevista.', 0),
        (14, N'E', N'brainstorming.', 1),
        (15, N'A', N'restrições de domínio.', 1),
        (15, N'B', N'restrições de unicidade.', 0),
        (15, N'C', N'restrições de integridade referencial.', 0),
        (15, N'D', N'restrições de integridade da entidade.', 0),
        (15, N'E', N'restrições de integridade semântica.', 0),
        (16, N'A', N'Um atributo que identifica unicamente cada registro de uma tabela.', 1),
        (16, N'B', N'Um atributo que pode se repetir livremente.', 0),
        (16, N'C', N'Um campo opcional usado apenas para indexação.', 0),
        (16, N'D', N'Um relacionamento entre duas tabelas distintas.', 0),
        (16, N'E', N'Uma restrição visual aplicada no front-end.', 0),
        (17, N'A', N'FROM', 0),
        (17, N'B', N'ORDER BY', 0),
        (17, N'C', N'GROUP BY', 0),
        (17, N'D', N'WHERE', 1),
        (17, N'E', N'HAVING', 0),
        (18, N'A', N'Uma entidade está sempre associada a apenas uma outra.', 0),
        (18, N'B', N'Várias entidades de um lado podem se associar a várias do outro lado.', 1),
        (18, N'C', N'Uma entidade não pode ter relacionamentos.', 0),
        (18, N'D', N'É um erro de modelagem que deve ser evitado.', 0),
        (18, N'E', N'Significa que a entidade é fraca.', 0),
        (19, N'A', N'1FN', 0),
        (19, N'B', N'2FN', 0),
        (19, N'C', N'3FN', 1),
        (19, N'D', N'Forma normal de Boyce-Codd obrigatoriamente', 0),
        (19, N'E', N'Nenhuma', 0),
        (20, N'A', N'SELECT', 0),
        (20, N'B', N'INSERT', 0),
        (20, N'C', N'UPDATE', 0),
        (20, N'D', N'CREATE TABLE', 1),
        (20, N'E', N'DELETE', 0);
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (21, N'A', N'3', 1),
        (21, N'B', N'2', 0),
        (21, N'C', N'Erro de sintaxe', 0),
        (21, N'D', N'4', 0),
        (21, N'E', N'3.5', 0),
        (22, N'A', N'float', 0),
        (22, N'B', N'int', 0),
        (22, N'C', N'bool', 0),
        (22, N'D', N'Depende do que o usuário digitar', 0),
        (22, N'E', N'str', 1),
        (23, N'A', N'3, 4, 5, 6', 0),
        (23, N'B', N'0, 1, 2, 3, 4, 5', 0),
        (23, N'C', N'2, 4, 6', 0),
        (23, N'D', N'2, 3, 4, 5, 6', 0),
        (23, N'E', N'2, 3, 4, 5', 1),
        (24, N'A', N'n * 2 == 0', 0),
        (24, N'B', N'n // 2 == 1', 0),
        (24, N'C', N'n % 2 == 0', 1),
        (24, N'D', N'n / 2 == 0', 0),
        (24, N'E', N'n % 2 == 1', 0),
        (25, N'A', N'o time X não venceu o último jogo e o time Y é campeão.', 0),
        (25, N'B', N'o time X venceu o último jogo e o time Y não é campeão.', 1),
        (25, N'C', N'ou o time X não venceu o último jogo ou o time Y é campeão.', 0),
        (25, N'D', N'o time X não venceu o último jogo e o time Y não é campeão.', 0),
        (25, N'E', N'se o time Y não é campeão, então o time X não venceu o último jogo.', 0),
        (26, N'A', N'B', 0),
        (26, N'B', N'C', 0),
        (26, N'C', N'Nada é impresso', 0),
        (26, N'D', N'A e B', 0),
        (26, N'E', N'A', 1),
        (27, N'A', N'False', 0),
        (27, N'B', N'None', 1),
        (27, N'C', N'0', 0),
        (27, N'D', N'Uma string vazia', 0),
        (27, N'E', N'Um erro', 0),
        (28, N'A', N'3', 1),
        (28, N'B', N'2', 0),
        (28, N'C', N'Infinitas vezes', 0),
        (28, N'D', N'Nenhuma', 0),
        (28, N'E', N'4', 0),
        (29, N'A', N'p + ~q * r', 0),
        (29, N'B', N'p + q * ~r', 0),
        (29, N'C', N'~p + q * r', 1),
        (29, N'D', N'~p + ~q * r', 0),
        (29, N'E', N'~p + q * ~r', 0),
        (30, N'A', N'Todos os valores são texto', 0),
        (30, N'B', N'Só existem números inteiros', 0),
        (30, N'C', N'Python não tem tipos', 0),
        (30, N'D', N'Estática: o tipo é declarado e não muda', 0),
        (30, N'E', N'Dinâmica: o tipo pertence ao valor e a variável pode receber outro tipo', 1),
        (31, N'A', N'<section>', 0),
        (31, N'B', N'<body>', 0),
        (31, N'C', N'<div>', 0),
        (31, N'D', N'<main>', 1),
        (31, N'E', N'<article>', 0),
        (32, N'A', N'outline', 0),
        (32, N'B', N'padding', 1),
        (32, N'C', N'gap', 0),
        (32, N'D', N'margin', 0),
        (32, N'E', N'border', 0),
        (33, N'A', N'Transforma o elemento em flex', 0),
        (33, N'B', N'Arredonda os cantos', 0),
        (33, N'C', N'Remove as margens', 0),
        (33, N'D', N'A largura passa a incluir padding e borda', 1),
        (33, N'E', N'A cor da borda', 0),
        (34, N'A', N'p', 0),
        (34, N'B', N'#titulo', 1),
        (34, N'C', N'*', 0),
        (34, N'D', N'div p', 0),
        (34, N'E', N'.destaque', 0),
        (35, N'A', N'min-width', 1),
        (35, N'B', N'orientation: portrait', 0),
        (35, N'C', N'max-height', 0),
        (35, N'D', N'print', 0),
        (35, N'E', N'max-width', 0),
        (36, N'A', N'place-self', 0),
        (36, N'B', N'flex-direction', 0),
        (36, N'C', N'align-items', 0),
        (36, N'D', N'align-content', 0),
        (36, N'E', N'justify-content', 1),
        (37, N'A', N'Três grades aninhadas', 0),
        (37, N'B', N'Três linhas de altura igual', 0),
        (37, N'C', N'Uma coluna com 3 itens', 0),
        (37, N'D', N'Três colunas de largura igual', 1),
        (37, N'E', N'Erro de sintaxe', 0),
        (38, N'A', N'segurança no uso, pois se uma imagem tiver baixa resolução e impossibilitar a correta visualização, o texto informativo associado à imagem possibilita a identificação do conteúdo do respectivo link.', 0),
        (38, N'B', N'facilidade de aprendizado, pois a comunicação visual do sistema web, por meio do uso de imagens, aliada à comunicação textual, por meio do uso de textos informativos para cada imagem, ajuda o usuário a aprender os caminhos de navegação.', 0),
        (38, N'C', N'acessibilidade, pois a possibilidade de acesso à página por pessoas com deficiência visual, para que interajam com os conteúdos, oferece condições de igualdade às pessoas na interação com o sistema web.', 1),
        (38, N'D', N'ergonomia, pois o uso de imagens em links causa um cansaço visual quando o sistema web é aberto em um smartphone ou em um tablet com baixa resolução, e o atributo alt substitui a imagem por texto.', 0),
        (38, N'E', N'comunicabilidade, pois se uma imagem não comunica adequadamente a informação desejada, um texto alternativo deve complementar a respectiva informação, de forma que a interação com o sistema web não seja prejudicada.', 0),
        (39, N'A', N'A página não carrega', 0),
        (39, N'B', N'As imagens ficam em preto e branco', 0),
        (39, N'C', N'Nada muda', 0),
        (39, N'D', N'O CSS é ignorado', 0),
        (39, N'E', N'O navegador simula uma tela larga e as media queries não se comportam como esperado', 1),
        (40, N'A', N'Grid é mais antigo que Flexbox', 0),
        (40, N'B', N'Grid não aceita media queries', 0),
        (40, N'C', N'Flexbox organiza em uma dimensão; Grid em duas', 1),
        (40, N'D', N'Flexbox só funciona com texto', 0),
        (40, N'E', N'Não há diferença', 0);
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (41, N'A', N'git add', 1),
        (41, N'B', N'git stage-all', 0),
        (41, N'C', N'git push', 0),
        (41, N'D', N'git save', 0),
        (41, N'E', N'git commit', 0),
        (42, N'A', N'git switch -c nova', 1),
        (42, N'B', N'git init nova', 0),
        (42, N'C', N'git merge nova', 0),
        (42, N'D', N'git branch -d nova', 0),
        (42, N'E', N'git pull nova', 0),
        (43, N'A', N'O Git escolhe a versão mais recente', 0),
        (43, N'B', N'Ocorre um conflito que precisa ser resolvido manualmente', 1),
        (43, N'C', N'O merge é cancelado para sempre', 0),
        (43, N'D', N'As duas versões são somadas', 0),
        (43, N'E', N'O arquivo é apagado', 0),
        (44, N'A', N'Para desfazer commits', 0),
        (44, N'B', N'Para guardar senhas com segurança', 0),
        (44, N'C', N'Para listar os colaboradores', 0),
        (44, N'D', N'Para configurar o GitHub Actions', 0),
        (44, N'E', N'Para indicar arquivos que não devem ser versionados', 1),
        (45, N'A', N'git pull', 1),
        (45, N'B', N'git push', 0),
        (45, N'C', N'git add', 0),
        (45, N'D', N'git log', 0),
        (45, N'E', N'git commit', 0),
        (46, N'A', N'Um backup automático', 0),
        (46, N'B', N'Um comando para baixar arquivos', 0),
        (46, N'C', N'Um pedido para integrar uma branch em outra, com espaço para revisão', 1),
        (46, N'D', N'Uma cópia do repositório', 0),
        (46, N'E', N'Um tipo de commit', 0),
        (47, N'A', N'I e II.', 0),
        (47, N'B', N'I e III.', 1),
        (47, N'C', N'II e IV.', 0),
        (47, N'D', N'I, III e IV.', 0),
        (47, N'E', N'II, III e IV.', 0),
        (48, N'A', N'Apagar o repositório remoto', 0),
        (48, N'B', N'Nenhum', 0),
        (48, N'C', N'Perder o .gitignore', 0),
        (48, N'D', N'Tornar o repositório público', 0),
        (48, N'E', N'Reescrever commits que outras pessoas já têm, causando divergência de histórico', 1),
        (49, N'A', N'I e II.', 1),
        (49, N'B', N'II e III.', 0),
        (49, N'C', N'III e IV.', 0),
        (49, N'D', N'I, II e IV.', 0),
        (49, N'E', N'I, III e IV.', 0),
        (50, N'A', N'I, apenas.', 0),
        (50, N'B', N'II, apenas.', 0),
        (50, N'C', N'I e III, apenas.', 0),
        (50, N'D', N'II e III, apenas.', 0),
        (50, N'E', N'I, II e III.', 1),
        (51, N'A', N'R1, R2 e R3.', 0),
        (51, N'B', N'R1, R2 e R5.', 0),
        (51, N'C', N'R1, R3 e R4.', 0),
        (51, N'D', N'R2, R4 e R5.', 1),
        (51, N'E', N'R3, R4 e R5.', 0),
        (52, N'A', N'Ser genérico', 0),
        (52, N'B', N'Ser longo', 0),
        (52, N'C', N'Não ter prioridade', 0),
        (52, N'D', N'Citar a linguagem de programação', 0),
        (52, N'E', N'Ser verificável', 1),
        (53, N'A', N'Um papel externo que interage com o sistema', 1),
        (53, N'B', N'Um método', 0),
        (53, N'C', N'Uma classe do sistema', 0),
        (53, N'D', N'Uma tela', 0),
        (53, N'E', N'Uma tabela do banco', 0),
        (54, N'A', N'I e II.', 1),
        (54, N'B', N'I e IV.', 0),
        (54, N'C', N'II e III.', 0),
        (54, N'D', N'I, III e IV.', 0),
        (54, N'E', N'II, III e IV.', 0),
        (55, N'A', N'Generalização', 0),
        (55, N'B', N'Associação', 0),
        (55, N'C', N'Composição', 1),
        (55, N'D', N'Agregação', 0),
        (55, N'E', N'Dependência', 0),
        (56, N'A', N'DER', 0),
        (56, N'B', N'Diagrama de casos de uso', 0),
        (56, N'C', N'Diagrama de classes', 0),
        (56, N'D', N'Diagrama de sequência', 1),
        (56, N'E', N'Diagrama de implantação', 0),
        (57, N'A', N'Exatamente um', 0),
        (57, N'B', N'Um ou mais', 1),
        (57, N'C', N'Nenhum ou um', 0),
        (57, N'D', N'No máximo um', 0),
        (57, N'E', N'Zero ou mais', 0),
        (58, N'A', N'Ator', 0),
        (58, N'B', N'Requisito não funcional', 0),
        (58, N'C', N'Diagrama', 0),
        (58, N'D', N'Restrição de hardware', 0),
        (58, N'E', N'Regra de negócio', 1),
        (59, N'A', N'Documento de entrevista com usuários; modelo de caso de uso para os requisitos funcionais; prototipação de telas.', 1),
        (59, N'B', N'Documento de estudo de viabilidade; modelo de caso de uso para os requisitos funcionais; prototipação de telas.', 0),
        (59, N'C', N'Matriz de rastreabilidade; modelo de caso de uso para os requisitos não-funcionais; prototipação de telas.', 0),
        (59, N'D', N'Documento de entrevista com usuários; modelo de caso de uso para os requisitos não-funcionais; matriz de rastreabilidade.', 0),
        (59, N'E', N'Documento de estudo de viabilidade; modelo de caso de uso para os requisitos funcionais; matriz de rastreabilidade.', 0),
        (60, N'A', N'O software deve ser operacionalizado no sistema Linux.', 0),
        (60, N'B', N'O tempo de desenvolvimento não deve ultrapassar seis meses.', 0),
        (60, N'C', N'O software deve emitir relatórios de compras a cada quinze dias.', 1),
        (60, N'D', N'O tempo de resposta do sistema não deve ultrapassar 30 segundos.', 0),
        (60, N'E', N'A base de dados deve ser protegida para acesso apenas de usuários autorizados.', 0);
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (61, N'A', N'I, apenas.', 0),
        (61, N'B', N'II, apenas.', 0),
        (61, N'C', N'I e III, apenas.', 0),
        (61, N'D', N'II e III, apenas.', 1),
        (61, N'E', N'I, II e III.', 0),
        (62, N'A', N'ARP', 0),
        (62, N'B', N'IP', 0),
        (62, N'C', N'UDP', 0),
        (62, N'D', N'TCP', 1),
        (62, N'E', N'DNS', 0),
        (63, N'A', N'255', 0),
        (63, N'B', N'128', 0),
        (63, N'C', N'24', 0),
        (63, N'D', N'254', 1),
        (63, N'E', N'256', 0),
        (64, N'A', N'150.162.1.1', 0),
        (64, N'B', N'192.168.1.10', 1),
        (64, N'C', N'1.1.1.1', 0),
        (64, N'D', N'200.10.5.4', 0),
        (64, N'E', N'8.8.8.8', 0),
        (65, N'A', N'I e II.', 0),
        (65, N'B', N'I e III.', 0),
        (65, N'C', N'II e IV.', 0),
        (65, N'D', N'I, III e IV.', 1),
        (65, N'E', N'II, III e IV.', 0),
        (66, N'A', N'200', 0),
        (66, N'B', N'301', 0),
        (66, N'C', N'500', 0),
        (66, N'D', N'404', 1),
        (66, N'E', N'201', 0),
        (67, N'A', N'outros 4 hubs interligados contendo, cada um, menos portas de conexão, o que resultaria na diminuição do volume de colisões na LAN e aumentaria o desempenho da rede.', 0),
        (67, N'B', N'um switch, que permitiria a diminuição do volume de colisões e uma melhoria no desempenho da rede.', 1),
        (67, N'C', N'um switch, que traria benefícios relacionados ao desempenho da rede, uma vez que o número de colisões permaneceria inalterado.', 0),
        (67, N'D', N'um switch, que diminuiria o desempenho da rede, uma vez que aumentaria o volume de colisões.', 0),
        (67, N'E', N'um switch, que aumentaria o volume de colisões e melhoraria o desempenho da rede.', 0),
        (68, N'A', N'Alterar o gateway da estação PROFESSORES03 para 175.16.10.200.', 0),
        (68, N'B', N'Alterar o endereço de IP da estação PROFESSORES01 para 175.16.30.200.', 0),
        (68, N'C', N'Alterar a máscara de sub-rede da estação PROFESSORES01 para 255.255.0.0.', 1),
        (68, N'D', N'Alterar a máscara de sub-rede da estação PROFESSORES03 para 255.255.255.0.', 0),
        (68, N'E', N'Alterar o gateway da estação PROFESSORES01 para o endereço de IP 175.16.30.200.', 0),
        (69, N'A', N'Eliminar a necessidade de DNS', 0),
        (69, N'B', N'Trocar IPv4 por IPv6', 0),
        (69, N'C', N'Criptografar todas as páginas', 0),
        (69, N'D', N'Aumentar a velocidade do Wi-Fi', 0),
        (69, N'E', N'Vários dispositivos com IP privado compartilharem um IP público', 1),
        (70, N'A', N'8080', 0),
        (70, N'B', N'80', 0),
        (70, N'C', N'21', 0),
        (70, N'D', N'22', 0),
        (70, N'E', N'443', 1),
        (71, N'A', N'class', 0),
        (71, N'B', N'instance', 0),
        (71, N'C', N'this', 0),
        (71, N'D', N'create', 0),
        (71, N'E', N'new', 1),
        (72, N'A', N'I e II.', 0),
        (72, N'B', N'I e III.', 0),
        (72, N'C', N'I e IV.', 1),
        (72, N'D', N'II e III.', 0),
        (72, N'E', N'II e IV.', 0),
        (73, N'A', N'Duas', 0),
        (73, N'B', N'Uma', 1),
        (73, N'C', N'Depende da versão do Java', 0),
        (73, N'D', N'Quantas quiser', 0),
        (73, N'E', N'Nenhuma', 0),
        (74, N'A', N'II, apenas.', 1),
        (74, N'B', N'III, apenas.', 0),
        (74, N'C', N'I e II, apenas.', 0),
        (74, N'D', N'I e III, apenas.', 0),
        (74, N'E', N'I, II e III.', 0),
        (75, N'A', N'Interfaces só existem em C#', 0),
        (75, N'B', N'Interfaces podem ser instanciadas', 0),
        (75, N'C', N'Uma classe pode implementar várias interfaces, mas estender apenas uma classe abstrata', 1),
        (75, N'D', N'Classes abstratas não podem ter métodos', 0),
        (75, N'E', N'Não há diferença', 0),
        (76, N'A', N'Erro de compilação', 0),
        (76, N'B', N'...', 0),
        (76, N'C', N'Animal', 0),
        (76, N'D', N'Miau', 1),
        (76, N'E', N'null', 0),
        (77, N'A', N'Herança', 0),
        (77, N'B', N'Sobrescrita', 0),
        (77, N'C', N'Sobrecarga', 1),
        (77, N'D', N'Encapsulamento', 0),
        (77, N'E', N'Abstração', 0),
        (78, N'A', N'Importar pacotes', 0),
        (78, N'B', N'Tornar a classe abstrata', 0),
        (78, N'C', N'Garantir que o objeto seja criado em um estado válido', 1),
        (78, N'D', N'Definir o tipo de retorno da classe', 0),
        (78, N'E', N'Destruir o objeto', 0),
        (79, N'A', N'Porque deixa o código mais lento', 0),
        (79, N'B', N'Porque impede a herança', 0),
        (79, N'C', N'Porque só funciona com tipos primitivos', 0),
        (79, N'D', N'Porque, sem validação, ele equivale a deixar o atributo público', 1),
        (79, N'E', N'Porque setters são proibidos em Java', 0),
        (80, N'A', N'@Overload', 0),
        (80, N'B', N'@Override', 1),
        (80, N'C', N'@Implements', 0),
        (80, N'D', N'@Super', 0),
        (80, N'E', N'@Extends', 0);
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (81, N'A', N'true', 0),
        (81, N'B', N'undefined', 0),
        (81, N'C', N'Erro de tipo', 0),
        (81, N'D', N'NaN', 0),
        (81, N'E', N'false', 1),
        (82, N'A', N'final', 0),
        (82, N'B', N'static', 0),
        (82, N'C', N'const', 1),
        (82, N'D', N'let', 0),
        (82, N'E', N'var', 0),
        (83, N'A', N'20', 0),
        (83, N'B', N'[1, 2, 3, 4]', 0),
        (83, N'C', N'undefined', 0),
        (83, N'D', N'[2, 4, 6, 8]', 1),
        (83, N'E', N'[2, 4]', 0),
        (84, N'A', N'sort', 0),
        (84, N'B', N'reduce', 0),
        (84, N'C', N'forEach', 0),
        (84, N'D', N'filter', 1),
        (84, N'E', N'map', 0),
        (85, N'A', N'Sempre que o status for diferente de 200', 0),
        (85, N'B', N'Nunca', 0),
        (85, N'C', N'Quando o JSON é inválido', 0),
        (85, N'D', N'Quando a resposta é 404', 0),
        (85, N'E', N'Apenas em falha de rede ou quando a requisição não pode ser feita', 1),
        (86, N'A', N'textContent é mais colorido', 0),
        (86, N'B', N'Não há diferença', 0),
        (86, N'C', N'innerHTML interpreta o texto como HTML e permite injeção de script', 1),
        (86, N'D', N'innerHTML não funciona em celulares', 0),
        (86, N'E', N'textContent aceita imagens', 0),
        (87, N'A', N'Somente em arquivos HTML', 0),
        (87, N'B', N'Somente dentro de funções declaradas com async', 1),
        (87, N'C', N'Somente dentro de laços for', 0),
        (87, N'D', N'Somente com setTimeout', 0),
        (87, N'E', N'Em qualquer lugar do código', 0),
        (88, N'A', N'Global', 0),
        (88, N'B', N'O arquivo inteiro', 0),
        (88, N'C', N'A função inteira', 0),
        (88, N'D', N'Somente o bloco if', 1),
        (88, N'E', N'A janela do navegador', 0),
        (89, N'A', N'102030', 0),
        (89, N'B', N'60', 1),
        (89, N'C', N'[10, 20, 30]', 0),
        (89, N'D', N'0', 0),
        (89, N'E', N'30', 0),
        (90, N'A', N'button.event = "click"', 0),
        (90, N'B', N'button.click(funcao)', 0),
        (90, N'C', N'document.listen("click")', 0),
        (90, N'D', N'button.addEventListener("click", funcao)', 1),
        (90, N'E', N'button.onClick()', 0),
        (91, N'A', N'I e IV.', 0),
        (91, N'B', N'I, III e V.', 1),
        (91, N'C', N'II, III e V.', 0),
        (91, N'D', N'II, IV e V.', 0),
        (91, N'E', N'I, II, III e V.', 0),
        (92, N'A', N'Testes ponta a ponta', 0),
        (92, N'B', N'Testes de unidade', 1),
        (92, N'C', N'Testes de carga', 0),
        (92, N'D', N'Testes de interface', 0),
        (92, N'E', N'Testes manuais', 0),
        (93, N'A', N'Assert, Act, Arrange', 0),
        (93, N'B', N'Arrange, Act, Assert', 1),
        (93, N'C', N'Abrir, Alterar, Apagar', 0),
        (93, N'D', N'Automatizar, Agendar, Avisar', 0),
        (93, N'E', N'Analisar, Arquivar, Aprovar', 0),
        (94, N'A', N'Valores negativos', 0),
        (94, N'B', N'5 e 9', 0),
        (94, N'C', N'Apenas 7', 0),
        (94, N'D', N'0 e 10', 0),
        (94, N'E', N'6.99, 7 e 7.01', 1),
        (95, N'A', N'Stub', 0),
        (95, N'B', N'Mock', 0),
        (95, N'C', N'Spy', 0),
        (95, N'D', N'Fake', 1),
        (95, N'E', N'Dummy', 0),
        (96, N'A', N'O nome da classe', 0),
        (96, N'B', N'O tipo de retorno int', 0),
        (96, N'C', N'O método ser público', 0),
        (96, N'D', N'A dependência do banco ser criada dentro do método, sem injeção', 1),
        (96, N'E', N'Nada impede', 0),
        (97, N'A', N'A velocidade do sistema', 0),
        (97, N'B', N'Se todas as regras estão corretas', 0),
        (97, N'C', N'A quantidade de usuários', 0),
        (97, N'D', N'Quais linhas foram executadas pelos testes', 1),
        (97, N'E', N'O número de defeitos', 0),
        (98, N'A', N'Ter nome genérico', 0),
        (98, N'B', N'Acessar a internet', 0),
        (98, N'C', N'Ser determinístico e independente dos outros testes', 1),
        (98, N'D', N'Depender da ordem de execução', 0),
        (98, N'E', N'Usar o banco de produção', 0),
        (99, N'A', N'Lentidão da rede do usuário', 0),
        (99, N'B', N'Erros de digitação em comentários', 0),
        (99, N'C', N'Cores erradas na tela', 0),
        (99, N'D', N'Nenhum', 0),
        (99, N'E', N'Mapeamento errado de colunas e consultas SQL inválidas', 1),
        (100, N'A', N'II.', 0),
        (100, N'B', N'I e III.', 0),
        (100, N'C', N'I, II e IV.', 1),
        (100, N'D', N'I, III e IV.', 0),
        (100, N'E', N'II, III e IV.', 0);
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (101, N'A', N'IaaS', 0),
        (101, N'B', N'Nuvem privada', 0),
        (101, N'C', N'PaaS', 1),
        (101, N'D', N'On-premises', 0),
        (101, N'E', N'SaaS', 0),
        (102, N'A', N'Contêiner', 0),
        (102, N'B', N'SaaS', 1),
        (102, N'C', N'Nuvem híbrida', 0),
        (102, N'D', N'IaaS', 0),
        (102, N'E', N'PaaS', 0),
        (103, N'A', N'Mudar de região', 0),
        (103, N'B', N'Acrescentar mais instâncias da aplicação', 1),
        (103, N'C', N'Trocar de provedor', 0),
        (103, N'D', N'Reduzir o número de usuários', 0),
        (103, N'E', N'Aumentar a CPU da mesma máquina', 0),
        (104, N'A', N'Redundância', 0),
        (104, N'B', N'Criptografia', 0),
        (104, N'C', N'Latência', 0),
        (104, N'D', N'Virtualização', 0),
        (104, N'E', N'Elasticidade', 1),
        (105, N'A', N'Uso de dois provedores públicos sem integração', 0),
        (105, N'B', N'Servidores físicos alugados', 0),
        (105, N'C', N'Uso apenas de SaaS', 0),
        (105, N'D', N'Nuvem sem internet', 0),
        (105, N'E', N'Integração entre ambiente privado e nuvem pública', 1),
        (106, N'A', N'O fabricante do hardware', 0),
        (106, N'B', N'O provedor de internet', 0),
        (106, N'C', N'Ninguém', 0),
        (106, N'D', N'O provedor de nuvem', 0),
        (106, N'E', N'O cliente', 1),
        (107, N'A', N'Porque é obrigatório em todos os provedores', 0),
        (107, N'B', N'Para reduzir a latência e atender exigências sobre dados', 1),
        (107, N'C', N'Para ter mais armazenamento', 0),
        (107, N'D', N'Para evitar backups', 0),
        (107, N'E', N'Para pagar mais barato sempre', 0),
        (108, N'A', N'I e II.', 0),
        (108, N'B', N'II e IV.', 0),
        (108, N'C', N'III e IV.', 0),
        (108, N'D', N'I, II e III.', 1),
        (108, N'E', N'I, III e IV.', 0),
        (109, N'A', N'As asserções I e II são proposições verdadeiras, e a asserção II justifica a I.', 1),
        (109, N'B', N'As asserções I e II são proposições verdadeiras, e a asserção II não justifica a I.', 0),
        (109, N'C', N'A asserção I é uma proposição verdadeira, e a II é uma proposição falsa.', 0),
        (109, N'D', N'A asserção I é uma proposição falsa, e a II é uma proposição verdadeira.', 0),
        (109, N'E', N'As asserções I e II são proposições falsas.', 0),
        (110, N'A', N'O conceito de transparência mencionado indica que a MV permite que um aplicativo acesse diretamente o hardware da máquina.', 0),
        (110, N'B', N'Uma das vantagens mais significativas de uma MV é a economia de carga de CPU e de memória RAM na execução de um aplicativo.', 0),
        (110, N'C', N'Uma MV oferece maior controle de segurança, uma vez que aplicativos são executados em um ambiente controlado.', 1),
        (110, N'D', N'Para emular uma CPU dual-core, uma MV deve ser instalada e executada em um computador com CPU dual-core.', 0),
        (110, N'E', N'Como uma MV não é uma máquina real, um sistema operacional nela executado fica automaticamente imune a vírus.', 0),
        (111, N'A', N'I, apenas.', 0),
        (111, N'B', N'II, apenas.', 0),
        (111, N'C', N'I e III, apenas.', 0),
        (111, N'D', N'II e III, apenas.', 0),
        (111, N'E', N'I, II e III.', 1),
        (112, N'A', N'Autorização é feita pelo navegador', 0),
        (112, N'B', N'Autenticação só existe em sistemas web', 0),
        (112, N'C', N'Autorização acontece antes da autenticação', 0),
        (112, N'D', N'Autenticação verifica quem é o usuário; autorização define o que ele pode fazer', 1),
        (112, N'E', N'São sinônimos', 0),
        (113, N'A', N'Porque é rápido demais e permite testar bilhões de tentativas', 1),
        (113, N'B', N'Porque é pago', 0),
        (113, N'C', N'Porque gera hashes muito longos', 0),
        (113, N'D', N'Porque é reversível', 0),
        (113, N'E', N'Porque não funciona com acentos', 0),
        (114, N'A', N'Fazer senhas iguais gerarem hashes diferentes', 1),
        (114, N'B', N'Acelerar a verificação', 0),
        (114, N'C', N'Comprimir a senha', 0),
        (114, N'D', N'Deixar o hash reversível', 0),
        (114, N'E', N'Guardar a senha original', 0),
        (115, N'A', N'Trocar o nome das tabelas', 0),
        (115, N'B', N'Esconder as mensagens de erro', 0),
        (115, N'C', N'Limitar o tamanho do banco', 0),
        (115, N'D', N'Usar consultas parametrizadas', 1),
        (115, N'E', N'Usar HTTPS', 0),
        (116, N'A', N'O uso de aspas simples no C#', 0),
        (116, N'B', N'A concatenação de texto do usuário na consulta, que permite injeção de SQL', 1),
        (116, N'C', N'O uso de SELECT *', 0),
        (116, N'D', N'O nome da tabela', 0),
        (116, N'E', N'Nada', 0),
        (117, N'A', N'As asserções I e II são proposições verdadeiras, e a II justifica a I.', 0),
        (117, N'B', N'As asserções I e II são proposições verdadeiras, e a II não justifica a I.', 1),
        (117, N'C', N'A asserção I é uma proposição verdadeira, e a II é uma proposição falsa.', 0),
        (117, N'D', N'A asserção I é uma proposição falsa, e a II é uma proposição verdadeira.', 0),
        (117, N'E', N'As asserções I e II são proposições falsas.', 0),
        (118, N'A', N'Pelo id enviado na URL', 0),
        (118, N'B', N'Pelo nome digitado no corpo', 0),
        (118, N'C', N'Pelo endereço IP', 0),
        (118, N'D', N'Pelo token validado no servidor', 1),
        (118, N'E', N'Pelo navegador usado', 0),
        (119, N'A', N'O tratamento de dados pessoais', 1),
        (119, N'B', N'A velocidade das redes', 0),
        (119, N'C', N'O preço de softwares', 0),
        (119, N'D', N'O uso de criptomoedas', 0),
        (119, N'E', N'A instalação de antivírus', 0),
        (120, N'A', N'I, apenas.', 0),
        (120, N'B', N'III, apenas.', 0),
        (120, N'C', N'I e II, apenas.', 1),
        (120, N'D', N'II e III, apenas.', 0),
        (120, N'E', N'I, II e III.', 0);
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (121, N'A', N'O(n)', 0),
        (121, N'B', N'O(log n)', 1),
        (121, N'C', N'O(n log n)', 0),
        (121, N'D', N'O(1)', 0),
        (121, N'E', N'O(n²)', 0),
        (122, N'A', N'pilha: topo → C → A → E. fila: início → D → A → A → E.', 0),
        (122, N'B', N'pilha: topo → A. fila: início → D → B → C → E.', 0),
        (122, N'C', N'pilha: topo → C → B → A. fila: início → D → C → C → E.', 1),
        (122, N'D', N'pilha: topo → B → A. fila: início → D → B → C → E.', 0),
        (122, N'E', N'pilha: topo → C → B → A. fila: início → D → B → C → E.', 0),
        (123, N'A', N'O(n²)', 1),
        (123, N'B', N'O(1)', 0),
        (123, N'C', N'O(log n)', 0),
        (123, N'D', N'O(2n)', 0),
        (123, N'E', N'O(n)', 0),
        (124, N'A', N'public void push(int elemento) { No novo = new No(elemento); if (topo == null) topo = novo; else topo.prox = novo; }', 0),
        (124, N'B', N'public void push(int elemento) { No novo = new No(elemento); topo = novo; }', 0),
        (124, N'C', N'public void push(int elemento) { topo.dado = elemento; }', 0),
        (124, N'D', N'public void push(int elemento) { No novo = new No(elemento); while (topo != null) topo = topo.prox; topo.prox = novo; }', 0),
        (124, N'E', N'public void push(int elemento) { No novo = new No(elemento); novo.prox = topo; topo = novo; }', 1),
        (125, N'A', N'Merge sort', 1),
        (125, N'B', N'Bubble sort', 0),
        (125, N'C', N'Selection sort', 0),
        (125, N'D', N'Quicksort', 0),
        (125, N'E', N'Insertion sort', 0),
        (126, N'A', N'Ela rejeita os valores repetidos', 0),
        (126, N'B', N'A busca passa a ser O(1)', 0),
        (126, N'C', N'Ela se transforma em uma pilha', 0),
        (126, N'D', N'Ela degenera em algo parecido com uma lista, e a busca vira O(n)', 1),
        (126, N'E', N'Ela fica perfeitamente balanceada', 0),
        (127, N'A', N'void preorder(noArvore *raiz) { if (raiz != NULL) { cout << raiz->dado << " "; preorder(raiz->esquerda); preorder(raiz->direita); } }', 1),
        (127, N'B', N'void preorder(noArvore *raiz) { if (raiz != NULL) { preorder(raiz->direita); preorder(raiz->esquerda); cout << raiz->dado << " "; } }', 0),
        (127, N'C', N'void preorder(noArvore *raiz) { if (raiz != NULL) { preorder(raiz->esquerda); cout << raiz->dado << " "; preorder(raiz->direita); } }', 0),
        (127, N'D', N'void preorder(noArvore *raiz) { if (raiz != NULL) { preorder(raiz->esquerda); preorder(raiz->direita); cout << raiz->dado << " "; } }', 0),
        (127, N'E', N'void preorder(noArvore *raiz) { if (raiz != NULL) { preorder(raiz->direita); cout << raiz->dado << " "; preorder(raiz->esquerda); } }', 0),
        (128, N'A', N'Linha 01; para i <- 1 até 9 faça', 0),
        (128, N'B', N'Linha 03; j <- i', 0),
        (128, N'C', N'Linha 04; enquanto ((j >= 1) ou (valor < vetor[j])) faça', 0),
        (128, N'D', N'Linha 05; vetor[j+1] <- vetor[j]', 1),
        (128, N'E', N'Linha 11; vetor[j] <- valor', 0),
        (129, N'A', N'O(n + 20)', 0),
        (129, N'B', N'O(5n)', 0),
        (129, N'C', N'O(1)', 0),
        (129, N'D', N'O(n²)', 0),
        (129, N'E', N'O(n)', 1),
        (130, N'A', N'À medida que o valor de n cresce, há um aumento no número de chamadas recursivas.', 0),
        (130, N'B', N'Na linha 4, a ordem de execução é calcular o valor para fibonacci(n-1) e somente depois calcular o valor para fibonacci(n-2).', 0),
        (130, N'C', N'O método recursivo é o mais eficiente para o cálculo do i-ésimo termo da sequência de Fibonacci, pois realiza duas chamadas por passo da recursão, cada uma mais simples do que a chamada original.', 1),
        (130, N'D', N'As condições de parada da recursão são: o valor de n é 0 ou o valor de n é 1.', 0),
        (130, N'E', N'O uso da recursão para o problema da série de Fibonacci não é indicado, pois ele gera rapidamente uma explosão de chamadas do método.', 0),
        (131, N'A', N'class', 0),
        (131, N'B', N'className', 1),
        (131, N'C', N'cssClass', 0),
        (131, N'D', N'classes', 0),
        (131, N'E', N'styleClass', 0),
        (132, N'A', N'Variáveis globais', 0),
        (132, N'B', N'Funções de estilo', 0),
        (132, N'C', N'Dados passados do componente pai para o filho, somente leitura', 1),
        (132, N'D', N'Estado interno do componente', 0),
        (132, N'E', N'Rotas da aplicação', 0),
        (133, N'A', N'useContext', 0),
        (133, N'B', N'useRef', 0),
        (133, N'C', N'useEffect', 0),
        (133, N'D', N'useProps', 0),
        (133, N'E', N'useState', 1),
        (134, N'A', N'Porque o JSX não aceita números', 0),
        (134, N'B', N'Porque o estado é uma constante do navegador', 0),
        (134, N'C', N'Atualiza normalmente', 0),
        (134, N'D', N'Porque o React só re-renderiza quando a função set é chamada', 1),
        (134, N'E', N'Porque é erro de sintaxe', 0),
        (135, N'A', N'Nunca', 0),
        (135, N'B', N'Somente na montagem do componente', 1),
        (135, N'C', N'Somente ao desmontar', 0),
        (135, N'D', N'A cada renderização', 0),
        (135, N'E', N'A cada clique', 0),
        (136, N'A', N'Para o React identificar qual item mudou entre renderizações', 1),
        (136, N'B', N'É opcional e não tem efeito', 0),
        (136, N'C', N'Para definir o estilo de cada item', 0),
        (136, N'D', N'Para criptografar os itens', 0),
        (136, N'E', N'Para ordenar a lista', 0),
        (137, N'A', N'Usar variáveis globais do window', 0),
        (137, N'B', N'Recarregar a página', 0),
        (137, N'C', N'Duplicar o estado nos dois', 0),
        (137, N'D', N'Ler o DOM diretamente', 0),
        (137, N'E', N'Elevar o estado para o componente pai comum', 1),
        (138, N'A', N'Erro de compilação', 0),
        (138, N'B', N'Olá, !', 0),
        (138, N'C', N'Olá, {nome}!', 0),
        (138, N'D', N'Ana', 0),
        (138, N'E', N'Olá, Ana!', 1),
        (139, N'A', N'Uma Promise', 0),
        (139, N'B', N'As dependências do efeito', 0),
        (139, N'C', N'Nada: ela é a função de limpeza executada antes da desmontagem', 1),
        (139, N'D', N'Um componente JSX', 0),
        (139, N'E', N'O novo estado', 0),
        (140, N'A', N'Porque o JavaScript exige', 0),
        (140, N'B', N'Por convenção estética apenas', 0),
        (140, N'C', N'Para o React diferenciar componentes próprios de tags HTML', 1),
        (140, N'D', N'Para melhorar o desempenho', 0),
        (140, N'E', N'Para funcionar no celular', 0);
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (141, N'A', N'INNER JOIN', 0),
        (141, N'B', N'LEFT JOIN', 1),
        (141, N'C', N'FULL JOIN somente', 0),
        (141, N'D', N'CROSS JOIN', 0),
        (141, N'E', N'SELF JOIN', 0),
        (142, N'A', N'SELECT YEAR(p.dt_pedido) AS ano, v.ds_nome AS vendedor, COUNT(DISTINCT p.id_pedido) AS qtd_vendas, SUM(ip.qt_produto * ip.vl_unitario) AS tot_vendas FROM tb_vendedor v INNER JOIN tb_pedido p ON v.id_vendedor = p.id_vendedor INNER JOIN tb_item_pedido ip ON p.id_pedido = ip.id_pedido GROUP BY year(p.dt_pedido), v.ds_nome ORDER BY year(p.dt_pedido), v.ds_nome;', 1),
        (142, N'B', N'SELECT YEAR(dt_pedido) AS ano, v.ds_nome AS vendedor, COUNT(DISTINCT p.id_pedido) AS qtd_vendas, SUM(ip.qt_produto * ip.vl_unitario) AS tot_vendas FROM tb_vendedor v INNER JOIN tb_pedido p ON v.id_vendedor = p.id_vendedor INNER JOIN tb_item_pedido ip ON p.id_pedido = ip.id_pedido GROUP BY dt_pedido, ds_nome ORDER BY year(p.dt_pedido), v.ds_nome;', 0),
        (142, N'C', N'SELECT YEAR(dt_pedido) AS ano, v.ds_nome AS vendedor, AVG(p.id_pedido) AS qtd_vendas, SUM(ip.qt_produto * ip.vl_unitario) AS tot_vendas FROM tb_vendedor v OUTER JOIN tb_pedido p ON v.id_vendedor = p.id_vendedor OUTER JOIN tb_item_pedido ip ON p.id_pedido = ip.id_pedido GROUP BY year(p.dt_pedido), v.ds_nome ORDER BY year(p.dt_pedido), v.ds_nome;', 0),
        (142, N'D', N'SELECT YEAR(p.dt_pedido) AS ano, v.ds_nome AS vendedor, COUNT(p.id_pedido) AS qtd_vendas, SUM(ip.qt_produto * ip.vl_unitario) AS tot_vendas FROM tb_vendedor v INNER JOIN tb_pedido p ON v.id_vendedor = p.id_vendedor INNER JOIN tb_item_pedido ip ON p.id_pedido = ip.id_pedido GROUP BY p.dt_pedido, v.ds_nome ORDER BY p.dt_pedido, v.ds_nome', 0),
        (142, N'E', N'SELECT YEAR(p.dt_pedido) AS ano, v.ds_nome AS vendedor, AVG(DISTINCT p.id_pedido) AS qtd_vendas, SUM(ip.qt_produto * ip.vl_unitario) AS tot_vendas FROM tb_pedido p INNER JOIN tb_item_pedido ip ON p.id_pedido = ip.id_pedido RIGHT JOIN tb_vendedor v ON p.id_vendedor = v.id_vendedor GROUP BY year(p.dt_pedido), v.ds_nome ORDER BY year(p.dt_pedido), v.ds_nome;', 0),
        (143, N'A', N'Um', 1),
        (143, N'B', N'Nenhum', 0),
        (143, N'C', N'Dois', 0),
        (143, N'D', N'Até 16', 0),
        (143, N'E', N'Ilimitados', 0),
        (144, N'A', N'A tabela perde a chave primária', 0),
        (144, N'B', N'As consultas ficam incorretas', 0),
        (144, N'C', N'O banco deixa de aceitar JOINs', 0),
        (144, N'D', N'As operações de escrita ficam mais lentas', 1),
        (144, N'E', N'Nenhum', 0),
        (145, N'A', N'ROW_NUMBER', 0),
        (145, N'B', N'COUNT', 0),
        (145, N'C', N'RANK', 1),
        (145, N'D', N'NTILE', 0),
        (145, N'E', N'DENSE_RANK', 0),
        (146, N'A', N'Porque YEAR não existe no SQL Server', 0),
        (146, N'B', N'Porque 2026 é um ano inválido', 0),
        (146, N'C', N'Porque a função aplicada à coluna impede a busca direta no índice', 1),
        (146, N'D', N'Porque índices não funcionam com datas', 0),
        (146, N'E', N'Porque SELECT * é proibido', 0),
        (147, N'A', N'Cria uma nova tabela', 0),
        (147, N'B', N'Cria um índice', 0),
        (147, N'C', N'Apaga linhas duplicadas', 0),
        (147, N'D', N'Divide as linhas em grupos para o cálculo sem juntá-las', 1),
        (147, N'E', N'Ordena o resultado final', 0),
        (148, N'A', N'EXISTS', 1),
        (148, N'B', N'LIKE', 0),
        (148, N'C', N'UNION', 0),
        (148, N'D', N'BETWEEN', 0),
        (148, N'E', N'IN com lista fixa', 0),
        (149, N'A', N'Que há erro de sintaxe', 0),
        (149, N'B', N'Que o banco está offline', 0),
        (149, N'C', N'Que a tabela está vazia', 0),
        (149, N'D', N'Que provavelmente falta um índice adequado', 1),
        (149, N'E', N'Que a consulta está perfeita', 0),
        (150, N'A', N'SELECT CL.NOME, HM.VAL_MOVIMENTADO FROM CLIENTE CL INNER JOIN CONTA CO ON CL.COD_CLIENTE = CO.COD_CLIENTE INNER JOIN HISTORICO_MOVIMENTACAO HM ON CO.NUM_CONTA = HM.NUM_CONTA ORDER BY HM.VAL_MOVIMENTADO', 0),
        (150, N'B', N'SELECT CL.NOME, AVG(HM.VAL_MOVIMENTADO) FROM CLIENTE CL INNER JOIN CONTA CO ON CL.COD_CLIENTE = CO.COD_CLIENTE INNER JOIN HISTORICO_MOVIMENTACAO HM ON CO.NUM_CONTA = HM.NUM_CONTA ORDER BY HM.VAL_MOVIMENTADO DESC', 0),
        (150, N'C', N'SELECT CL.NOME, AVG(HM.VAL_MOVIMENTADO) FROM CLIENTE CL INNER JOIN CONTA CO ON CL.COD_CLIENTE = CO.COD_CLIENTE INNER JOIN HISTORICO_MOVIMENTACAO HM ON CO.NUM_CONTA = HM.NUM_CONTA GROUP BY CL.COD_CLIENTE, CL.NOME ORDER BY AVG(HM.VAL_MOVIMENTADO) DESC', 1),
        (150, N'D', N'SELECT CL.NOME, AVG(HM.VAL_MOVIMENTADO) FROM CLIENTE CL LEFT OUTER JOIN CONTA CO ON CL.COD_CLIENTE = CO.NUM_CONTA LEFT OUTER JOIN HISTORICO_MOVIMENTACAO HM ON CO.NUM_CONTA = HM.COD_HISTORICO GROUP BY CL.NOME ORDER BY AVG(HM.VAL_MOVIMENTADO) DESC', 0),
        (150, N'E', N'SELECT CL.NOME, AVG(HM.VAL_MOVIMENTADO) FROM CLIENTE CL LEFT OUTER JOIN CONTA CO ON CL.COD_CLIENTE = CO.COD_CLIENTE LEFT OUTER JOIN HISTORICO_MOVIMENTACAO HM ON CO.NUM_CONTA = HM.NUM_CONTA GROUP BY CL.NOME ORDER BY AVG(HM.VAL_MOVIMENTADO) DESC', 0);

    -- ---------------------------------------------------------
    -- 6. Auditoria da publicacao dos cursos (mesmo texto do AdminService)
    -- ---------------------------------------------------------
    INSERT INTO Log_Auditoria (ID_ADM, Acao_Realizada, Data_Acao) VALUES
        (1, N'Curso 2 (Banco de Dados e Modelagem de Sistemas) alterado para Publicado', '2026-08-24T18:39:00'),
        (1, N'Curso 1 (Fundamentos de C#) alterado para Publicado', '2026-08-24T21:25:00'),
        (1, N'Curso 3 (Lógica de Programação com Python) alterado para Publicado', '2026-08-26T14:47:00'),
        (1, N'Curso 4 (HTML e CSS Responsivo) alterado para Publicado', '2026-08-27T14:13:00'),
        (1, N'Curso 5 (Git e GitHub na Prática) alterado para Publicado', '2026-08-28T20:24:00'),
        (1, N'Curso 6 (Engenharia de Requisitos e UML) alterado para Publicado', '2026-08-31T14:49:00'),
        (1, N'Curso 7 (Redes de Computadores) alterado para Publicado', '2026-09-01T20:50:00'),
        (1, N'Curso 8 (Orientação a Objetos com Java) alterado para Publicado', '2026-09-03T14:38:00'),
        (1, N'Curso 9 (JavaScript Moderno) alterado para Publicado', '2026-09-04T14:47:00'),
        (1, N'Curso 10 (Testes Automatizados) alterado para Publicado', '2026-09-08T21:48:00'),
        (1, N'Curso 11 (Fundamentos de Cloud Computing) alterado para Publicado', '2026-09-10T12:28:00'),
        (1, N'Curso 12 (Segurança da Informação) alterado para Publicado', '2026-09-14T13:40:00'),
        (1, N'Curso 13 (Estruturas de Dados e Algoritmos) alterado para Publicado', '2026-09-16T19:02:00');

    -- ---------------------------------------------------------
    -- 7. Matriculas (Historico), progresso, tentativas de prova e certificados
    -- ---------------------------------------------------------
    SET IDENTITY_INSERT Historico ON;
    INSERT INTO Historico (ID_Historico, ID_Estudante, ID_Curso, Status_Conclusao, Data_Conclusao) VALUES
        (1, 1, 1, N'Concluido', '2026-09-03'),
        (2, 1, 2, N'Concluido', '2026-09-13'),
        (3, 1, 3, N'Concluido', '2026-09-15'),
        (4, 1, 4, N'Concluido', '2026-09-18'),
        (5, 1, 5, N'Concluido', '2026-09-23'),
        (6, 1, 6, N'Concluido', '2026-09-30'),
        (7, 1, 7, N'Em andamento', NULL),
        (8, 1, 9, N'Em andamento', NULL),
        (9, 2, 2, N'Concluido', '2026-09-03'),
        (10, 2, 1, N'Concluido', '2026-09-05'),
        (11, 2, 3, N'Concluido', '2026-09-07'),
        (12, 2, 4, N'Concluido', '2026-09-06'),
        (13, 2, 5, N'Concluido', '2026-09-07'),
        (14, 2, 6, N'Concluido', '2026-09-09'),
        (15, 2, 7, N'Concluido', '2026-09-14'),
        (16, 2, 8, N'Concluido', '2026-09-21'),
        (17, 2, 9, N'Concluido', '2026-09-19'),
        (18, 2, 10, N'Concluido', '2026-09-23'),
        (19, 2, 11, N'Concluido', '2026-09-21'),
        (20, 2, 12, N'Concluido', '2026-09-22'),
        (21, 2, 13, N'Concluido', '2026-09-25'),
        (22, 3, 6, N'Concluido', '2026-09-09'),
        (23, 3, 4, N'Concluido', '2026-09-17'),
        (24, 3, 1, N'Concluido', '2026-09-26'),
        (25, 3, 5, N'Concluido', '2026-09-30'),
        (26, 3, 7, N'Concluido', '2026-10-05'),
        (27, 3, 3, N'Concluido', '2026-10-10'),
        (28, 3, 2, N'Concluido', '2026-10-17'),
        (29, 3, 10, N'Em andamento', NULL),
        (30, 4, 4, N'Concluido', '2026-09-05'),
        (31, 4, 7, N'Concluido', '2026-09-12'),
        (32, 4, 3, N'Concluido', '2026-09-15'),
        (33, 4, 1, N'Concluido', '2026-09-23'),
        (34, 4, 6, N'Concluido', '2026-09-21'),
        (35, 4, 5, N'Concluido', '2026-09-30'),
        (36, 4, 12, N'Em andamento', NULL),
        (37, 4, 9, N'Em andamento', NULL),
        (38, 5, 1, N'Concluido', '2026-09-12'),
        (39, 5, 6, N'Concluido', '2026-09-11'),
        (40, 5, 4, N'Concluido', '2026-09-15'),
        (41, 5, 7, N'Concluido', '2026-09-24'),
        (42, 5, 5, N'Concluido', '2026-10-01'),
        (43, 5, 3, N'Concluido', '2026-10-05'),
        (44, 5, 2, N'Em andamento', NULL),
        (45, 5, 12, N'Em andamento', NULL),
        (46, 6, 7, N'Concluido', '2026-09-11'),
        (47, 6, 4, N'Concluido', '2026-09-15'),
        (48, 6, 1, N'Concluido', '2026-09-25'),
        (49, 6, 3, N'Em andamento', NULL),
        (50, 6, 5, N'Em andamento', NULL),
        (51, 7, 1, N'Concluido', '2026-09-12'),
        (52, 7, 5, N'Concluido', '2026-09-22'),
        (53, 7, 4, N'Concluido', '2026-09-25'),
        (54, 7, 3, N'Em andamento', NULL),
        (55, 7, 6, N'Em andamento', NULL),
        (56, 8, 6, N'Concluido', '2026-09-10'),
        (57, 8, 1, N'Concluido', '2026-09-20'),
        (58, 8, 7, N'Concluido', '2026-09-21'),
        (59, 8, 3, N'Concluido', '2026-10-02'),
        (60, 8, 4, N'Em andamento', NULL);
    INSERT INTO Historico (ID_Historico, ID_Estudante, ID_Curso, Status_Conclusao, Data_Conclusao) VALUES
        (61, 8, 5, N'Em andamento', NULL),
        (62, 9, 4, N'Concluido', '2026-09-11'),
        (63, 9, 5, N'Concluido', '2026-09-19'),
        (64, 9, 6, N'Concluido', '2026-09-26'),
        (65, 9, 3, N'Concluido', '2026-10-02'),
        (66, 9, 1, N'Em andamento', NULL),
        (67, 9, 7, N'Em andamento', NULL),
        (68, 10, 1, N'Concluido', '2026-09-21'),
        (69, 10, 4, N'Concluido', '2026-09-20'),
        (70, 10, 5, N'Em andamento', NULL),
        (71, 10, 7, N'Em andamento', NULL),
        (72, 11, 6, N'Concluido', '2026-09-16'),
        (73, 11, 3, N'Concluido', '2026-09-21'),
        (74, 11, 1, N'Concluido', '2026-10-02'),
        (75, 11, 4, N'Concluido', '2026-10-05'),
        (76, 11, 5, N'Em andamento', NULL),
        (77, 11, 7, N'Em andamento', NULL),
        (78, 12, 6, N'Concluido', '2026-09-17'),
        (79, 12, 3, N'Concluido', '2026-09-25'),
        (80, 12, 7, N'Concluido', '2026-10-01'),
        (81, 12, 5, N'Concluido', '2026-10-07'),
        (82, 12, 4, N'Em andamento', NULL),
        (83, 12, 1, N'Em andamento', NULL),
        (84, 13, 3, N'Concluido', '2026-09-18'),
        (85, 13, 5, N'Concluido', '2026-09-25'),
        (86, 13, 7, N'Em andamento', NULL),
        (87, 13, 1, N'Em andamento', NULL),
        (88, 14, 7, N'Concluido', '2026-09-23'),
        (89, 14, 1, N'Concluido', '2026-10-02'),
        (90, 14, 5, N'Em andamento', NULL),
        (91, 14, 4, N'Em andamento', NULL),
        (92, 15, 4, N'Em andamento', NULL),
        (93, 15, 5, N'Em andamento', NULL),
        (94, 16, 6, N'Concluido', '2026-09-28'),
        (95, 16, 4, N'Em andamento', NULL),
        (96, 17, 5, N'Concluido', '2026-10-01'),
        (97, 17, 6, N'Em andamento', NULL),
        (98, 18, 6, N'Em andamento', NULL),
        (99, 18, 7, N'Em andamento', NULL),
        (100, 19, 4, N'Em andamento', NULL),
        (101, 19, 7, N'Em andamento', NULL),
        (102, 20, 3, N'Em andamento', NULL);
    SET IDENTITY_INSERT Historico OFF;

    INSERT INTO Progresso (ID_Estudante, ID_Material, Status_Conclusao, Data_Visualizacao, Porcentagem_Assistida) VALUES
        (1, 1, 1, '2026-08-26T20:25:00', 100),
        (1, 2, 1, '2026-08-27T06:25:00', 100),
        (2, 7, 1, '2026-08-27T21:31:32', 100),
        (1, 3, 1, '2026-08-29T00:25:00', 100),
        (4, 16, 1, '2026-08-30T00:25:57', 100),
        (2, 1, 1, '2026-08-30T00:31:32', 100),
        (1, 4, 1, '2026-08-30T03:25:00', 100),
        (2, 8, 1, '2026-08-30T09:31:32', 100),
        (1, 5, 1, '2026-08-30T18:25:00', 100),
        (2, 9, 1, '2026-08-31T01:31:32', 100),
        (1, 6, 1, '2026-08-31T16:25:00', 100),
        (4, 17, 1, '2026-08-31T16:25:57', 100),
        (2, 10, 1, '2026-08-31T17:31:32', 100),
        (2, 2, 1, '2026-08-31T17:31:32', 100),
        (2, 16, 1, '2026-08-31T19:31:32', 100),
        (2, 12, 1, '2026-08-31T20:31:32', 100),
        (2, 3, 1, '2026-09-01T07:31:32', 100),
        (2, 11, 1, '2026-09-01T11:31:32', 100),
        (2, 20, 1, '2026-09-01T12:31:32', 100),
        (2, 13, 1, '2026-09-01T13:31:32', 100),
        (5, 1, 1, '2026-09-01T18:13:49', 100),
        (1, 7, 1, '2026-09-02T01:25:00', 100),
        (2, 17, 1, '2026-09-02T01:31:32', 100),
        (2, 14, 1, '2026-09-02T02:31:32', 100),
        (2, 4, 1, '2026-09-02T12:31:32', 100),
        (5, 2, 1, '2026-09-02T19:13:49', 100),
        (4, 18, 1, '2026-09-03T02:25:57', 100),
        (3, 24, 1, '2026-09-03T07:49:00', 100),
        (2, 5, 1, '2026-09-03T08:31:32', 100),
        (2, 21, 1, '2026-09-03T08:31:32', 100),
        (2, 18, 1, '2026-09-03T13:31:32', 100),
        (1, 8, 1, '2026-09-03T14:25:00', 100),
        (2, 24, 1, '2026-09-04T03:31:32', 100),
        (3, 25, 1, '2026-09-04T03:49:00', 100),
        (5, 3, 1, '2026-09-04T07:13:49', 100),
        (2, 19, 1, '2026-09-04T10:31:32', 100),
        (4, 19, 1, '2026-09-04T12:25:57', 100),
        (2, 15, 1, '2026-09-04T14:31:32', 100),
        (2, 22, 1, '2026-09-04T14:31:32', 100),
        (6, 28, 1, '2026-09-04T14:32:39', 100),
        (2, 25, 1, '2026-09-04T16:31:32', 100),
        (2, 6, 1, '2026-09-04T20:31:32', 100),
        (3, 26, 1, '2026-09-04T20:49:00', 100),
        (7, 1, 1, '2026-09-05T01:26:32', 100),
        (8, 24, 1, '2026-09-05T06:16:41', 100),
        (5, 4, 1, '2026-09-05T08:13:49', 100),
        (4, 28, 1, '2026-09-05T11:50:00', 100),
        (1, 9, 1, '2026-09-05T14:25:00', 100),
        (2, 26, 1, '2026-09-05T20:31:32', 100),
        (20, 12, 1, '2026-09-06T03:48:38', 100),
        (2, 27, 1, '2026-09-06T11:31:32', 100),
        (8, 25, 1, '2026-09-06T13:16:41', 100),
        (9, 16, 1, '2026-09-06T15:02:32', 100),
        (3, 27, 1, '2026-09-06T17:49:00', 100),
        (4, 29, 1, '2026-09-06T18:50:00', 100),
        (6, 29, 1, '2026-09-06T19:32:39', 100),
        (7, 2, 1, '2026-09-06T22:26:32', 100),
        (5, 5, 1, '2026-09-07T00:13:49', 100),
        (2, 23, 1, '2026-09-07T00:31:32', 100),
        (20, 13, 0, '2026-09-07T08:48:38', 20),
        (5, 24, 1, '2026-09-07T09:13:49', 100),
        (1, 12, 1, '2026-09-07T16:25:00', 100),
        (1, 10, 1, '2026-09-07T18:25:00', 100),
        (6, 30, 1, '2026-09-07T20:32:39', 100),
        (4, 30, 1, '2026-09-07T22:50:00', 100),
        (5, 6, 1, '2026-09-08T00:13:49', 100),
        (8, 26, 1, '2026-09-08T01:16:41', 100),
        (2, 28, 1, '2026-09-08T02:31:32', 100),
        (9, 17, 1, '2026-09-08T09:02:32', 100),
        (7, 3, 1, '2026-09-08T09:26:32', 100),
        (5, 25, 1, '2026-09-08T10:13:49', 100),
        (4, 12, 1, '2026-09-08T13:50:00', 100),
        (10, 1, 1, '2026-09-08T14:25:34', 100),
        (2, 29, 1, '2026-09-08T18:31:32', 100),
        (8, 27, 1, '2026-09-08T19:16:41', 100),
        (7, 4, 1, '2026-09-08T19:26:32', 100),
        (5, 26, 1, '2026-09-09T00:13:49', 100),
        (6, 16, 1, '2026-09-09T09:32:39', 100),
        (6, 31, 1, '2026-09-09T11:32:39', 100),
        (9, 18, 1, '2026-09-09T17:02:32', 100),
        (7, 5, 1, '2026-09-09T18:26:32', 100),
        (1, 11, 1, '2026-09-09T20:25:00', 100),
        (1, 13, 1, '2026-09-10T03:25:00', 100),
        (11, 24, 1, '2026-09-10T04:35:28', 100),
        (4, 31, 1, '2026-09-10T07:50:00', 100),
        (5, 16, 1, '2026-09-10T13:13:49', 100),
        (8, 1, 1, '2026-09-10T13:16:41', 100),
        (9, 19, 1, '2026-09-10T15:02:32', 100),
        (3, 16, 1, '2026-09-10T17:49:00', 100),
        (4, 13, 1, '2026-09-10T19:50:00', 100),
        (2, 30, 1, '2026-09-11T01:31:32', 100),
        (10, 2, 1, '2026-09-11T02:25:34', 100),
        (5, 27, 1, '2026-09-11T08:13:49', 100),
        (6, 17, 1, '2026-09-11T08:32:39', 100),
        (11, 25, 1, '2026-09-11T08:35:28', 100),
        (8, 2, 1, '2026-09-11T10:16:41', 100),
        (2, 32, 1, '2026-09-11T14:31:32', 100),
        (5, 17, 1, '2026-09-11T16:13:49', 100),
        (12, 24, 1, '2026-09-11T18:10:50', 100),
        (4, 14, 1, '2026-09-11T18:50:00', 100);
    INSERT INTO Progresso (ID_Estudante, ID_Material, Status_Conclusao, Data_Visualizacao, Porcentagem_Assistida) VALUES
        (7, 6, 1, '2026-09-11T20:26:32', 100),
        (6, 18, 1, '2026-09-11T22:32:39', 100),
        (2, 31, 1, '2026-09-12T02:31:32', 100),
        (1, 14, 1, '2026-09-12T03:25:00', 100),
        (10, 3, 1, '2026-09-12T05:25:34', 100),
        (4, 15, 1, '2026-09-12T11:50:00', 100),
        (11, 26, 1, '2026-09-12T13:35:28', 100),
        (5, 18, 1, '2026-09-12T20:13:49', 100),
        (9, 20, 1, '2026-09-12T23:02:32', 100),
        (10, 4, 1, '2026-09-13T05:25:34', 100),
        (3, 17, 1, '2026-09-13T05:49:00', 100),
        (8, 3, 1, '2026-09-13T06:16:41', 100),
        (1, 16, 1, '2026-09-13T08:25:00', 100),
        (12, 25, 1, '2026-09-13T10:10:50', 100),
        (6, 19, 1, '2026-09-13T10:32:39', 100),
        (2, 33, 1, '2026-09-13T16:31:32', 100),
        (13, 12, 1, '2026-09-13T17:39:47', 100),
        (10, 16, 1, '2026-09-13T20:25:34', 100),
        (12, 26, 1, '2026-09-13T22:10:50', 100),
        (5, 19, 1, '2026-09-14T00:13:49', 100),
        (7, 20, 1, '2026-09-14T03:26:32', 100),
        (1, 17, 1, '2026-09-14T06:25:00', 100),
        (1, 15, 1, '2026-09-14T08:25:00', 100),
        (2, 36, 1, '2026-09-14T08:31:32', 100),
        (4, 1, 1, '2026-09-14T08:50:00', 100),
        (11, 27, 1, '2026-09-14T09:35:28', 100),
        (2, 34, 1, '2026-09-14T13:31:32', 100),
        (3, 18, 1, '2026-09-14T15:49:00', 100),
        (13, 13, 1, '2026-09-14T19:39:47', 100),
        (10, 5, 1, '2026-09-14T20:25:34', 100),
        (12, 27, 1, '2026-09-14T23:10:50', 100),
        (4, 2, 1, '2026-09-14T23:50:00', 100),
        (14, 28, 1, '2026-09-15T03:24:47', 100),
        (2, 37, 1, '2026-09-15T04:31:32', 100),
        (6, 1, 1, '2026-09-15T06:32:39', 100),
        (9, 21, 1, '2026-09-15T07:02:32', 100),
        (8, 4, 1, '2026-09-15T09:16:41', 100),
        (1, 18, 1, '2026-09-15T16:25:00', 100),
        (13, 14, 1, '2026-09-15T19:39:47', 100),
        (11, 12, 1, '2026-09-15T20:35:28', 100),
        (6, 2, 1, '2026-09-16T02:32:39', 100),
        (4, 24, 1, '2026-09-16T02:50:00', 100),
        (10, 17, 1, '2026-09-16T04:25:34', 100),
        (13, 15, 1, '2026-09-16T07:39:47', 100),
        (12, 12, 1, '2026-09-16T08:10:50', 100),
        (10, 6, 1, '2026-09-16T08:25:34', 100),
        (3, 19, 1, '2026-09-16T09:49:00', 100),
        (14, 29, 1, '2026-09-16T14:24:47', 100),
        (7, 21, 1, '2026-09-16T15:26:32', 100),
        (4, 3, 1, '2026-09-16T17:50:00', 100),
        (8, 5, 1, '2026-09-16T20:16:41', 100),
        (2, 35, 1, '2026-09-16T23:31:32', 100),
        (10, 18, 1, '2026-09-17T00:25:34', 100),
        (2, 40, 1, '2026-09-17T00:31:32', 100),
        (2, 44, 1, '2026-09-17T04:31:32', 100),
        (8, 28, 1, '2026-09-17T09:16:41', 100),
        (1, 20, 1, '2026-09-17T12:25:00', 100),
        (9, 22, 1, '2026-09-17T13:02:32', 100),
        (2, 41, 1, '2026-09-17T13:31:32', 100),
        (11, 13, 1, '2026-09-17T14:35:28', 100),
        (2, 38, 1, '2026-09-17T15:31:32', 100),
        (3, 1, 1, '2026-09-17T17:49:00', 100),
        (1, 19, 1, '2026-09-17T18:25:00', 100),
        (8, 29, 1, '2026-09-17T20:16:41', 100),
        (15, 16, 1, '2026-09-17T20:20:39', 100),
        (2, 45, 1, '2026-09-18T00:31:32', 100),
        (5, 28, 1, '2026-09-18T02:13:49', 100),
        (2, 48, 1, '2026-09-18T05:31:32', 100),
        (14, 30, 1, '2026-09-18T08:24:47', 100),
        (7, 22, 1, '2026-09-18T08:26:32', 100),
        (2, 39, 1, '2026-09-18T10:31:32', 100),
        (9, 23, 1, '2026-09-18T11:02:32', 100),
        (6, 3, 1, '2026-09-18T12:32:39', 100),
        (4, 25, 1, '2026-09-18T12:50:00', 100),
        (11, 14, 1, '2026-09-18T13:35:28', 100),
        (2, 46, 1, '2026-09-18T14:31:32', 100),
        (1, 21, 1, '2026-09-18T15:25:00', 100),
        (12, 13, 1, '2026-09-18T16:10:50', 100),
        (8, 6, 1, '2026-09-18T18:16:41', 100),
        (14, 31, 1, '2026-09-18T19:24:47', 100),
        (4, 4, 1, '2026-09-18T21:50:00', 100),
        (2, 49, 1, '2026-09-18T22:31:32', 100),
        (4, 26, 1, '2026-09-19T00:50:00', 100),
        (10, 19, 1, '2026-09-19T01:25:34', 100),
        (15, 17, 1, '2026-09-19T07:20:39', 100),
        (8, 30, 1, '2026-09-19T10:16:41', 100),
        (2, 42, 1, '2026-09-19T17:31:32', 100),
        (9, 24, 1, '2026-09-19T18:02:32', 100),
        (8, 31, 1, '2026-09-19T21:16:41', 100),
        (15, 18, 1, '2026-09-20T00:20:39', 100),
        (3, 2, 1, '2026-09-20T00:49:00', 100),
        (5, 29, 1, '2026-09-20T01:13:49', 100),
        (13, 20, 1, '2026-09-20T02:39:47', 100),
        (4, 27, 1, '2026-09-20T04:50:00', 100),
        (12, 14, 1, '2026-09-20T05:10:50', 100),
        (7, 16, 1, '2026-09-20T05:26:32', 100),
        (2, 50, 1, '2026-09-20T06:31:32', 100),
        (16, 24, 1, '2026-09-20T07:29:44', 100),
        (1, 22, 1, '2026-09-20T09:25:00', 100),
        (6, 4, 1, '2026-09-20T10:32:39', 100);
    INSERT INTO Progresso (ID_Estudante, ID_Material, Status_Conclusao, Data_Visualizacao, Porcentagem_Assistida) VALUES
        (2, 43, 1, '2026-09-20T11:31:32', 100),
        (4, 5, 1, '2026-09-20T11:50:00', 100),
        (9, 25, 1, '2026-09-20T15:02:32', 100),
        (7, 23, 1, '2026-09-20T15:26:32', 100),
        (7, 17, 1, '2026-09-20T15:26:32', 100),
        (11, 15, 1, '2026-09-20T16:35:28', 100),
        (2, 51, 1, '2026-09-20T17:31:32', 100),
        (2, 47, 1, '2026-09-20T19:31:32', 100),
        (16, 25, 1, '2026-09-20T22:29:44', 100),
        (15, 19, 1, '2026-09-21T00:20:39', 100),
        (1, 23, 1, '2026-09-21T05:25:00', 100),
        (2, 52, 1, '2026-09-21T05:31:32', 100),
        (3, 3, 1, '2026-09-21T08:49:00', 100),
        (14, 1, 1, '2026-09-21T11:24:47', 100),
        (4, 20, 1, '2026-09-21T15:50:00', 100),
        (13, 21, 1, '2026-09-21T20:39:47', 100),
        (11, 1, 1, '2026-09-21T23:35:28', 100),
        (12, 15, 1, '2026-09-22T00:10:50', 100),
        (7, 18, 1, '2026-09-22T00:26:32', 100),
        (16, 26, 1, '2026-09-22T04:29:44', 100),
        (2, 53, 1, '2026-09-22T05:31:32', 100),
        (6, 5, 1, '2026-09-22T05:32:39', 100),
        (5, 30, 1, '2026-09-22T07:13:49', 100),
        (12, 28, 1, '2026-09-22T08:10:50', 100),
        (4, 6, 1, '2026-09-22T09:50:00', 100),
        (9, 26, 1, '2026-09-22T14:02:32', 100),
        (14, 2, 1, '2026-09-22T17:24:47', 100),
        (6, 12, 1, '2026-09-22T19:32:39', 100),
        (16, 27, 1, '2026-09-22T23:29:44', 100),
        (3, 4, 1, '2026-09-23T00:49:00', 100),
        (1, 24, 1, '2026-09-23T04:25:00', 100),
        (4, 21, 1, '2026-09-23T04:50:00', 100),
        (5, 31, 1, '2026-09-23T10:13:49', 100),
        (3, 5, 1, '2026-09-23T10:49:00', 100),
        (10, 20, 1, '2026-09-23T18:25:34', 100),
        (2, 54, 1, '2026-09-23T19:31:32', 100),
        (12, 29, 1, '2026-09-24T01:10:50', 100),
        (5, 20, 1, '2026-09-24T01:13:49', 100),
        (7, 19, 1, '2026-09-24T01:26:32', 100),
        (13, 22, 1, '2026-09-24T02:39:47', 100),
        (9, 27, 1, '2026-09-24T07:02:32', 100),
        (14, 3, 1, '2026-09-24T07:24:47', 100),
        (13, 28, 1, '2026-09-24T10:39:47', 100),
        (11, 2, 1, '2026-09-24T11:35:28', 100),
        (6, 6, 1, '2026-09-24T13:32:39', 100),
        (2, 55, 1, '2026-09-24T15:31:32', 100),
        (6, 13, 1, '2026-09-24T16:32:39', 100),
        (14, 4, 1, '2026-09-24T17:24:47', 100),
        (3, 20, 1, '2026-09-24T23:49:00', 100),
        (13, 29, 1, '2026-09-25T00:39:47', 100),
        (13, 23, 1, '2026-09-25T04:39:47', 100),
        (12, 30, 1, '2026-09-25T05:10:50', 100),
        (7, 12, 1, '2026-09-25T08:26:32', 100),
        (8, 12, 1, '2026-09-25T10:16:41', 100),
        (10, 21, 1, '2026-09-25T14:25:34', 100),
        (12, 31, 1, '2026-09-25T15:10:50', 100),
        (1, 25, 1, '2026-09-25T15:25:00', 100),
        (4, 22, 1, '2026-09-25T15:50:00', 100),
        (5, 21, 1, '2026-09-25T16:13:49', 100),
        (3, 6, 1, '2026-09-25T17:49:00', 100),
        (7, 13, 0, '2026-09-26T03:26:32', 20),
        (11, 3, 1, '2026-09-26T03:35:28', 100),
        (14, 5, 1, '2026-09-26T04:24:47', 100),
        (10, 22, 1, '2026-09-26T04:25:34', 100),
        (17, 20, 1, '2026-09-26T04:37:22', 100),
        (6, 14, 1, '2026-09-26T12:32:39', 100),
        (13, 30, 1, '2026-09-26T19:39:47', 100),
        (17, 21, 1, '2026-09-26T22:37:22', 100),
        (1, 26, 1, '2026-09-26T23:25:00', 100),
        (13, 31, 1, '2026-09-27T05:39:47', 100),
        (4, 48, 1, '2026-09-27T07:50:00', 100),
        (6, 15, 0, '2026-09-27T09:32:39', 20),
        (3, 21, 1, '2026-09-27T10:49:00', 100),
        (4, 23, 1, '2026-09-27T15:50:00', 100),
        (16, 16, 1, '2026-09-27T19:29:44', 100),
        (8, 13, 1, '2026-09-27T20:16:41', 100),
        (5, 22, 1, '2026-09-27T23:13:49', 100),
        (11, 4, 1, '2026-09-28T01:35:28', 100),
        (18, 24, 1, '2026-09-28T07:28:03', 100),
        (12, 20, 1, '2026-09-28T08:10:50', 100),
        (3, 22, 1, '2026-09-28T08:49:00', 100),
        (14, 6, 1, '2026-09-28T09:24:47', 100),
        (1, 27, 1, '2026-09-28T10:25:00', 100),
        (18, 25, 0, '2026-09-28T12:28:03', 80),
        (9, 12, 1, '2026-09-28T14:02:32', 100),
        (5, 23, 1, '2026-09-28T14:13:49', 100),
        (10, 23, 1, '2026-09-28T16:25:34', 100),
        (17, 22, 1, '2026-09-29T04:37:22', 100),
        (5, 12, 1, '2026-09-29T05:13:49', 100),
        (1, 28, 1, '2026-09-29T11:25:00', 100),
        (3, 23, 1, '2026-09-29T11:49:00', 100),
        (5, 13, 1, '2026-09-29T15:13:49', 100),
        (8, 14, 1, '2026-09-29T18:16:41', 100),
        (11, 5, 1, '2026-09-29T18:35:28', 100),
        (3, 28, 1, '2026-09-29T23:49:00', 100),
        (16, 17, 1, '2026-09-30T03:29:44', 100),
        (12, 21, 1, '2026-09-30T07:10:50', 100),
        (14, 20, 1, '2026-09-30T07:24:47', 100),
        (9, 13, 1, '2026-09-30T10:02:32', 100),
        (17, 23, 1, '2026-09-30T13:37:22', 100);
    INSERT INTO Progresso (ID_Estudante, ID_Material, Status_Conclusao, Data_Visualizacao, Porcentagem_Assistida) VALUES
        (14, 21, 0, '2026-09-30T15:24:47', 80),
        (3, 29, 1, '2026-09-30T16:49:00', 100),
        (4, 36, 1, '2026-09-30T16:50:00', 100),
        (5, 14, 1, '2026-09-30T18:13:49', 100),
        (8, 15, 1, '2026-09-30T18:16:41', 100),
        (11, 16, 1, '2026-09-30T18:35:28', 100),
        (9, 14, 1, '2026-09-30T21:02:32', 100),
        (11, 17, 1, '2026-10-01T08:35:28', 100),
        (1, 29, 1, '2026-10-01T13:25:00', 100),
        (6, 20, 1, '2026-10-01T14:32:39', 100),
        (4, 37, 1, '2026-10-01T16:50:00', 100),
        (10, 28, 1, '2026-10-01T19:25:34', 100),
        (9, 15, 1, '2026-10-01T22:02:32', 100),
        (11, 6, 1, '2026-10-01T23:35:28', 100),
        (11, 18, 1, '2026-10-02T01:35:28', 100),
        (12, 22, 1, '2026-10-02T10:10:50', 100),
        (5, 15, 1, '2026-10-02T10:13:49', 100),
        (5, 7, 1, '2026-10-02T11:13:49', 100),
        (16, 18, 1, '2026-10-02T15:29:44', 100),
        (3, 30, 1, '2026-10-02T15:49:00', 100),
        (7, 24, 1, '2026-10-02T16:26:32', 100),
        (7, 25, 0, '2026-10-02T21:26:32', 80),
        (16, 19, 0, '2026-10-02T22:29:44', 20),
        (10, 29, 0, '2026-10-03T00:25:34', 20),
        (6, 21, 1, '2026-10-03T05:32:39', 100),
        (4, 38, 1, '2026-10-03T07:50:00', 100),
        (1, 30, 1, '2026-10-03T11:25:00', 100),
        (19, 16, 1, '2026-10-03T11:53:39', 100),
        (3, 31, 1, '2026-10-03T17:49:00', 100),
        (12, 23, 1, '2026-10-04T00:10:50', 100),
        (18, 28, 1, '2026-10-04T00:28:03', 100),
        (11, 19, 1, '2026-10-04T04:35:28', 100),
        (8, 16, 1, '2026-10-04T05:16:41', 100),
        (5, 8, 1, '2026-10-04T11:13:49', 100),
        (1, 31, 1, '2026-10-04T13:25:00', 100),
        (4, 39, 1, '2026-10-05T01:50:00', 100),
        (19, 17, 1, '2026-10-05T06:53:39', 100),
        (11, 20, 1, '2026-10-05T11:35:28', 100),
        (8, 17, 1, '2026-10-05T13:16:41', 100),
        (6, 22, 1, '2026-10-05T16:32:39', 100),
        (19, 18, 1, '2026-10-05T16:53:39', 100),
        (3, 12, 1, '2026-10-06T00:49:00', 100),
        (5, 9, 1, '2026-10-06T03:13:49', 100),
        (9, 1, 1, '2026-10-06T08:02:32', 100),
        (18, 29, 1, '2026-10-06T10:28:03', 100),
        (1, 36, 1, '2026-10-06T12:25:00', 100),
        (12, 16, 1, '2026-10-06T19:10:50', 100),
        (8, 18, 1, '2026-10-07T00:16:41', 100),
        (1, 37, 1, '2026-10-07T06:25:00', 100),
        (3, 13, 1, '2026-10-07T06:49:00', 100),
        (11, 21, 1, '2026-10-07T11:35:28', 100),
        (1, 38, 0, '2026-10-07T12:25:00', 35),
        (9, 2, 1, '2026-10-07T17:02:32', 100),
        (19, 19, 1, '2026-10-07T19:53:39', 100),
        (8, 19, 1, '2026-10-08T01:16:41', 100),
        (5, 10, 1, '2026-10-08T02:13:49', 100),
        (18, 30, 1, '2026-10-08T09:28:03', 100),
        (11, 22, 1, '2026-10-08T09:35:28', 100),
        (3, 14, 1, '2026-10-08T09:49:00', 100),
        (14, 16, 1, '2026-10-08T20:24:47', 100),
        (12, 17, 1, '2026-10-08T22:10:50', 100),
        (9, 3, 1, '2026-10-08T23:02:32', 100),
        (11, 23, 1, '2026-10-09T00:35:28', 100),
        (5, 11, 0, '2026-10-09T01:13:49', 65),
        (3, 15, 1, '2026-10-09T07:49:00', 100),
        (18, 31, 0, '2026-10-09T08:28:03', 65),
        (9, 4, 1, '2026-10-09T11:02:32', 100),
        (5, 48, 1, '2026-10-09T12:13:49', 100),
        (5, 49, 1, '2026-10-10T05:13:49', 100),
        (3, 7, 1, '2026-10-10T05:49:00', 100),
        (5, 50, 1, '2026-10-10T18:13:49', 100),
        (12, 1, 1, '2026-10-10T19:10:50', 100),
        (14, 17, 1, '2026-10-10T23:24:47', 100),
        (12, 18, 1, '2026-10-11T01:10:50', 100),
        (12, 2, 1, '2026-10-11T06:10:50', 100),
        (14, 18, 0, '2026-10-11T07:24:47', 50),
        (9, 28, 1, '2026-10-11T08:02:32', 100),
        (8, 20, 1, '2026-10-12T01:16:41', 100),
        (11, 28, 1, '2026-10-12T01:35:28', 100),
        (12, 3, 1, '2026-10-12T02:10:50', 100),
        (5, 51, 1, '2026-10-12T09:13:49', 100),
        (3, 8, 1, '2026-10-12T09:49:00', 100),
        (12, 19, 1, '2026-10-12T18:10:50', 100),
        (12, 4, 0, '2026-10-12T18:10:50', 20),
        (8, 21, 0, '2026-10-12T19:16:41', 35),
        (3, 9, 1, '2026-10-13T04:49:00', 100),
        (11, 29, 1, '2026-10-13T05:35:28', 100),
        (11, 30, 0, '2026-10-13T14:35:28', 20),
        (9, 29, 1, '2026-10-13T19:02:32', 100),
        (3, 10, 1, '2026-10-14T21:49:00', 100),
        (9, 30, 1, '2026-10-15T08:02:32', 100),
        (9, 31, 0, '2026-10-16T11:02:32', 20),
        (3, 40, 1, '2026-10-16T15:49:00', 100),
        (3, 11, 1, '2026-10-16T23:49:00', 100);

    INSERT INTO Desempenho (ID_Estudante, ID_Prova, Nota, Data_Realizacao, Tentativas) VALUES
        (1, 1, 10.00, '2026-09-03T04:25:00', 1),
        (2, 2, 9.00, '2026-09-03T13:31:32', 1),
        (4, 4, 8.00, '2026-09-05T14:25:57', 1),
        (2, 1, 10.00, '2026-09-05T21:31:32', 1),
        (2, 4, 8.00, '2026-09-06T03:31:32', 1),
        (2, 3, 10.00, '2026-09-07T08:31:32', 1),
        (2, 5, 9.00, '2026-09-07T09:31:32', 1),
        (5, 1, 5.00, '2026-09-09T05:13:49', 1),
        (2, 6, 8.00, '2026-09-09T07:31:32', 1),
        (3, 6, 8.00, '2026-09-09T16:49:00', 1),
        (8, 6, 8.00, '2026-09-10T00:16:41', 1),
        (1, 2, 6.00, '2026-09-11T12:25:00', 1),
        (6, 7, 7.00, '2026-09-11T12:32:39', 1),
        (9, 4, 7.00, '2026-09-11T15:02:32', 1),
        (5, 6, 8.00, '2026-09-11T21:13:49', 1),
        (5, 1, 9.00, '2026-09-12T05:13:49', 2),
        (7, 1, 7.00, '2026-09-12T12:26:32', 1),
        (4, 7, 8.00, '2026-09-12T16:50:00', 1),
        (1, 2, 8.00, '2026-09-13T20:25:00', 2),
        (2, 7, 9.00, '2026-09-14T18:31:32', 1),
        (4, 3, 8.00, '2026-09-15T07:50:00', 1),
        (1, 3, 9.00, '2026-09-15T10:25:00', 1),
        (5, 4, 8.00, '2026-09-15T23:13:49', 1),
        (6, 4, 7.00, '2026-09-15T23:32:39', 1),
        (11, 6, 8.00, '2026-09-16T20:35:28', 1),
        (3, 4, 8.00, '2026-09-17T10:49:00', 1),
        (12, 6, 7.00, '2026-09-17T14:10:50', 1),
        (13, 3, 8.00, '2026-09-18T00:39:47', 1),
        (2, 8, 6.00, '2026-09-18T06:31:32', 1),
        (1, 4, 8.00, '2026-09-18T11:25:00', 1),
        (2, 9, 9.00, '2026-09-19T00:31:32', 1),
        (9, 5, 8.00, '2026-09-19T01:02:32', 1),
        (10, 1, 6.00, '2026-09-19T04:25:34', 1),
        (8, 1, 9.00, '2026-09-20T11:16:41', 1),
        (10, 4, 7.00, '2026-09-20T20:25:34', 1),
        (14, 7, 6.00, '2026-09-21T01:24:47', 1),
        (7, 5, 5.00, '2026-09-21T02:26:32', 1),
        (8, 7, 9.00, '2026-09-21T03:16:41', 1),
        (2, 11, 8.00, '2026-09-21T03:31:32', 1),
        (2, 8, 10.00, '2026-09-21T04:31:32', 2),
        (10, 1, 7.00, '2026-09-21T09:25:34', 2),
        (11, 3, 7.00, '2026-09-21T18:35:28', 1),
        (4, 6, 8.00, '2026-09-21T19:50:00', 1),
        (15, 4, 6.00, '2026-09-21T21:20:39', 1),
        (2, 12, 8.00, '2026-09-22T00:31:32', 1),
        (7, 5, 7.00, '2026-09-22T04:26:32', 2),
        (15, 4, 6.00, '2026-09-22T15:20:39', 2),
        (12, 3, 6.00, '2026-09-22T16:10:50', 1),
        (2, 10, 10.00, '2026-09-23T06:31:32', 1),
        (1, 5, 10.00, '2026-09-23T11:25:00', 1),
        (14, 7, 8.00, '2026-09-23T17:24:47', 2),
        (4, 1, 10.00, '2026-09-23T19:50:00', 1),
        (12, 3, 4.00, '2026-09-24T09:10:50', 2),
        (5, 7, 10.00, '2026-09-24T20:13:49', 1),
        (2, 13, 9.00, '2026-09-25T08:31:32', 1),
        (12, 3, 8.00, '2026-09-25T10:10:50', 3),
        (7, 4, 8.00, '2026-09-25T12:26:32', 1),
        (6, 1, 8.00, '2026-09-25T14:32:39', 1),
        (13, 5, 8.00, '2026-09-25T19:39:47', 1),
        (16, 6, 5.00, '2026-09-25T22:29:44', 1);
    INSERT INTO Desempenho (ID_Estudante, ID_Prova, Nota, Data_Realizacao, Tentativas) VALUES
        (9, 6, 7.00, '2026-09-26T00:02:32', 1),
        (3, 1, 7.00, '2026-09-26T16:49:00', 1),
        (12, 7, 3.00, '2026-09-26T18:10:50', 1),
        (16, 6, 7.00, '2026-09-28T18:29:44', 2),
        (12, 7, 6.00, '2026-09-29T05:10:50', 2),
        (14, 1, 6.00, '2026-09-29T08:24:47', 1),
        (10, 5, 6.00, '2026-09-29T09:25:34', 1),
        (13, 7, 6.00, '2026-09-29T12:39:47', 1),
        (14, 1, 6.00, '2026-09-30T04:24:47', 2),
        (4, 5, 9.00, '2026-09-30T04:50:00', 1),
        (3, 5, 8.00, '2026-09-30T19:49:00', 1),
        (1, 6, 7.00, '2026-09-30T20:25:00', 1),
        (12, 7, 7.00, '2026-10-01T02:10:50', 3),
        (5, 5, 8.00, '2026-10-01T05:13:49', 1),
        (17, 5, 7.00, '2026-10-01T05:37:22', 1),
        (10, 5, 6.00, '2026-10-02T06:25:34', 2),
        (8, 3, 7.00, '2026-10-02T11:16:41', 1),
        (9, 3, 7.00, '2026-10-02T12:02:32', 1),
        (14, 1, 8.00, '2026-10-02T12:24:47', 3),
        (11, 1, 7.00, '2026-10-02T23:35:28', 1),
        (12, 5, 6.00, '2026-10-04T20:10:50', 1),
        (11, 4, 8.00, '2026-10-05T02:35:28', 1),
        (5, 3, 8.00, '2026-10-05T08:13:49', 1),
        (3, 7, 9.00, '2026-10-05T11:49:00', 1),
        (12, 5, 9.00, '2026-10-07T05:10:50', 2),
        (8, 4, 6.00, '2026-10-08T15:16:41', 1),
        (19, 4, 6.00, '2026-10-09T20:53:39', 1),
        (3, 3, 8.00, '2026-10-10T05:49:00', 1),
        (11, 5, 6.00, '2026-10-10T08:35:28', 1),
        (8, 4, 6.00, '2026-10-10T22:16:41', 2),
        (11, 5, 6.00, '2026-10-12T08:35:28', 2),
        (12, 4, 3.00, '2026-10-14T18:10:50', 1),
        (3, 2, 5.00, '2026-10-17T08:49:00', 1),
        (3, 2, 10.00, '2026-10-17T21:49:00', 2);

    -- Codigo_Autenticacao fica de fora: quem gera e o banco (DEFAULT NEWID()).
    INSERT INTO Certificado (ID_Historico, Data_Emissao) VALUES
        (1, '2026-09-03T04:25:00'),
        (9, '2026-09-03T13:31:32'),
        (30, '2026-09-05T14:25:57'),
        (10, '2026-09-05T21:31:32'),
        (12, '2026-09-06T03:31:32'),
        (11, '2026-09-07T08:31:32'),
        (13, '2026-09-07T09:31:32'),
        (14, '2026-09-09T07:31:32'),
        (22, '2026-09-09T16:49:00'),
        (56, '2026-09-10T00:16:41'),
        (46, '2026-09-11T12:32:39'),
        (62, '2026-09-11T15:02:32'),
        (39, '2026-09-11T21:13:49'),
        (38, '2026-09-12T05:13:49'),
        (51, '2026-09-12T12:26:32'),
        (31, '2026-09-12T16:50:00'),
        (2, '2026-09-13T20:25:00'),
        (15, '2026-09-14T18:31:32'),
        (32, '2026-09-15T07:50:00'),
        (3, '2026-09-15T10:25:00'),
        (40, '2026-09-15T23:13:49'),
        (47, '2026-09-15T23:32:39'),
        (72, '2026-09-16T20:35:28'),
        (23, '2026-09-17T10:49:00'),
        (78, '2026-09-17T14:10:50'),
        (84, '2026-09-18T00:39:47'),
        (4, '2026-09-18T11:25:00'),
        (17, '2026-09-19T00:31:32'),
        (63, '2026-09-19T01:02:32'),
        (57, '2026-09-20T11:16:41'),
        (69, '2026-09-20T20:25:34'),
        (58, '2026-09-21T03:16:41'),
        (19, '2026-09-21T03:31:32'),
        (16, '2026-09-21T04:31:32'),
        (68, '2026-09-21T09:25:34'),
        (73, '2026-09-21T18:35:28'),
        (34, '2026-09-21T19:50:00'),
        (20, '2026-09-22T00:31:32'),
        (52, '2026-09-22T04:26:32'),
        (18, '2026-09-23T06:31:32'),
        (5, '2026-09-23T11:25:00'),
        (88, '2026-09-23T17:24:47'),
        (33, '2026-09-23T19:50:00'),
        (41, '2026-09-24T20:13:49'),
        (21, '2026-09-25T08:31:32'),
        (79, '2026-09-25T10:10:50'),
        (53, '2026-09-25T12:26:32'),
        (48, '2026-09-25T14:32:39'),
        (85, '2026-09-25T19:39:47'),
        (64, '2026-09-26T00:02:32'),
        (24, '2026-09-26T16:49:00'),
        (94, '2026-09-28T18:29:44'),
        (35, '2026-09-30T04:50:00'),
        (25, '2026-09-30T19:49:00'),
        (6, '2026-09-30T20:25:00'),
        (80, '2026-10-01T02:10:50'),
        (42, '2026-10-01T05:13:49'),
        (96, '2026-10-01T05:37:22'),
        (59, '2026-10-02T11:16:41'),
        (65, '2026-10-02T12:02:32');
    INSERT INTO Certificado (ID_Historico, Data_Emissao) VALUES
        (89, '2026-10-02T12:24:47'),
        (74, '2026-10-02T23:35:28'),
        (75, '2026-10-05T02:35:28'),
        (43, '2026-10-05T08:13:49'),
        (26, '2026-10-05T11:49:00'),
        (81, '2026-10-07T05:10:50'),
        (27, '2026-10-10T05:49:00'),
        (28, '2026-10-17T21:49:00');

    -- ---------------------------------------------------------
    -- 8. Conquistas (automaticas pela regra + 3 concessoes manuais de tutor)
    -- ---------------------------------------------------------
    INSERT INTO Conquista (ID_Estudante, ID_Medalha, Data_Conquista) VALUES
        (1, 1, '2026-08-26T20:25:00'),
        (2, 1, '2026-08-27T21:31:32'),
        (4, 1, '2026-08-30T00:25:57'),
        (2, 2, '2026-09-01T11:31:32'),
        (5, 1, '2026-09-01T18:13:49'),
        (1, 4, '2026-09-03T04:25:00'),
        (1, 5, '2026-09-03T04:25:00'),
        (1, 6, '2026-09-03T04:25:00'),
        (3, 1, '2026-09-03T07:49:00'),
        (2, 4, '2026-09-03T13:31:32'),
        (2, 7, '2026-09-03T13:31:32'),
        (2, 3, '2026-09-04T10:31:32'),
        (6, 1, '2026-09-04T14:32:39'),
        (7, 1, '2026-09-05T01:26:32'),
        (8, 1, '2026-09-05T06:16:41'),
        (4, 4, '2026-09-05T14:25:57'),
        (4, 14, '2026-09-05T14:25:57'),
        (2, 5, '2026-09-05T21:31:32'),
        (2, 6, '2026-09-05T21:31:32'),
        (2, 14, '2026-09-06T03:31:32'),
        (20, 1, '2026-09-06T03:48:38'),
        (9, 1, '2026-09-06T15:02:32'),
        (2, 13, '2026-09-07T08:31:32'),
        (2, 15, '2026-09-07T09:31:32'),
        (1, 2, '2026-09-07T16:25:00'),
        (10, 1, '2026-09-08T14:25:34'),
        (2, 16, '2026-09-09T07:31:32'),
        (3, 4, '2026-09-09T16:49:00'),
        (3, 16, '2026-09-09T16:49:00'),
        (8, 4, '2026-09-10T00:16:41'),
        (8, 16, '2026-09-10T00:16:41'),
        (11, 1, '2026-09-10T04:35:28'),
        (5, 2, '2026-09-10T13:13:49'),
        (4, 2, '2026-09-10T19:50:00'),
        (6, 4, '2026-09-11T12:32:39'),
        (6, 17, '2026-09-11T12:32:39'),
        (9, 4, '2026-09-11T15:02:32'),
        (9, 14, '2026-09-11T15:02:32'),
        (12, 1, '2026-09-11T18:10:50'),
        (5, 4, '2026-09-11T21:13:49'),
        (5, 16, '2026-09-11T21:13:49'),
        (5, 6, '2026-09-12T05:13:49'),
        (7, 4, '2026-09-12T12:26:32'),
        (7, 6, '2026-09-12T12:26:32'),
        (1, 9, '2026-09-12T12:50:40'),
        (4, 17, '2026-09-12T16:50:00'),
        (13, 1, '2026-09-13T17:39:47'),
        (1, 7, '2026-09-13T20:25:00'),
        (2, 17, '2026-09-14T18:31:32'),
        (14, 1, '2026-09-15T03:24:47'),
        (4, 13, '2026-09-15T07:50:00'),
        (1, 13, '2026-09-15T10:25:00'),
        (5, 14, '2026-09-15T23:13:49'),
        (6, 14, '2026-09-15T23:32:39'),
        (6, 2, '2026-09-16T02:32:39'),
        (11, 4, '2026-09-16T20:35:28'),
        (11, 16, '2026-09-16T20:35:28'),
        (8, 2, '2026-09-17T09:16:41'),
        (3, 14, '2026-09-17T10:49:00'),
        (12, 4, '2026-09-17T14:10:50'),
        (12, 16, '2026-09-17T14:10:50'),
        (1, 3, '2026-09-17T18:25:00'),
        (15, 1, '2026-09-17T20:20:39'),
        (13, 4, '2026-09-18T00:39:47'),
        (13, 13, '2026-09-18T00:39:47'),
        (1, 14, '2026-09-18T11:25:00'),
        (2, 19, '2026-09-19T00:31:32'),
        (9, 15, '2026-09-19T01:02:32'),
        (10, 2, '2026-09-19T01:25:34'),
        (3, 2, '2026-09-20T00:49:00'),
        (4, 3, '2026-09-20T04:50:00'),
        (7, 2, '2026-09-20T05:26:32'),
        (16, 1, '2026-09-20T07:29:44'),
        (8, 6, '2026-09-20T11:16:41'),
        (9, 2, '2026-09-20T15:02:32'),
        (10, 4, '2026-09-20T20:25:34'),
        (10, 14, '2026-09-20T20:25:34'),
        (8, 17, '2026-09-21T03:16:41'),
        (2, 21, '2026-09-21T03:31:32'),
        (2, 18, '2026-09-21T04:31:32'),
        (10, 6, '2026-09-21T09:25:34'),
        (11, 13, '2026-09-21T18:35:28'),
        (4, 16, '2026-09-21T19:50:00'),
        (2, 22, '2026-09-22T00:31:32'),
        (7, 15, '2026-09-22T04:26:32'),
        (2, 20, '2026-09-23T06:31:32'),
        (1, 15, '2026-09-23T11:25:00'),
        (14, 4, '2026-09-23T17:24:47'),
        (14, 17, '2026-09-23T17:24:47'),
        (4, 5, '2026-09-23T19:50:00'),
        (4, 6, '2026-09-23T19:50:00'),
        (12, 2, '2026-09-24T01:10:50'),
        (11, 2, '2026-09-24T11:35:28'),
        (5, 5, '2026-09-24T20:13:49'),
        (5, 17, '2026-09-24T20:13:49'),
        (13, 2, '2026-09-25T04:39:47'),
        (2, 23, '2026-09-25T08:31:32'),
        (2, 8, '2026-09-25T08:31:32'),
        (12, 13, '2026-09-25T10:10:50'),
        (7, 14, '2026-09-25T12:26:32');
    INSERT INTO Conquista (ID_Estudante, ID_Medalha, Data_Conquista) VALUES
        (6, 6, '2026-09-25T14:32:39'),
        (5, 3, '2026-09-25T16:13:49'),
        (13, 15, '2026-09-25T19:39:47'),
        (9, 16, '2026-09-26T00:02:32'),
        (17, 1, '2026-09-26T04:37:22'),
        (3, 6, '2026-09-26T16:49:00'),
        (18, 1, '2026-09-28T07:28:03'),
        (14, 2, '2026-09-28T09:24:47'),
        (16, 4, '2026-09-28T18:29:44'),
        (16, 16, '2026-09-28T18:29:44'),
        (4, 15, '2026-09-30T04:50:00'),
        (3, 3, '2026-09-30T16:49:00'),
        (3, 15, '2026-09-30T19:49:00'),
        (1, 16, '2026-09-30T20:25:00'),
        (12, 17, '2026-10-01T02:10:50'),
        (5, 15, '2026-10-01T05:13:49'),
        (17, 4, '2026-10-01T05:37:22'),
        (17, 15, '2026-10-01T05:37:22'),
        (8, 13, '2026-10-02T11:16:41'),
        (9, 13, '2026-10-02T12:02:32'),
        (14, 6, '2026-10-02T12:24:47'),
        (11, 6, '2026-10-02T23:35:28'),
        (19, 1, '2026-10-03T11:53:39'),
        (11, 14, '2026-10-05T02:35:28'),
        (5, 13, '2026-10-05T08:13:49'),
        (2, 10, '2026-10-05T11:05:27'),
        (3, 17, '2026-10-05T11:49:00'),
        (8, 3, '2026-10-05T13:16:41'),
        (6, 3, '2026-10-05T16:32:39'),
        (12, 15, '2026-10-07T05:10:50'),
        (11, 3, '2026-10-07T11:35:28'),
        (3, 11, '2026-10-08T11:10:42'),
        (9, 3, '2026-10-09T11:02:32'),
        (3, 13, '2026-10-10T05:49:00'),
        (12, 3, '2026-10-11T01:10:50'),
        (3, 5, '2026-10-17T21:49:00'),
        (3, 7, '2026-10-17T21:49:00');

    -- ---------------------------------------------------------
    -- 9. Chamados: duvidas (ao tutor do curso), respostas, tecnicos (fila dos
    --    administradores, sem destinatario), mensagens e parecer de rejeicao
    -- ---------------------------------------------------------
    INSERT INTO Chamado (ID_Remetente, ID_Destinatario, ID_Curso, Tipo_Chamado, Assunto, Descricao, Data_Abertura, Status_Chamado) VALUES
        (7, 2, 1, N'Duvida', N'Diferença entre List e array', N'Na aula de Arrays e Listas ficou a dúvida: quando vale a pena usar array em vez de List<T> se a List é mais flexível?', '2026-08-28T09:25:00', N'Resolvido'),
        (2, 7, 1, N'Resposta', N'Re: Diferença entre List e array', N'Use array quando o tamanho é conhecido e não muda, como os 7 dias da semana. A List<T> cobra uma cópia interna quando cresce; para coleções que mudam, ela é a escolha padrão.', '2026-08-28T23:25:00', N'Resolvido'),
        (13, 2, 1, N'Duvida', N'Erro IndexOutOfRangeException', N'Meu laço for dá IndexOutOfRangeException na última volta. Estou usando i <= numeros.Length. O que está errado?', '2026-09-05T18:26:32', N'Aberto'),
        (10, 6, 7, N'Duvida', N'Cálculo de hosts no /26', N'Pelo meu cálculo uma rede /26 tem 64 hosts, mas o gabarito da aula fala em 62. Qual está certo?', '2026-09-05T19:50:00', N'Resolvido'),
        (6, 10, 7, N'Resposta', N'Re: Cálculo de hosts no /26', N'Os dois números aparecem: são 64 endereços, mas o primeiro identifica a rede e o último é o de broadcast. Sobram 62 hosts utilizáveis.', '2026-09-06T04:50:00', N'Resolvido'),
        (15, 4, 4, N'Duvida', N'Media query não funciona no celular', N'Coloquei min-width: 768px mas no meu celular o layout continua igual ao do computador.', '2026-09-08T05:02:32', N'Resolvido'),
        (4, 15, 4, N'Resposta', N'Re: Media query não funciona no celular', N'Confira se a meta tag viewport está no head. Sem ela, o navegador do celular simula uma tela larga e a media query é aplicada.', '2026-09-09T07:02:32', N'Resolvido'),
        (16, 5, 5, N'Duvida', N'Conflito no merge', N'Apareceram uns <<<<<<< no meu arquivo depois do merge. Posso apagar só essas linhas?', '2026-09-22T11:25:34', N'Resolvido'),
        (5, 16, 5, N'Resposta', N'Re: Conflito no merge', N'Pode, mas antes escolha qual versão do trecho fica (ou combine as duas). Depois remova os marcadores, faça git add e conclua com git commit.', '2026-09-23T07:25:34', N'Resolvido'),
        (14, 3, 3, N'Duvida', N'range não inclui o último número?', N'Por que range(1, 10) não mostra o 10? Achei que era um erro do meu código.', '2026-09-26T08:16:41', N'Resolvido'),
        (3, 14, 3, N'Resposta', N'Re: range não inclui o último número?', N'Não é erro: o range para antes do valor final. Para incluir o 10, use range(1, 11).', '2026-09-26T22:16:41', N'Resolvido'),
        (10, 6, 12, N'Duvida', N'BCrypt e sal', N'Se o sal fica guardado junto do hash, o atacante não consegue usá-lo também?', '2026-09-29T09:50:00', N'Em andamento'),
        (6, 8, NULL, N'Mensagem', N'Mensagem do tutor', N'Parabéns por concluir todos os cursos de infraestrutura! Você recebeu a medalha Maratonista.', '2026-10-05T16:04:27', N'Aberto'),
        (2, 15, NULL, N'Mensagem', N'Mensagem do tutor', N'Vi que seu progresso em Fundamentos de C# parou há alguns dias. Se travou em algum ponto, abra uma dúvida pelo curso que eu respondo.', '2026-10-06T16:32:30', N'Aberto'),
        (7, 4, 9, N'Duvida', N'Quando usar async/await', N'Ainda não entendi se preciso usar .then() mesmo usando async/await. Posso misturar os dois?', '2026-10-07T06:25:00', N'Aberto'),
        (18, NULL, NULL, N'Tecnico', N'Não consigo baixar o certificado', N'Ao clicar em imprimir certificado no celular, a página abre em branco.', '2026-10-09T13:04:20', N'Resolvido'),
        (9, 2, 2, N'Duvida', N'Dúvida sobre 3FN', N'Na tabela de endereço, guardar cidade e estado juntos quebra a 3FN mesmo se eu nunca mudar o nome da cidade?', '2026-10-10T05:49:00', N'Aberto'),
        (1, 18, NULL, N'Resposta', N'Re: Não consigo baixar o certificado', N'No celular, use a opção Compartilhar do navegador e escolha Imprimir ou Salvar como PDF. Atualizamos a orientação na página do certificado.', '2026-10-10T22:56:52', N'Resolvido'),
        (5, NULL, NULL, N'Tecnico', N'Pré-visualização da aula com código', N'Na pré-visualização do editor de aulas, os trechos de código perdem a indentação.', '2026-10-11T11:26:44', N'Aberto'),
        (1, 2, NULL, N'Avaliacao', N'Curso rejeitado: SQL Avançado e Otimização de Consultas', N'A aula "Índices e plano de execução" precisa de um exemplo com o plano de execução antes e depois da criação do índice, e a questão 6 deve explicar a alternativa correta no enunciado de revisão. Ajuste e submeta novamente.', '2026-10-13T13:23:00', N'Aberto'),
        (21, NULL, NULL, N'Tecnico', N'Vídeo da aula não aparece', N'Procurei o vídeo da aula de Python e só aparece texto. O vídeo foi removido?', '2026-10-14T11:00:42', N'Em andamento'),
        (23, NULL, NULL, N'Tecnico', N'Erro ao alterar e-mail', N'Tentei trocar meu e-mail e apareceu a mensagem de que ele já está em uso, mas é um e-mail novo.', '2026-10-16T11:30:54', N'Aberto');

    -- ---------------------------------------------------------
    -- 10. Desativacao de uma estudante: o UPDATE dispara a trigger
    --     TRG_Auditoria_StatusUsuario, que grava o log sozinha. A data do
    --     log e ajustada depois porque a trigger usa GETDATE().
    -- ---------------------------------------------------------
    UPDATE Usuario SET Status_Usuario = 0 WHERE ID_Usuario = 26;
    UPDATE Log_Auditoria SET Data_Acao = '2026-10-02T15:45:35'
    WHERE ID_Log = (SELECT MAX(ID_Log) FROM Log_Auditoria);

    -- Rejeicao do curso 15: log com o ADM real (o AdminService grava assim)
    INSERT INTO Log_Auditoria (ID_ADM, Acao_Realizada, Data_Acao)
    VALUES (1, N'Curso 15 (SQL Avançado e Otimização de Consultas) alterado para Rejeitado', '2026-10-13T13:23:00');

    COMMIT TRANSACTION;
    PRINT 'Carga concluida.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- =============================================================
-- CONFERENCIA
--
-- As consultas de "regra" devolvem ZERO linhas quando os dados estao
-- coerentes com as regras do back-end. Qualquer linha que aparecer e uma
-- inconsistencia, com o motivo na primeira coluna.
-- =============================================================

-- Contagem por tabela
SELECT 'Usuario' AS Tabela, COUNT(*) AS Linhas FROM Usuario
UNION ALL SELECT 'ADM', COUNT(*) FROM ADM
UNION ALL SELECT 'Tutor', COUNT(*) FROM Tutor
UNION ALL SELECT 'Estudante', COUNT(*) FROM Estudante
UNION ALL SELECT 'Curso', COUNT(*) FROM Curso
UNION ALL SELECT 'Material', COUNT(*) FROM Material
UNION ALL SELECT 'Prova', COUNT(*) FROM Prova
UNION ALL SELECT 'Questao', COUNT(*) FROM Questao
UNION ALL SELECT 'Alternativas', COUNT(*) FROM Alternativas
UNION ALL SELECT 'Medalha', COUNT(*) FROM Medalha
UNION ALL SELECT 'Historico', COUNT(*) FROM Historico
UNION ALL SELECT 'Progresso', COUNT(*) FROM Progresso
UNION ALL SELECT 'Desempenho', COUNT(*) FROM Desempenho
UNION ALL SELECT 'Certificado', COUNT(*) FROM Certificado
UNION ALL SELECT 'Conquista', COUNT(*) FROM Conquista
UNION ALL SELECT 'Chamado', COUNT(*) FROM Chamado
UNION ALL SELECT 'Log_Auditoria', COUNT(*) FROM Log_Auditoria;

-- Regras: cada consulta abaixo deve voltar VAZIA
SELECT 'Prova sem exatamente 10 questoes' AS Problema, p.ID_Prova AS Id
FROM Prova p LEFT JOIN Questao q ON q.ID_Prova = p.ID_Prova
GROUP BY p.ID_Prova HAVING COUNT(q.ID_Questao) <> 10
UNION ALL
SELECT 'Questao sem exatamente 1 alternativa correta', q.ID_Questao
FROM Questao q JOIN Alternativas a ON a.ID_Questao = q.ID_Questao
GROUP BY q.ID_Questao HAVING SUM(CAST(a.Eh_Correta AS INT)) <> 1
UNION ALL
SELECT 'Matricula em curso nao publicado', h.ID_Historico
FROM Historico h JOIN Curso c ON c.ID_Curso = h.ID_Curso
WHERE c.Status_Curso <> 'Publicado'
UNION ALL
SELECT 'Certificado sem curso concluido', ce.ID_Certificado
FROM Certificado ce JOIN Historico h ON h.ID_Historico = ce.ID_Historico
WHERE h.Status_Conclusao <> 'Concluido'
UNION ALL
SELECT 'Curso concluido sem certificado', h.ID_Historico
FROM Historico h
WHERE h.Status_Conclusao = 'Concluido'
  AND NOT EXISTS (SELECT 1 FROM Certificado ce WHERE ce.ID_Historico = h.ID_Historico)
UNION ALL
-- RegrasConclusaoCurso: todos os materiais concluidos E prova aprovada
SELECT 'Concluido sem todos os materiais ou sem aprovacao', h.ID_Historico
FROM Historico h JOIN Curso c ON c.ID_Curso = h.ID_Curso
WHERE h.Status_Conclusao = 'Concluido'
  AND (   (SELECT COUNT(*) FROM Material m WHERE m.ID_Curso = c.ID_Curso)
       <> (SELECT COUNT(*) FROM Progresso pr JOIN Material m ON m.ID_Material = pr.ID_Material
           WHERE m.ID_Curso = c.ID_Curso AND pr.ID_Estudante = h.ID_Estudante AND pr.Status_Conclusao = 1)
       OR NOT EXISTS (SELECT 1 FROM Desempenho d JOIN Prova p ON p.ID_Prova = d.ID_Prova
                      WHERE d.ID_Prova = c.ID_Prova AND d.ID_Estudante = h.ID_Estudante
                        AND d.Nota >= p.Nota_Minima))
UNION ALL
SELECT 'Aprovado com todos os materiais, mas nao concluido', h.ID_Historico
FROM Historico h JOIN Curso c ON c.ID_Curso = h.ID_Curso
WHERE h.Status_Conclusao <> 'Concluido'
  AND (SELECT COUNT(*) FROM Material m WHERE m.ID_Curso = c.ID_Curso)
    = (SELECT COUNT(*) FROM Progresso pr JOIN Material m ON m.ID_Material = pr.ID_Material
       WHERE m.ID_Curso = c.ID_Curso AND pr.ID_Estudante = h.ID_Estudante AND pr.Status_Conclusao = 1)
  AND EXISTS (SELECT 1 FROM Desempenho d JOIN Prova p ON p.ID_Prova = d.ID_Prova
              WHERE d.ID_Prova = c.ID_Prova AND d.ID_Estudante = h.ID_Estudante AND d.Nota >= p.Nota_Minima)
UNION ALL
SELECT 'Prova feita sem matricula no curso', d.ID_Desempenho
FROM Desempenho d JOIN Curso c ON c.ID_Prova = d.ID_Prova
WHERE NOT EXISTS (SELECT 1 FROM Historico h WHERE h.ID_Estudante = d.ID_Estudante AND h.ID_Curso = c.ID_Curso)
UNION ALL
SELECT 'Medalha de curso sem o curso concluido', cq.ID_Conquista
FROM Conquista cq JOIN Medalha m ON m.ID_Medalha = cq.ID_Medalha
WHERE m.ID_Curso IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Historico h WHERE h.ID_Estudante = cq.ID_Estudante
                  AND h.ID_Curso = m.ID_Curso AND h.Status_Conclusao = 'Concluido')
UNION ALL
SELECT 'Curso concluido sem a medalha do curso', h.ID_Historico
FROM Historico h JOIN Medalha m ON m.ID_Curso = h.ID_Curso
WHERE h.Status_Conclusao = 'Concluido'
  AND NOT EXISTS (SELECT 1 FROM Conquista cq WHERE cq.ID_Estudante = h.ID_Estudante AND cq.ID_Medalha = m.ID_Medalha)
UNION ALL
SELECT 'Medalha repetida para o mesmo estudante', MIN(cq.ID_Conquista)
FROM Conquista cq GROUP BY cq.ID_Estudante, cq.ID_Medalha HAVING COUNT(*) > 1
UNION ALL
SELECT 'Duvida que nao foi ao tutor do curso', ch.ID_Chamado
FROM Chamado ch JOIN Curso c ON c.ID_Curso = ch.ID_Curso JOIN Tutor t ON t.ID_Tutor = c.ID_Tutor_Criou
WHERE ch.Tipo_Chamado = 'Duvida' AND ch.ID_Destinatario <> t.ID_Usuario
UNION ALL
SELECT 'Chamado tecnico com destinatario fixo', ch.ID_Chamado
FROM Chamado ch WHERE ch.Tipo_Chamado = 'Tecnico' AND ch.ID_Destinatario IS NOT NULL;

-- Ranking calculado como a API calcula (RegrasGamificacao):
-- XP = 50 por material + 100 por prova aprovada + 50 por nota 10 + 75 por medalha
-- Nivel = 1 + XP / 350
SELECT u.Nome_Usuario AS Estudante,
       x.Materiais, x.ProvasAprovadas, x.NotasMaximas, x.Medalhas,
       x.Materiais * 50 + x.ProvasAprovadas * 100 + x.NotasMaximas * 50 + x.Medalhas * 75 AS XP,
       1 + (x.Materiais * 50 + x.ProvasAprovadas * 100 + x.NotasMaximas * 50 + x.Medalhas * 75) / 350 AS Nivel,
       (SELECT COUNT(*) FROM Historico h WHERE h.ID_Estudante = e.ID_Estudante AND h.Status_Conclusao = 'Concluido') AS CursosConcluidos
FROM Estudante e
JOIN Usuario u ON u.ID_Usuario = e.ID_Usuario
CROSS APPLY (SELECT
    (SELECT COUNT(*) FROM Progresso p WHERE p.ID_Estudante = e.ID_Estudante AND p.Status_Conclusao = 1) AS Materiais,
    (SELECT COUNT(DISTINCT d.ID_Prova) FROM Desempenho d JOIN Prova pr ON pr.ID_Prova = d.ID_Prova
      WHERE d.ID_Estudante = e.ID_Estudante AND d.Nota >= pr.Nota_Minima) AS ProvasAprovadas,
    (SELECT COUNT(DISTINCT d.ID_Prova) FROM Desempenho d WHERE d.ID_Estudante = e.ID_Estudante AND d.Nota >= 10) AS NotasMaximas,
    (SELECT COUNT(*) FROM Conquista c WHERE c.ID_Estudante = e.ID_Estudante) AS Medalhas
) x
ORDER BY XP DESC;

-- Acentuacao: se alguma linha aparecer, o arquivo foi colado com codificacao errada.
SELECT TOP 20 'Questao' AS Origem, Enunciado AS Texto FROM Questao
WHERE Enunciado LIKE '%' + CHAR(195) + '%'
UNION ALL
SELECT 'Material', Titulo_Material FROM Material
WHERE Titulo_Material LIKE '%' + CHAR(195) + '%';
