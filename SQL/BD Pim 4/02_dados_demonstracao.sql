-- =============================================================
-- 02_dados_demonstracao.sql  |  Tech Quest - carga de demonstracao
--
-- Roda DEPOIS de 01_estrutura.sql, em banco vazio.
--
-- IDEMPOTENTE por tabela: cada bloco so insere se a tabela estiver
-- vazia. Rodar duas vezes nao duplica nem quebra; rodar num banco que
-- ja tem dados nao mexe neles.
--
-- SENHAS (BCrypt, fator de trabalho 11 -- o mesmo de BCryptHashService):
--   Felipe Almeida         felipe123@gmail.com    senha: aluno123@
--   Ana Souza              tutor123@gmail.com     senha: tutor123@
--   Carlos Mendes          admin123@gmail.com     senha: admin123@
--
-- Os hashes abaixo sao reais e verificam com BCrypt.Net. Gerados uma vez;
-- o BCrypt embute o sal em cada hash, por isso dois hashes da MESMA senha
-- sao diferentes e ambos validos -- nao se compara hash com hash, se
-- verifica a senha contra o hash.
--
-- ACENTUACAO: este arquivo e UTF-8. Os literais usam prefixo N para o
-- SQL Server interpretar como Unicode. A conferencia no fim detecta
-- texto corrompido por codificacao errada.
-- =============================================================

SET NOCOUNT ON;

-- -------------------------------------------------------------
-- 1. Usuarios e suas especializacoes
--
-- O papel NAO e uma coluna em Usuario: e a presenca de linha em ADM,
-- Tutor ou Estudante. Por isso cada usuario entra em duas tabelas.
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Usuario)
BEGIN
    INSERT INTO Usuario (Nome_Usuario, Email_Usuario, Senha_Usuario, Status_Usuario)
    VALUES (N'Felipe Almeida', N'felipe123@gmail.com', '$2b$11$hJGB7As2N0z0dNAyqzta.errVRKmhjkRo5I96pyLTJQn5YjCFiE7C', 1);
    INSERT INTO Estudante (ID_Usuario) VALUES (SCOPE_IDENTITY());

    INSERT INTO Usuario (Nome_Usuario, Email_Usuario, Senha_Usuario, Status_Usuario)
    VALUES (N'Ana Souza', N'tutor123@gmail.com', '$2b$11$oMMqH6O271RgwpJ7n9NEoehkyw7qTuzczcSsSlKFBOoQctzJMCzNu', 1);
    INSERT INTO Tutor (ID_Usuario) VALUES (SCOPE_IDENTITY());

    INSERT INTO Usuario (Nome_Usuario, Email_Usuario, Senha_Usuario, Status_Usuario)
    VALUES (N'Carlos Mendes', N'admin123@gmail.com', '$2b$11$951xpD5760ZdZQ/MQF5jEuvnGVsaIAgxU6Kq7m1jM1NHHsFx4YszG', 1);
    INSERT INTO Adm (ID_Usuario) VALUES (SCOPE_IDENTITY());

END

-- -------------------------------------------------------------
-- 2. Catalogo de medalhas
-- Os criterios aqui sao TEXTO para o aluno ler. Quem decide a concessao
-- e RegrasMedalhas, no projeto de dominio -- o banco nao avalia regra.
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Medalha)
BEGIN
    INSERT INTO Medalha (Nome_Medalha, Raridade, Descricao_Medalha) VALUES
        (N'Primeiro Passo', N'comum', N'Conclua seu primeiro material'),
        (N'Estudante Dedicado', N'comum', N'Conclua 10 materiais'),
        (N'Bibliotecário', N'raro', N'Conclua 20 materiais'),
        (N'Quiz Champion', N'comum', N'Seja aprovado em uma prova'),
        (N'Pontuação Perfeita', N'épico', N'Tire nota 10 em uma prova'),
        (N'Mestre em C#', N'raro', N'Conclua um curso de C#'),
        (N'DBA Iniciante', N'raro', N'Conclua um curso de Banco de Dados'),
        (N'Lenda da Tech Quest', N'lendário', N'Conclua todos os cursos publicados'),
        (N'Mestre em Arrays', N'épico', N'Concedida pelo tutor'),
        (N'Maratonista', N'épico', N'Concedida pelo tutor'),
        (N'POO Master', N'épico', N'Concedida pelo tutor'),
        (N'Arquiteto SOLID', N'épico', N'Concedida pelo tutor');
