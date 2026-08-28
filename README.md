# Pim 4

# Desafio
Desenvolver o projeto de uma plataforma web de avaliação e apoio à aprendizagem para uma empresa fictícia de tecnologia educacional, expandindo para as versões Desktop e Mobile e conectando com um banco de dados.

# Status do Projeto
Em desenvolvimento

### Backlog de Atividades

| ID | Atividade | Prioridade | Sprint | Situação |
| :---: | :--- | :---: | :---: | :---: |
| 1 | Mapear e criar o modelo físico no SQL Server | ALTA | 01 | A FAZER |
| 2 | Implementar Stored Procedures e Triggers no banco SQL | ALTA | 01 | A FAZER |
| 3 | Configurar e hospedar o banco SQL Server em nuvem | ALTA | 01 | A FAZER |
| 4 | Estruturar projeto Backend em C# (.NET API REST) | ALTA | 01 | A FAZER |
| 5 | Criar endpoints da API C# para autenticação e dados | ALTA | 02 | A FAZER |
| 6 | Refatorar Web (HTML/CSS/JS) trocando JSON por fetch/API | ALTA | 02 | A FAZER |
| 7 | Desenvolver Aplicação Desktop em C# (Perfil Admin) | ALTA | 03 | A FAZER |
| 8 | Desenvolver Aplicação Mobile em React Native (Alunos/Profs) | ALTA | 03 | A FAZER |
| 9 | Integrar aplicações Web, Desktop e Mobile à API C# | ALTA | 03 | A FAZER |
| 10 | Testar integração ponta a ponta e elaborar relatório PIM | ALTA | 03 | A FAZER |

### Requisitos Funcionais

| ID | Requisito | Descrição |
| :---: | :--- | :--- |
| RF01 | Gerir Utilizadores | Cadastrar, consultar, atualizar e desativar contas de usuários. |
| RF02 | Autenticar Acesso | Validar a entrada de utilizadores via e-mail e senha com suporte a recuperação de credenciais. |
| RF03 | Gerir Cursos | Criar, editar, arquivar e excluir cursos. |
| RF04 | Moderar Publicações | Revisar, aprovar ou rejeitar cursos submetidos por tutores antes da publicação oficial. |
| RF05 | Monitorizar Progresso | Registar a visualização de materiais e calcular a percentagem de conclusão do curso. |
| RF06 | Avaliar Conhecimento | Aplicar provas de múltipla escolha com correção automática e atribuição de nota. |
| RF07 | Emitir Certificados | Gerar documento digital de conclusão com código de autenticidade para alunos aprovados. |
| RF08 | Atribuir Medalhas | Conceder insígnias visuais ao perfil do estudante ao atingir marcos específicos (gamificação). |
| RF09 | Protocolar Chamados | Abrir tickets de suporte técnico ou pedagógico direcionados aos responsáveis. |
| RF10 | Interagir no Suporte | Responder, encaminhar e encerrar os chamados abertos pelos utilizadores. |
| RF11 | Auditar Ações | Rastrear e armazenar logs de atividades críticas realizadas por administradores. |
| RF12 | Extrair Relatórios | Gerar dashboards com o desempenho médio, taxa de evasão e estatísticas de conclusão. |

### Requisitos Não Funcionais

| ID | Requisito | Descrição |
| :---: | :--- | :--- |
| RNF01 | Segurança | Todas as senhas de utilizadores devem ser armazenadas no banco de dados utilizando criptografia (Hash). |
| RNF02 | Integridade | O sistema não deve permitir a exclusão de cursos que possuam estudantes matriculados (deve-se usar o status "Arquivado"). |
| RNF03 | Usabilidade | A interface deve ser responsiva, permitindo que o estudante acesse os cursos tanto por computador como por dispositivos móveis. |

### Cronograma de Sprints

| Sprint | Periodo | Objetivo |
| :---: | :---: | :---: |
| 01 | 31/08 - 14/09 | Modelagem do Banco SQL Server em Nuvem e Estruturação da API C# |
| 02 | 15/09 - 05/10 | Implementação dos Endpoints REST C# e Refatoração da Versão Web |
| 03 | 06/10 - 27/10 | Desenvolvimento das Aplicações Desktop (C#) e Mobile (React Native) |

### DoR - Definition of Ready

* **Revisão da Base do PIM III:** Requisitos (RF/RNF), protótipos e estrutura de telas do PIM III revisados e prontos para integração.
* **Definição de Tecnologias:** SGBD SQL Server, framework .NET C# para API/Desktop e React Native para Mobile acordados por toda a equipe.
* **Arquitetura Unificada:** Divisão de acessos mapeada: Web (Acesso Geral), Desktop (Acesso Admin) e Mobile (Alunos/Professores).
* **Modelagem de Dados Aprovada:** Tabela e esquema ER atualizados para conversão em DDL no SQL Server.
* **Estrutura de Trabalho:** Backlog do PIM IV priorizado e dividido por áreas de atuação dos integrantes.

### DoD - Definition of Done

* **Banco SQL Server Operacional:** Scripts DDL (`CREATE TABLE`), Stored Procedures e Triggers de auditoria rodando na nuvem.
* **API REST C# Concluída:** API desenvolvida em C# conectada ao SQL Server com autenticação JWT e validação de permissões.
* **Web Refatorada:** Código HTML/CSS/JS integrado à API C# via `fetch`/`axios` sem nenhuma dependência de JSON local.
* **Desktop C# Funcional:** Sistema Desktop C# operacional para Administradores realizando moderação, auditoria de logs e relatórios.
* **Mobile React Native Funcional:** App Mobile rodando em React Native integrado à API para Alunos e Professores.
* **Relatório PIM IV:** Documentação ABNT completa abordando as disciplinas do semestre e a arquitetura distribuída.

### 🛠️ Stack Tecnológica

| Camada / Frente | Tecnologias / Ferramentas | Descrição e Aplicação |
| :--- | :--- | :--- |
| **Banco de Dados** | Microsoft SQL Server | Relacional, Stored Procedures, Triggers e Índices[cite: 2] |
| **Backend & API** | C# / .NET Core REST API | Regras de negócio, Controllers, Entity Framework / Dapper e Autenticação JWT |
| **Aplicação Web** | HTML5, CSS3, JavaScript | Interface completa (Acesso Geral) integrada à API via `fetch`/`axios` |
| **Aplicação Desktop** | C# (.NET WPF / WinForms) | Painel exclusivo para Administradores (Moderação, Auditoria/Logs e Dashboards) |
| **Aplicação Mobile** | React Native (Expo) | App móvel focado em Alunos e Professores (Provas, Progresso, Medalhas e Suporte) |

# Equipe

| NOME | RA | PAPEL | GIT |
| :---: | :---: | :---: | :---: |
| Diego Santos Rodrigues | R862JH7 | Scrum Master | [![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/dsr-23) |
| Willian Augusto Quincas Senna | H78BIA3 | Dev | [![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/WillSennaa) |
| Cauã Brandão da Costa | R8327B0 | Product Owner | [![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Caua-Brandao) |
| Luiz Gustavo da Silva Borges | H4062J7 | Dev | [![GitHub](https://img.shields.io/badge/github-%23121011.svg?style=for-the-badge&logo=github&logoColor=white)](https://github.com/LuizBorges16) |