END

-- -------------------------------------------------------------
-- 3. Cursos, provas, materiais e questoes
--
-- A prova e criada ANTES do curso: a chave estrangeira esta em
-- Curso.ID_Prova, entao o curso precisa do id da prova para nascer.
-- -------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Curso)
BEGIN
    DECLARE @idTutor INT = (SELECT TOP 1 ID_Tutor FROM Tutor ORDER BY ID_Tutor);
    DECLARE @idProva INT, @idCurso INT, @idQuestao INT;

    -- ---- Fundamentos de C# ----
    INSERT INTO Prova (Titulo_Prova, Nota_Minima, Tempo_Minutos)
    VALUES (N'Prova: Arrays e Listas em C#', 7.00, 30);
    SET @idProva = SCOPE_IDENTITY();

    INSERT INTO Curso (Nome_Curso, Descricao_Curso, Categoria_Curso, Nivel_Curso,
                       Duracao_Horas, ID_Tutor_Criou, ID_Prova, Status_Curso)
    VALUES (N'Fundamentos de C#', N'Domine as bases da linguagem mais poderosa da Microsoft. Do zero ao seu primeiro software profissional com orientacao a objetos e boas praticas.', N'Programacao', N'Iniciante', 12, @idTutor, @idProva, N'Publicado');
    SET @idCurso = SCOPE_IDENTITY();

    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Introducao ao C# e ao .NET', N'texto', N'C# e uma linguagem de programacao orientada a objetos criada pela Microsoft e executada sobre a plataforma .NET. O codigo que voce escreve nao vai direto para o processador: ele e compilado para uma linguagem intermediaria (IL) e so no momento da execucao o runtime traduz essa IL para instrucoes da maquina.

Essa etapa extra explica duas caracteristicas da linguagem. A primeira e a portabilidade: o mesmo programa roda em Windows, Linux e macOS porque quem conhece o sistema operacional e o runtime, nao o seu codigo. A segunda e a verificacao de tipos antes da execucao, que transforma em erro de compilacao o que em linguagens dinamicas so apareceria com o programa rodando.

O .NET nao e so o runtime: e tambem a biblioteca padrao. Colecoes, acesso a arquivos, rede e acesso a banco de dados ja vem prontos, e e dessa biblioteca que vem o Console.WriteLine que voce usa no primeiro programa.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Variaveis e Tipos de Dados', N'texto', N'Toda variavel em C# tem um tipo declarado, e esse tipo nao muda. Escrever int idade = 25 cria um espaco que guarda numeros inteiros e so numeros inteiros; atribuir um texto ali e erro de compilacao, nao erro em tempo de execucao.

Os tipos se dividem em dois grupos com comportamentos diferentes. Tipos de VALOR (int, double, decimal, bool, char, e structs) guardam o dado em si; copiar a variavel copia o conteudo. Tipos de REFERENCIA (string, arrays, classes) guardam o endereco de onde o dado esta; copiar a variavel copia o endereco, e as duas passam a apontar para o mesmo objeto. Confundir os dois e a origem de uma classe inteira de bugs.

Sobre decimal e double: os dois guardam numeros com virgula, mas double usa base binaria e introduz pequenos erros de arredondamento, enquanto decimal usa base decimal. Para dinheiro, use decimal -- um centavo perdido por arredondamento e um centavo a explicar.

A palavra var nao cria tipo dinamico: ela pede ao compilador que descubra o tipo a partir do valor atribuido. var nome = "Ana" e exatamente string nome = "Ana".');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Estruturas de Controle', N'texto', N'Estruturas de controle sao o que permite ao programa tomar caminhos diferentes. O if avalia uma condicao booleana e executa um bloco ou outro. O switch compara um mesmo valor contra varias possibilidades e e mais legivel que uma sequencia de if/else quando a comparacao e sempre sobre a mesma variavel.

Para repeticao, a escolha depende do que se sabe antes de comecar. Use for quando a quantidade de voltas e conhecida (percorrer dez posicoes de um array). Use while quando a repeticao depende de uma condicao que pode nem ser verdadeira na primeira vez. Use do/while quando o bloco precisa executar ao menos uma vez antes do teste. Use foreach para percorrer uma colecao inteira sem controlar o indice -- e a forma mais segura, porque nao da para errar o limite.

Dois comandos alteram o fluxo de dentro do laco: break encerra a repeticao e continue pula para a proxima volta. Ambos resolvem casos legitimos, mas um laco cheio deles costuma ser sinal de que a condicao poderia ser escrita melhor.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Arrays e Listas em C#', N'texto', N'Arrays sao colecoes de TAMANHO FIXO que armazenam elementos do mesmo tipo. Ao declarar int[] numeros = new int[5] voce reserva exatamente cinco posicoes: nao e possivel acrescentar a sexta. O acesso e por indice comecando em zero, e pedir uma posicao fora do intervalo lanca IndexOutOfRangeException.

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

O <T> e um parametro de tipo: List<string> so aceita texto, List<int> so aceita inteiros. E o compilador que garante isso, o que evita a conversao de tipo que as colecoes antigas exigiam.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Orientacao a Objetos: Classes e Objetos', N'texto', N'Uma classe e a definicao de um tipo: quais dados ele guarda e o que ele sabe fazer. Um objeto e uma instancia dessa definicao. A classe Aluno existe uma vez no codigo; os objetos Aluno existem um por aluno, cada um com seus proprios valores.

Encapsulamento e o primeiro dos quatro pilares, e o mais pratico: os campos ficam privados e o acesso passa por propriedades. Isso permite validar na entrada -- rejeitar uma nota fora de 0 a 10, por exemplo -- em um unico lugar, em vez de confiar que todo codigo que mexe no objeto vai lembrar da regra.

Heranca permite que uma classe reaproveite outra: Tutor e Estudante sao Usuario e herdam nome e e-mail. Polimorfismo permite que o mesmo metodo se comporte de forma diferente em cada classe filha. Abstracao e o resultado dos tres: quem usa a classe trabalha com o que ela faz, nao com como ela faz.

Heranca e menos usada do que parece. Quando a relacao nao e claramente "e um tipo de", composicao -- a classe conter outra como campo -- costuma produzir um desenho mais facil de mudar.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Exercicios do Modulo 3', N'exercicio', N'Lista de exercicios sobre arrays, listas e estruturas de repeticao. Resolva no seu proprio ambiente antes de fazer a prova.

1. Declare um array de dez inteiros, preencha com os numeros de 1 a 10 e imprima a soma.

2. Dada uma List<string> com nomes, imprima apenas os que comecam com a letra A, sem usar LINQ.

3. Escreva um metodo que receba uma List<int> e devolva o maior valor sem usar Max(). Trate o caso da lista vazia.

4. Explique, em duas ou tres linhas, por que o codigo abaixo lanca excecao:

    int[] n = new int[3];
    for (int i = 0; i <= 3; i++) { n[i] = i; }

5. Converta um array int[] para List<int> e depois de volta para array. Diga o que acontece com a memoria em cada conversao.

6. Crie uma List<string>, adicione cinco itens, remova o terceiro e imprima Count antes e depois. Explique o resultado.

As respostas nao sao entregues pela plataforma: a verificacao e a prova do curso, que cobre os mesmos conceitos.');

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Considere o código abaixo. Qual será a saída ao executá-lo?', N'List<int> numeros = new List<int> { 5, 10, 15, 20, 25 };
int soma = 0;
foreach (int n in numeros) {
    if (n > 10) soma += n;
}
Console.WriteLine(soma);', 1);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'50', 0),
        (@idQuestao, N'B', N'60', 1),
        (@idQuestao, N'C', N'45', 0),
        (@idQuestao, N'D', N'15', 0),
        (@idQuestao, N'E', N'75', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Qual a diferença fundamental entre Array e List<T> em C#?', NULL, 2);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'Array tem tamanho fixo definido na criação; List<T> é dinâmica e cresce conforme necessário.', 1),
        (@idQuestao, N'B', N'Array é sempre mais rápida em qualquer operação.', 0),
        (@idQuestao, N'C', N'List<T> só aceita tipos primitivos.', 0),
        (@idQuestao, N'D', N'Não há diferença, são sinônimos.', 0),
        (@idQuestao, N'E', N'Array não permite tipos genéricos como List<T>.', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Qual método é utilizado para adicionar um elemento ao final de uma List<T> em C#?', NULL, 3);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'Insert()', 0),
        (@idQuestao, N'B', N'Push()', 0),
        (@idQuestao, N'C', N'Add()', 1),
        (@idQuestao, N'D', N'Append()', 0),
        (@idQuestao, N'E', N'Set()', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Em C#, qual a forma correta de declarar um array de strings com 5 posições?', NULL, 4);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'string[5] nomes;', 0),
        (@idQuestao, N'B', N'string[] nomes = new string[5];', 1),
        (@idQuestao, N'C', N'Array<string> nomes(5);', 0),
        (@idQuestao, N'D', N'string nomes[5] = new string;', 0),
        (@idQuestao, N'E', N'List<string>(5) nomes;', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Qual a complexidade de tempo (Big-O) da operação Add() em uma List<T> no pior caso (quando precisa redimensionar internamente)?', NULL, 5);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'O(1)', 0),
        (@idQuestao, N'B', N'O(log n)', 0),
        (@idQuestao, N'C', N'O(n)', 1),
        (@idQuestao, N'D', N'O(n²)', 0),
        (@idQuestao, N'E', N'O(2^n)', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Qual instrução em C# percorre todos os elementos de uma coleção sem precisar de índice numérico?', NULL, 6);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'for', 0),
        (@idQuestao, N'B', N'while', 0),
        (@idQuestao, N'C', N'do-while', 0),
        (@idQuestao, N'D', N'foreach', 1),
        (@idQuestao, N'E', N'switch', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Qual namespace deve ser importado para usar List<T> em C#?', NULL, 7);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'System.Linq', 0),
        (@idQuestao, N'B', N'System.IO', 0),
        (@idQuestao, N'C', N'System.Collections.Generic', 1),
        (@idQuestao, N'D', N'System.Text', 0),
        (@idQuestao, N'E', N'System.Data', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Considere o código:

int[] num = { 1, 2, 3, 4, 5 };
Console.WriteLine(num.Length);

Qual o resultado?', NULL, 8);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'4', 0),
        (@idQuestao, N'B', N'5', 1),
        (@idQuestao, N'C', N'6', 0),
        (@idQuestao, N'D', N'Erro de compilação', 0),
        (@idQuestao, N'E', N'0', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Em C#, qual método remove o primeiro elemento que satisfaz a condição em uma List<T>?', NULL, 9);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'Remove()', 1),
        (@idQuestao, N'B', N'Delete()', 0),
        (@idQuestao, N'C', N'Clear()', 0),
        (@idQuestao, N'D', N'Pop()', 0),
        (@idQuestao, N'E', N'Drop()', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Qual a forma mais idiomática (recomendada) em C# para verificar se uma lista está vazia?', NULL, 10);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'if (lista == null)', 0),
        (@idQuestao, N'B', N'if (lista.Length == 0)', 0),
        (@idQuestao, N'C', N'if (lista.Count == 0)', 1),
        (@idQuestao, N'D', N'if (lista.Size() == 0)', 0),
        (@idQuestao, N'E', N'if (lista.isEmpty())', 0);

    -- ---- Banco de Dados e Modelagem de Sistemas ----
    INSERT INTO Prova (Titulo_Prova, Nota_Minima, Tempo_Minutos)
    VALUES (N'Prova: Análise de Sistemas e Banco de Dados', 7.00, 30);
    SET @idProva = SCOPE_IDENTITY();

    INSERT INTO Curso (Nome_Curso, Descricao_Curso, Categoria_Curso, Nivel_Curso,
                       Duracao_Horas, ID_Tutor_Criou, ID_Prova, Status_Curso)
    VALUES (N'Banco de Dados e Modelagem de Sistemas', N'Domine modelagem de dados, SQL e os fundamentos de analise de sistemas, RUP e tecnicas de elicitacao de requisitos.', N'Dados', N'Intermediario', 18, @idTutor, @idProva, N'Publicado');
    SET @idCurso = SCOPE_IDENTITY();

    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Modelo Entidade-Relacionamento', N'texto', N'O Modelo Entidade-Relacionamento (MER) descreve O QUE o sistema precisa guardar, antes de decidir COMO guardar. Ele tem tres elementos: entidades (as coisas sobre as quais se guarda informacao -- Usuario, Curso, Material), atributos (o que se sabe sobre cada uma -- nome, titulo, data) e relacionamentos (como elas se ligam -- um Curso TEM varios Materiais).

A cardinalidade do relacionamento e a parte que mais decide o desenho final. Um-para-muitos (1:N) resolve-se colocando a chave estrangeira no lado N: Material guarda ID_Curso, e nao o contrario, porque um material pertence a um curso so. Muitos-para-muitos (N:N) nao tem como ser resolvido com uma chave estrangeira em nenhum dos lados -- precisa de uma tabela intermediaria. No Tech Quest, "estudante faz cursos" e N:N, e e por isso que existe a tabela Historico: ela nao e uma tabela auxiliar, e a materializacao do relacionamento, e ganha atributos proprios (data de conclusao, situacao).

O MER e conceitual: nao fala de tipo de dado, de indice nem de banco especifico. A traducao para tabelas, colunas e chaves e o modelo LOGICO, e so depois vem o fisico, com tipos e indices. Pular a etapa conceitual e comum e custa caro: erro de cardinalidade descoberto depois que ha dado gravado exige migracao, nao edicao.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Normalizacao de Dados', N'texto', N'Normalizar e organizar as colunas de forma que cada fato fique guardado em um lugar so. O objetivo nao e estetico: e eliminar tres anomalias concretas.

Anomalia de atualizacao: se o nome do tutor estivesse repetido em cada curso dele, mudar o nome exigiria acertar todas as linhas -- e esquecer uma deixa o banco se contradizendo. Anomalia de insercao: se os dados do curso estivessem na mesma tabela da matricula, nao daria para cadastrar um curso antes do primeiro aluno. Anomalia de exclusao: apagar a ultima matricula apagaria junto o curso.

As tres primeiras formas normais resolvem a maioria dos casos. 1FN: cada coluna guarda um valor unico, sem lista dentro de um campo (nada de "C#, SQL, UML" numa coluna so). 2FN: estando na 1FN, nenhuma coluna depende de apenas parte de uma chave composta. 3FN: estando na 2FN, nenhuma coluna depende de outra coluna que nao seja a chave -- se Cidade e Estado estao na mesma tabela, Estado depende de Cidade, nao da chave, e pede tabela propria.

Desnormalizar e aceitavel, mas e uma troca consciente: repete-se um dado para evitar um JOIN caro, assumindo a responsabilidade de manter as copias em sincronia. Fazer isso sem ter normalizado antes nao e otimizacao, e desorganizacao.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'SQL: DDL e DML', N'texto', N'SQL divide-se em subconjuntos pelo que cada comando faz. Os dois do dia a dia sao DDL e DML, e confundi-los e a origem de acidentes serios.

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

Ha ainda DCL (GRANT e REVOKE, permissoes) e TCL (BEGIN TRANSACTION, COMMIT, ROLLBACK). O TCL e o que garante que duas gravacoes que precisam valer juntas -- gravar a nota e conceder a medalha, por exemplo -- nao deixem o banco pela metade se a segunda falhar.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Elicitacao de Requisitos e RUP', N'texto', N'Elicitar requisitos e descobrir o que o sistema precisa fazer. "Descobrir" e a palavra certa: o cliente raramente sabe enunciar o que precisa, e o que ele pede costuma ser uma solucao que ele imaginou, nao o problema que tem.

Requisitos funcionais dizem O QUE o sistema faz ("o tutor submete o curso para aprovacao"). Nao funcionais dizem COMO ele se comporta -- desempenho, seguranca, disponibilidade, usabilidade ("a senha e armazenada com hash"). Os nao funcionais sao os mais esquecidos e os mais caros de acrescentar depois, porque costumam atravessar a arquitetura inteira.

As tecnicas de elicitacao se combinam: entrevista (profundidade, pouca abrangencia), questionario (o inverso), observacao do trabalho real (revela o que ninguem lembra de contar), analise de documentos e prototipacao (que transforma a discussao abstrata em algo que o usuario aponta e corrige).

O RUP (Rational Unified Process) organiza isso em quatro fases -- Concepcao, Elaboracao, Construcao e Transicao -- percorridas em iteracoes, nao em sequencia unica. A diferenca para o modelo cascata e essa: em todas as fases se levanta requisito, se modela, se programa e se testa; o que muda e a PROPORCAO. A Concepcao e majoritariamente levantamento; a Construcao, majoritariamente programacao. Entregar algo executavel cedo e proposital: e o que permite descobrir requisito errado enquanto corrigir ainda e barato.');
    INSERT INTO Material (ID_Curso, Titulo_Material, Tipo_Material, Conteudo)
    VALUES (@idCurso, N'Estudo de Caso: Modelagem Completa', N'exercicio', N'Exercicio de modelagem usando a propria plataforma Tech Quest como objeto de estudo. Resolva no papel antes de olhar o banco.

1. Liste as entidades necessarias para que um estudante se matricule num curso, estude os materiais, faca a prova e receba certificado. Para cada uma, diga quais atributos ela guarda.

2. Para cada par de entidades relacionadas, determine a cardinalidade e diga onde fica a chave estrangeira. Justifique: por que a chave de Curso fica em Material, e nao o contrario?

3. "Estudante faz cursos" e um relacionamento muitos-para-muitos. Modele a tabela que o materializa e identifique quais atributos pertencem ao RELACIONAMENTO, e nao a nenhuma das duas entidades.

4. Um usuario pode ser estudante, tutor ou administrador. Compare duas solucoes: (a) uma coluna Tipo na tabela Usuario; (b) uma tabela de especializacao por papel, com chave estrangeira para Usuario. Diga qual permite guardar atributos proprios de cada papel e o que acontece, em cada uma, quando aparece um quarto papel.

5. A nota da prova fica em qual tabela, e por que ela nao pode ficar em Estudante nem em Prova?

6. Verifique se o seu modelo esta na 3FN. Se alguma coluna depende de outra coluna que nao e chave, aponte qual e separe.

7. Escreva o DDL das tabelas do item 1, com chaves primarias, estrangeiras e as restricoes NOT NULL que voce considerar necessarias.

Depois de resolver, compare com o script de criacao do banco do projeto. Onde o seu modelo divergir, a pergunta certa nao e qual esta certo -- e qual decisao cada um tomou, e o que ela custa.');

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Considere a seguinte situação hipotética: uma classe ''Veículo'' possui atributos como cor, placa e ano. Foram criadas duas novas classes — ''Carro'' e ''Moto'' — que reaproveitam todos esses atributos e adicionam características próprias. Que conceito de orientação a objetos está descrito na situação hipotética acima?', NULL, 1);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'Sobrecarga', 0),
        (@idQuestao, N'B', N'Herança', 1),
        (@idQuestao, N'C', N'Sobreposição', 0),
        (@idQuestao, N'D', N'Abstração', 0),
        (@idQuestao, N'E', N'Mensagem', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Com relação às máquinas virtuais (MV), assinale a opção correta.', NULL, 2);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'A MV permite acesso direto ao hardware.', 0),
        (@idQuestao, N'B', N'A MV economiza CPU e memória RAM.', 0),
        (@idQuestao, N'C', N'A MV oferece maior controle de segurança.', 1),
        (@idQuestao, N'D', N'Uma MV dual-core exige CPU dual-core física.', 0),
        (@idQuestao, N'E', N'Sistemas em MV são imunes a vírus.', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Considere as asserções sobre o RUP (Rational Unified Process):

I — O RUP é um processo iterativo e incremental.
II — Uma iteração do RUP entrega sempre o sistema 100% finalizado.

Sobre a iteração do RUP, assinale a opção correta.', NULL, 3);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'As duas asserções são verdadeiras, e a segunda justifica a primeira.', 0),
        (@idQuestao, N'B', N'As duas asserções são verdadeiras, e a segunda não justifica a primeira.', 0),
        (@idQuestao, N'C', N'A primeira é verdadeira e a segunda é falsa.', 1),
        (@idQuestao, N'D', N'A primeira é falsa e a segunda é verdadeira.', 0),
        (@idQuestao, N'E', N'As duas asserções são falsas.', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Um grupo de alunos precisava levantar requisitos para um novo sistema acadêmico. Reuniram-se livremente em uma sala onde todos podiam falar abertamente, sem julgamentos imediatos, gerando o maior número possível de ideias e sugestões. A estratégia utilizada pelo grupo de alunos é uma adaptação de qual técnica?', NULL, 4);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'JAD', 0),
        (@idQuestao, N'B', N'PIECES', 0),
        (@idQuestao, N'C', N'FAST', 0),
        (@idQuestao, N'D', N'Entrevista', 0),
        (@idQuestao, N'E', N'Brainstorming', 1);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Considere o algoritmo abaixo de busca em um vetor ordenado:

I — Possui complexidade O(log n) quando implementado como busca binária.
II — Funciona corretamente apenas em vetores ordenados.
III — Divide o espaço de busca pela metade a cada iteração.

Com relação ao algoritmo apresentado, assinale a opção correta.', NULL, 5);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'Apenas um item está certo.', 0),
        (@idQuestao, N'B', N'Apenas os itens I e II estão certos.', 0),
        (@idQuestao, N'C', N'Apenas os itens I e III estão certos.', 0),
        (@idQuestao, N'D', N'Apenas os itens II e III estão certos.', 0),
        (@idQuestao, N'E', N'Todos os itens estão certos.', 1);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Em modelagem de banco de dados relacional, o que é uma chave primária?', NULL, 6);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'Um atributo que identifica unicamente cada registro de uma tabela.', 1),
        (@idQuestao, N'B', N'Um atributo que pode se repetir livremente.', 0),
        (@idQuestao, N'C', N'Um campo opcional usado apenas para indexação.', 0),
        (@idQuestao, N'D', N'Um relacionamento entre duas tabelas distintas.', 0),
        (@idQuestao, N'E', N'Uma restrição visual aplicada no front-end.', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'Qual cláusula SQL é utilizada para filtrar registros baseados em uma condição?', NULL, 7);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'FROM', 0),
        (@idQuestao, N'B', N'ORDER BY', 0),
        (@idQuestao, N'C', N'GROUP BY', 0),
        (@idQuestao, N'D', N'WHERE', 1),
        (@idQuestao, N'E', N'HAVING', 0);

    INSERT INTO Questao (ID_Prova, Enunciado, Codigo_Exemplo, Ordem_Questao)
    VALUES (@idProva, N'No modelo Entidade-Relacionamento (ER), o que representa um relacionamento N:M (muitos para muitos)?', NULL, 8);
    SET @idQuestao = SCOPE_IDENTITY();
    INSERT INTO Alternativas (ID_Questao, Letra_Alternativa, Texto_Alternativa, Eh_Correta) VALUES
        (@idQuestao, N'A', N'Uma entidade está sempre associada a apenas uma outra.', 0),
        (@idQuestao, N'B', N'Várias entidades de um lado podem se associar a várias do outro lado.', 1),
        (@idQuestao, N'C', N'Uma entidade não pode ter relacionamentos.', 0),
        (@idQuestao, N'D', N'É um erro de modelagem que deve ser evitado.', 0),
        (@idQuestao, N'E', N'Significa que a entidade é fraca.', 0);

END

-- =============================================================
-- CONFERENCIA
-- =============================================================
SELECT 'Usuario' AS Tabela, COUNT(*) AS Linhas FROM Usuario
UNION ALL SELECT 'ADM', COUNT(*) FROM ADM
UNION ALL SELECT 'Tutor', COUNT(*) FROM Tutor
UNION ALL SELECT 'Estudante', COUNT(*) FROM Estudante
UNION ALL SELECT 'Medalha', COUNT(*) FROM Medalha
UNION ALL SELECT 'Curso', COUNT(*) FROM Curso
UNION ALL SELECT 'Material', COUNT(*) FROM Material
UNION ALL SELECT 'Prova', COUNT(*) FROM Prova
UNION ALL SELECT 'Questao', COUNT(*) FROM Questao
UNION ALL SELECT 'Alternativas', COUNT(*) FROM Alternativas;

-- Acentuacao: se alguma linha aparecer aqui, o arquivo foi salvo ou colado
-- com codificacao errada e o texto esta corrompido.
SELECT TOP 20 'Questao' AS Origem, Enunciado AS Texto FROM Questao
WHERE Enunciado LIKE '%' + CHAR(195) + '%'
UNION ALL
SELECT 'Material', Titulo_Material FROM Material
WHERE Titulo_Material LIKE '%' + CHAR(195) + '%';
