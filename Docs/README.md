# Documentação do Projeto — Tech Quest (PIM IV)

Este documento detalha os **requisitos**, as **regras de negócio** e as **decisões de arquitetura (ADR)** da plataforma Tech Quest. Ele complementa o `README.md` da raiz (visão geral, backlog e equipe) e o `SQL/README.md` (dicionário de dados).

| Documento | Onde |
| :--- | :--- |
| Manual do PIM IV (UNIP) | `Docs/Manual_PIM_IV - ADS - Versão 2.docx` |
| Diagramas UML (casos de uso, classes BCE, sequência, MER/DER) | `Diagramas/DiagramasUML.asta` |
| Dicionário de dados | `SQL/README.md` |
| Contrato da API | Swagger em `/swagger` na URL da API publicada |

---

## Sumário

1. [Contexto da solução](#1-contexto-da-solução)
2. [Atores](#2-atores)
3. [Requisitos funcionais](#3-requisitos-funcionais)
4. [Requisitos não funcionais](#4-requisitos-não-funcionais)
5. [Regras de negócio](#5-regras-de-negócio)
6. [Registro de Decisões de Arquitetura (ADR)](#6-registro-de-decisões-de-arquitetura-adr)
7. [Matriz de rastreabilidade](#7-matriz-de-rastreabilidade)

---

## 1. Contexto da solução

A Tech Quest é uma plataforma gamificada de ensino de programação para uma empresa fictícia de tecnologia educacional. O tutor cria cursos, o administrador aprova, e o estudante estuda, faz provas e ganha XP, medalhas e certificados.

No PIM IV, o site estático do PIM III (HTML, CSS e JavaScript com dados em JSON) foi transformado em um sistema distribuído:

| Componente | Tecnologia | Público |
| :--- | :--- | :--- |
| Banco de dados | Azure SQL Database (SQL Server) | — |
| API REST | ASP.NET Core 8 (C#), no Azure App Service | todos os clientes |
| Aplicação Web | HTML5, CSS3, JavaScript + `api.js` | Estudante, Tutor e Administrador |
| Aplicação Desktop | C# .NET 8, Windows Forms, padrão MVP | Administrador |
| Aplicativo Mobile | React Native (Expo), APK Android | Estudante e Tutor |

Fluxo principal da plataforma: **o tutor cria o curso → o administrador revisa e aprova → o estudante se matricula, estuda, faz a prova e recebe o certificado.**

---

## 2. Atores

| Ator | Descrição | Clientes |
| :--- | :--- | :--- |
| **Usuário** | Ator abstrato com o que é comum a todos: login, edição de perfil, troca de senha e e-mail. Visitantes sem login podem validar certificados. | Web, Desktop, Mobile |
| **Estudante** | Cria a própria conta, matricula-se, estuda, faz provas, acompanha XP, medalhas e certificados, abre chamados. | Web, Mobile |
| **Tutor** | Cria e mantém cursos, materiais e provas; submete para aprovação; responde dúvidas; acompanha alunos; concede medalhas manuais. | Web, Mobile |
| **Administrador** | Aprova ou rejeita cursos; gerencia usuários e medalhas; atende chamados técnicos; consulta painel e auditoria. | Web, Desktop |

O papel de cada conta não é uma coluna do banco: ele é definido pela especialização (`ADM`, `Tutor` ou `Estudante`) e viaja dentro do token JWT (ver [ADR-006](#adr-006--papel-do-usuário-definido-por-especialização)).

---

## 3. Requisitos funcionais

Legenda de situação: ✅ atendido · 🟡 atendido parcialmente (com justificativa).

### RF01 — Gerir usuários ✅
**Descrição:** cadastrar, consultar, ativar e desativar contas de usuários.
**Atores:** Administrador (gestão); Usuário (dados do próprio perfil).
**Detalhamento:**
- O administrador cria contas de qualquer papel (Estudante, Tutor ou Administrador). No Desktop, a senha é gerada de forma aleatória e segura e copiada para a área de transferência.
- O administrador lista os usuários com papel e situação e pode ativar ou desativar contas.
- Cada usuário edita o próprio nome, telefone, data de nascimento e cidade.
- Contas não são excluídas, apenas desativadas (RN-USU-03).

**Critérios de aceitação:** e-mail duplicado é recusado; senha com menos de 6 caracteres é recusada; o administrador não consegue desativar a própria conta.
**Implementação:** `POST /api/admin/usuarios`, `GET /api/admin/usuarios`, `PATCH /api/admin/usuarios/{id}/status`, `PUT /api/perfil`.

### RF02 — Autenticar acesso 🟡
**Descrição:** validar a entrada do usuário com e-mail e senha.
**Atores:** Usuário.
**Detalhamento:**
- O login confere o e-mail, compara a senha com o hash BCrypt e verifica se a conta está ativa.
- Em caso de sucesso, a API emite um token JWT com o id, o nome e o papel, válido por 8 horas.
- Em caso de falha, a mensagem é genérica ("E-mail ou senha inválidos"), sem revelar qual dado está errado.
- O Desktop aceita apenas administradores; o Mobile aceita apenas estudantes e tutores.
- Troca de senha e de e-mail exigem a senha atual.

**Parcial:** a recuperação de senha por e-mail ("esqueci minha senha") não foi implementada, porque a plataforma não tem serviço de envio de e-mail. A recuperação é feita pelo administrador.
**Implementação:** `POST /api/auth/login`, `POST /api/auth/registrar`, `PUT /api/perfil/senha`, `PUT /api/perfil/email`.

### RF03 — Gerir cursos 🟡
**Descrição:** criar e editar cursos, seus materiais e sua prova.
**Atores:** Tutor.
**Detalhamento:**
- O tutor cria o curso (nome, descrição, categoria, nível, carga horária), que nasce em **Rascunho**.
- O tutor adiciona, edita e remove aulas (texto ou exercício), com pré-visualização no Mobile.
- O tutor cria a prova (título, nota mínima, tempo) e cadastra as questões com alternativas e gabarito.
- A edição só é permitida enquanto o curso está em Rascunho ou Rejeitado (RN-CUR-03).

**Parcial:** a exclusão de cursos foi substituída, de propósito, pela remoção de aulas e questões em cursos ainda não publicados, para cumprir o RNF02. O arquivamento de cursos publicados está mapeado como trabalho futuro.
**Implementação:** rotas `/api/tutor/cursos`, `/api/tutor/aulas`, `/api/tutor/provas`, `/api/tutor/questoes`.

### RF04 — Moderar publicações ✅
**Descrição:** revisar, aprovar ou rejeitar cursos submetidos pelos tutores antes da publicação.
**Atores:** Tutor (submete), Administrador (avalia).
**Detalhamento:**
- O tutor submete o curso (exige ao menos uma aula), que passa a **Pendente**.
- O administrador vê a fila de aprovações e abre o curso completo: dados, texto das aulas e prova com gabarito.
- Ao aprovar, o curso vai para **Publicado**. Ao rejeitar, vai para **Rejeitado** e o motivo é enviado ao tutor como mensagem.
- Toda avaliação registra o administrador que avaliou e uma linha em `Log_Auditoria`.

**Implementação:** `POST /api/tutor/cursos/{id}/submeter`, `GET /api/admin/cursos`, `GET /api/admin/cursos/{id}`, `POST /api/admin/cursos/{id}/aprovar`, `POST /api/admin/cursos/{id}/rejeitar`.

### RF05 — Monitorar progresso ✅
**Descrição:** registrar a leitura dos materiais e calcular o percentual de conclusão do curso.
**Atores:** Estudante.
**Detalhamento:**
- O estudante se matricula no curso publicado. A matrícula é a linha em `Historico`.
- Ao abrir uma aula, o sistema registra o percentual lido; ao concluir, marca a aula como concluída.
- O percentual do curso é a razão entre aulas concluídas e total de aulas.
- A cada conclusão, o sistema reavalia a conclusão do curso, o certificado e as medalhas, numa única transação.

**Implementação:** `POST /api/cursos/{id}/matricula`, `PUT /api/materiais/{id}/progresso`.

### RF06 — Avaliar conhecimento ✅
**Descrição:** aplicar provas de múltipla escolha com correção automática e atribuição de nota.
**Atores:** Estudante.
**Detalhamento:**
- A prova é enviada ao cliente **sem o gabarito**.
- O estudante responde e envia; a API corrige no servidor, calcula a nota e compara com a nota mínima da prova.
- Cada envio gera uma nova tentativa em `Desempenho`.
- O resultado mostra nota, aprovação e a correção questão a questão.

**Implementação:** `GET /api/provas/{id}`, `POST /api/provas/{id}/tentativas`.

### RF07 — Emitir certificados ✅
**Descrição:** gerar documento digital de conclusão com código de autenticidade.
**Atores:** Sistema (em favor do Estudante); qualquer pessoa (validação).
**Detalhamento:**
- Quando o curso é concluído (RN-CON-01), o sistema marca a matrícula como `Concluido` e grava o certificado.
- O código de autenticação é um GUID gerado pelo próprio banco (`NEWID()`).
- O estudante visualiza, imprime ou salva em PDF pelo navegador e compartilha o código.
- Qualquer pessoa valida o certificado pelo código, sem login.

**Implementação:** `ConclusaoCursoService`, `GET /api/perfil/certificados`, `GET /api/certificados/{codigo}` (pública).

### RF08 — Atribuir medalhas ✅
**Descrição:** conceder medalhas ao estudante ao atingir marcos (gamificação).
**Atores:** Sistema (automáticas), Tutor (manuais), Administrador (catálogo).
**Detalhamento:**
- Oito medalhas são concedidas automaticamente pelas regras RN-GAM-04.
- Quatro medalhas são concedidas manualmente pelo tutor.
- O administrador mantém o catálogo (nome, raridade, descrição).
- O estudante vê as conquistas, com XP e nível calculados (RN-GAM-01 a 03).

**Implementação:** `RegrasMedalhas`, `ConquistaService`, `POST /api/tutor/estudantes/{id}/medalhas/{idMedalha}`, `/api/admin/medalhas`, `GET /api/perfil/conquistas`.

### RF09 — Protocolar chamados ✅
**Descrição:** abrir tickets de suporte técnico ou pedagógico direcionados aos responsáveis.
**Atores:** Estudante (e demais usuários).
**Detalhamento:**
- **Dúvida educacional:** exige o curso e é encaminhada automaticamente ao tutor que criou o curso.
- **Problema técnico:** nasce sem destinatário e entra na fila de todos os administradores.
- O cliente não escolhe o destinatário: o roteamento é feito pela API (RN-CHA-01).

**Implementação:** `POST /api/chamados`, `GET /api/chamados`.

### RF10 — Interagir no suporte 🟡
**Descrição:** responder e encerrar os chamados.
**Atores:** Tutor (dúvidas), Administrador (técnicos).
**Detalhamento:**
- O responsável pode marcar o chamado como **Em andamento** ("estou analisando").
- Ao responder, o chamado passa a **Resolvido** e a resposta fica visível ao remetente.
- O chamado também pode ser encerrado sem resposta.

**Parcial:** não há encaminhamento manual de um chamado para outro responsável nem anexo de arquivos (exigiria armazenamento de arquivos no Azure).
**Implementação:** `GET /api/chamados/recebidos`, `POST /api/chamados/{id}/responder`, `PATCH /api/chamados/{id}/status`, `POST /api/chamados/{id}/fechar`, `GET /api/admin/chamados`.

### RF11 — Auditar ações ✅
**Descrição:** rastrear e armazenar logs de ações críticas.
**Atores:** Sistema; Administrador (consulta).
**Detalhamento:**
- A trigger `TRG_Auditoria_StatusUsuario` registra toda mudança de status de usuário, mesmo se feita fora da API.
- A API registra cada aprovação ou rejeição de curso com o id real do administrador.
- O administrador consulta os logs mais recentes, com limite configurável.

**Implementação:** trigger no banco, `AdminService`, `GET /api/admin/logs`.

### RF12 — Extrair relatórios 🟡
**Descrição:** gerar painéis com estatísticas da plataforma e de desempenho.
**Atores:** Administrador (painel); Estudante (relatório próprio).
**Detalhamento:**
- Painel administrativo com nove indicadores: total de usuários, usuários ativos, estudantes, tutores, cursos publicados, cursos pendentes, matrículas, certificados emitidos e chamados abertos.
- Relatório de desempenho do estudante gerado pela procedure `SP_RelatorioDesempenhoEstudante` ("Minhas provas").

**Parcial:** a taxa de evasão e gráficos ao longo do tempo não foram implementados, porque o modelo não tem registro de acessos diários.
**Implementação:** `GET /api/admin/resumo`, `GET /api/perfil/desempenho`.

---

## 4. Requisitos não funcionais

| ID | Categoria | Requisito | Como é atendido |
| :--- | :--- | :--- | :--- |
| **RNF01** | Segurança | Senhas armazenadas com hash. | BCrypt com fator de trabalho 11 e sal embutido; a senha em texto nunca é gravada nem registrada em log. |
| **RNF02** | Integridade | Não excluir cursos com estudantes matriculados. | Não existe exclusão de curso nem de usuário. "Publicado" é estado final; contas são desativadas. |
| **RNF03** | Usabilidade | Interface responsiva em computador e celular. | Web mobile-first com menu lateral em telas largas; Mobile adapta de 1 a 3 colunas e troca as abas por menu lateral a partir de 768 de largura. |
| RNF04 | Segurança | Autenticação e autorização em toda rota protegida. | JWT HMAC-SHA256 validando emissor, audiência, validade e assinatura; `[Authorize(Roles = ...)]` por controller; o id do usuário vem do token, nunca da URL ou do corpo. |
| RNF05 | Segurança | Proteger dados sensíveis da avaliação e da infraestrutura. | Gabarito fora dos DTOs do estudante; consultas parametrizadas (EF Core); segredos fora do repositório; stack trace só em desenvolvimento. |
| RNF06 | Segurança | Comunicação cifrada. | HTTPS em todos os clientes; conexão com o banco com `Encrypt=True`. |
| RNF07 | Disponibilidade | A solução deve estar acessível pela internet. | API no Azure App Service e banco no Azure SQL Database, região Brazil South. |
| RNF08 | Resiliência | Tolerar a pausa automática do banco serverless. | `EnableRetryOnFailure` (até 5 tentativas) na API; timeouts de 45 s (Mobile) e 60 s (Desktop). |
| RNF09 | Interoperabilidade | Um único contrato para todos os clientes. | API REST com JSON, documentada no Swagger; Web, Desktop e Mobile consomem as mesmas rotas. |
| RNF10 | Manutenibilidade | Código organizado em camadas e testável. | Domain, Application, Infrastructure e API separados; 89 casos de teste xUnit no back-end e 146 no Desktop, sem dependência de banco. |
| RNF11 | Rastreabilidade | Ações críticas auditadas. | Trigger de status de usuário e log de avaliação de cursos (RF11). |
| RNF12 | Tratamento de erros | Respostas de erro padronizadas. | Middleware converte exceções em JSON com status HTTP adequado e identificador para o suporte. |

---

## 5. Regras de negócio

As regras estão implementadas na API. A coluna "Onde" indica a classe responsável.

### 5.1 Usuários e acesso

| ID | Regra | Onde |
| :--- | :--- | :--- |
| RN-USU-01 | O e-mail é único e gravado em minúsculas. | `AutenticacaoService`, `AdminService` |
| RN-USU-02 | A senha deve ter no mínimo 6 caracteres. | `AutenticacaoService`, `AdminService`, `ContaService` |
| RN-USU-03 | Contas não são excluídas; são desativadas (`Status_Usuario = 0`). Conta inativa não faz login. | `AdminService`, `ContaService`, `AutenticacaoService` |
| RN-USU-04 | O cadastro público cria sempre um Estudante. Tutores e administradores são criados por um administrador. | `AutenticacaoService`, `AdminService` |
| RN-USU-05 | O papel é definido pela especialização (`ADM`, `Tutor`, `Estudante`), criada na mesma transação do usuário. | `AdminService` |
| RN-USU-06 | Trocar senha, trocar e-mail e encerrar a conta exigem a senha atual. A nova senha deve ser diferente da atual; o novo e-mail não pode estar em uso. | `ContaService` |
| RN-USU-07 | Um administrador não pode desativar a própria conta nem encerrá-la pelo perfil. | `AdminService`, `ContaService` |
| RN-USU-08 | O token JWT vale 8 horas e carrega id, nome, e-mail e papel. | `JwtTokenService` |

### 5.2 Cursos e publicação

| ID | Regra | Onde |
| :--- | :--- | :--- |
| RN-CUR-01 | Todo curso nasce em **Rascunho**. | `TutorService` |
| RN-CUR-02 | Transições válidas: Rascunho → Pendente → Publicado; Pendente → Rejeitado → Pendente. Publicado é estado final. | `FluxoPublicacaoCurso` |
| RN-CUR-03 | Conteúdo (dados, aulas, prova, questões) só pode ser editado em Rascunho ou Rejeitado. | `FluxoPublicacaoCurso.PermiteEdicao` |
| RN-CUR-04 | O tutor só altera os próprios cursos. | `TutorService` |
| RN-CUR-05 | Só é possível submeter um curso com ao menos uma aula. | `TutorService.SubmeterAsync` |
| RN-CUR-06 | A rejeição envia o motivo ao tutor como mensagem (Chamado do tipo `Avaliacao`). No Desktop, o motivo exige no mínimo 10 caracteres. | `AdminService` |
| RN-CUR-07 | Só cursos **Publicados** aparecem no catálogo e aceitam matrícula. | `CursoService`, `ProgressoService` |

### 5.3 Provas

| ID | Regra | Onde |
| :--- | :--- | :--- |
| RN-PRO-01 | Cada curso tem no máximo uma prova. | `TutorService.CriarProvaAsync` |
| RN-PRO-02 | A nota mínima fica entre 0 e 10 (padrão 7,00); o tempo padrão é 30 minutos. | `TutorService`, banco |
| RN-PRO-03 | Toda questão tem enunciado, ao menos duas alternativas, letras sem repetição e **exatamente uma** correta. | `RegrasQuestao` |
| RN-PRO-04 | O gabarito nunca é enviado ao estudante durante a prova. | DTOs de `ProvaService` |
| RN-PRO-05 | Nota = acertos × 10 ÷ total de questões, com duas casas decimais. Aprovado se nota ≥ nota mínima. | `ProvaService`, `Prova.Aprovado` |
| RN-PRO-06 | Cada envio é uma nova tentativa, numerada, sem limite de tentativas. | `ProvaService` |

### 5.4 Progresso, conclusão e certificado

| ID | Regra | Onde |
| :--- | :--- | :--- |
| RN-PRG-01 | O progresso nunca retrocede: reabrir uma aula concluída não a desmarca nem reduz o percentual. | `ProgressoService` |
| RN-PRG-02 | Concluir uma aula sem matrícula matricula o estudante automaticamente. | Mobile + `ProgressoService` |
| RN-CON-01 | O curso está concluído quando **todas** as aulas foram concluídas e, se houver prova, o estudante foi aprovado. | `RegrasConclusaoCurso` |
| RN-CON-02 | Curso sem aulas nunca é concluído (evita certificar o que não houve). | `RegrasConclusaoCurso` |
| RN-CON-03 | A conclusão é verificada nos dois gatilhos: conclusão da última aula e aprovação na prova. | `ConclusaoCursoService` |
| RN-CER-01 | O certificado é emitido uma única vez por matrícula, com código GUID gerado pelo banco. | `ConclusaoCursoService`, banco |
| RN-CER-02 | A validação do certificado é pública, pelo código. | `CertificadosController` |

### 5.5 Gamificação

| ID | Regra | Onde |
| :--- | :--- | :--- |
| RN-GAM-01 | XP = 50 por aula concluída + 100 por prova aprovada + 50 de bônus por nota máxima + 75 por medalha. | `RegrasGamificacao` |
| RN-GAM-02 | Nível = 1 + XP ÷ 350 (progressão linear). | `RegrasGamificacao` |
| RN-GAM-03 | XP e nível não são gravados: são calculados a partir de `Progresso`, `Desempenho` e `Conquista`. | `GamificacaoService` |
| RN-GAM-04 | Medalhas automáticas: Primeiro Passo (1 aula), Estudante Dedicado (10 aulas), Bibliotecário (20 aulas), Quiz Champion (1 prova aprovada), Pontuação Perfeita (1 nota máxima), Mestre em C# (concluir curso de C#), DBA Iniciante (concluir curso de Banco de Dados), Lenda da Tech Quest (concluir todos os cursos publicados). | `RegrasMedalhas` |
| RN-GAM-05 | Medalhas manuais, concedidas pelo tutor: Mestre em Arrays, Maratonista, POO Master, Arquiteto SOLID. | `TutorService` |
| RN-GAM-06 | Um estudante não recebe a mesma medalha duas vezes. | `ConquistaService`, `TutorService` |
| RN-GAM-07 | As medalhas automáticas são localizadas pelo **nome**; por isso o nome delas não pode ser alterado. | `RegrasMedalhas`, Desktop |

### 5.6 Suporte e auditoria

| ID | Regra | Onde |
| :--- | :--- | :--- |
| RN-CHA-01 | Dúvida (`Duvida`) exige o curso e vai ao tutor que o criou; se o curso não tiver tutor, vai para a fila técnica. | `ChamadoService` |
| RN-CHA-02 | Chamado técnico (`Tecnico`) nasce sem destinatário, visível a todos os administradores. | `ChamadoService` |
| RN-CHA-03 | Situações válidas: Aberto, Em andamento, Resolvido. Responder resolve o chamado. | `ChamadoService` |
| RN-AUD-01 | Toda mudança de status de usuário gera log pela trigger do banco. | `TRG_Auditoria_StatusUsuario` |
| RN-AUD-02 | Toda aprovação ou rejeição de curso gera log com o id do administrador. | `AdminService` |

---

## 6. Registro de Decisões de Arquitetura (ADR)

Cada ADR segue o formato: **Contexto** (o problema), **Decisão** (o que foi feito), **Alternativas rejeitadas** e **Consequências**. Todas estão com status **Aceita**, salvo indicação.

### ADR-001 — Uma API REST única para os três clientes
- **Contexto:** o projeto tem três clientes (Web, Desktop e Mobile) que precisam das mesmas regras: nota, XP, certificado e fluxo de publicação.
- **Decisão:** toda regra de negócio fica em uma API REST central. Os clientes apenas exibem dados e enviam ações. Só a API conhece a connection string do banco.
- **Alternativas rejeitadas:** cada cliente acessando o banco diretamente (senha do banco espalhada, inclusive dentro do APK, e regras duplicadas e divergentes).
- **Consequências:** regra em um lugar só; um cliente novo não duplica lógica. Em contrapartida, todos os clientes dependem da disponibilidade da API.

### ADR-002 — ASP.NET Core 8 com Controllers
- **Contexto:** o manual vincula o projeto às disciplinas de .NET do semestre.
- **Decisão:** API em ASP.NET Core 8 (versão LTS) com Controllers.
- **Alternativas rejeitadas:** Node.js ou Java (fora do conteúdo do semestre); Minimal APIs (rotas soltas no `Program.cs`, menos visível a organização em camadas pedida na Etapa 6).
- **Consequências:** mesma linguagem do Desktop (C#), suporte de longo prazo e Swagger integrado.

### ADR-003 — Arquitetura em camadas (Domain, Application, Infrastructure, API)
- **Contexto:** as regras precisam ser testáveis sem banco e independentes de framework.
- **Decisão:** quatro projetos com dependências apontando para dentro. O Domain não referencia nenhum outro projeto. A Application define interfaces de repositório que a Infrastructure implementa. O `Program.cs` é o único ponto que conhece todas as camadas (injeção de dependência).
- **Alternativas rejeitadas:** projeto único com controllers acessando o `DbContext` diretamente.
- **Consequências:** regras de domínio testadas em milissegundos com dublês em memória. Custo: mais projetos e interfaces para manter.

### ADR-004 — Entity Framework Core com Repository e Unit of Work
- **Contexto:** o banco do PIM III já existia, com nomes de colunas próprios.
- **Decisão:** EF Core 8 com mapeamento Fluent API para os nomes existentes, repositórios por agregado e `UnidadeDeTrabalho` para transações. A procedure é chamada por `FromSqlInterpolated`.
- **Alternativas rejeitadas:** ADO.NET com SQL escrito à mão em todas as operações (mais código e maior risco de SQL Injection); Dapper (o README previa, mas o mapeamento e o controle de transação do EF cobriram a necessidade).
- **Consequências:** consultas parametrizadas por padrão. Tabelas com trigger exigem `HasTrigger()` no mapeamento.

### ADR-005 — Preservar o esquema do PIM III com alterações apenas aditivas
- **Contexto:** o script e os diagramas do PIM III foram entregues e avaliados; o PIM IV é continuação.
- **Decisão:** o esquema original foi congelado. Toda mudança entrou como `ALTER TABLE ... ADD`. Lacunas do modelo foram resolvidas no código (ADR-006, ADR-007, ADR-010).
- **Alternativas rejeitadas:** redesenhar o banco do zero.
- **Consequências:** continuidade com o PIM III e com os diagramas. Algumas regras que poderiam ser constraints ficaram na aplicação (ex.: valores de status).

### ADR-006 — Papel do usuário definido por especialização
- **Contexto:** o modelo do PIM III tem as tabelas `ADM`, `Tutor` e `Estudante` ligadas a `Usuario`.
- **Decisão:** o papel é determinado pela existência de registro em uma dessas tabelas, resolvido no login e gravado no token JWT. O cliente nunca informa o papel.
- **Alternativas rejeitadas:** coluna `Papel` em `Usuario` (alteraria o modelo e permitiria inconsistência com as especializações).
- **Consequências:** cada papel pode ter atributos próprios. Criar um usuário exige duas inserções na mesma transação.

### ADR-007 — XP e nível calculados, não armazenados
- **Contexto:** a interface do PIM III já exibia XP e nível, mas o banco não tinha essas colunas.
- **Decisão:** XP e nível são grandezas derivadas, calculadas por `RegrasGamificacao` a partir de `Progresso`, `Desempenho` e `Conquista`.
- **Alternativas rejeitadas:** colunas `XP` e `Nivel` em `Estudante` (estado duplicado que pode sair de sincronia e alteraria o modelo).
- **Consequências:** o valor sempre reflete o que o aluno fez. Custo: algumas consultas a mais ao montar o perfil.

### ADR-008 — Correção da prova no servidor
- **Contexto:** no PIM III, o `questoes.json` com as respostas era baixado pelo navegador; o gabarito ficava visível no DevTools.
- **Decisão:** a prova é enviada sem o gabarito (separação feita nos DTOs). A correção e a nota são calculadas na API, e o gabarito só aparece no resultado, após o envio.
- **Alternativas rejeitadas:** corrigir no cliente (mais simples, mas sem nenhuma garantia de integridade da nota).
- **Consequências:** nota confiável, igual em Web e Mobile. Exige rede para enviar a prova.

### ADR-009 — Autenticação com JWT e senhas com BCrypt
- **Contexto:** três tipos de cliente (navegador, desktop, celular) precisam se autenticar na mesma API.
- **Decisão:** token JWT sem estado (HMAC-SHA256, 8 horas) e senhas com BCrypt fator 11. A chave de assinatura fica fora do código.
- **Alternativas rejeitadas:** sessão com cookie (inadequada para app nativo e desktop); SHA-256 ou MD5 para senha (rápidos demais, sem custo contra força bruta).
- **Consequências:** a API não guarda sessão. Um token não pode ser revogado antes de expirar; o encerramento de sessão é feito no cliente.

### ADR-010 — Máquina de estados de publicação e política de não exclusão
- **Contexto:** `Status_Curso` é um `VARCHAR` livre, e históricos, notas e certificados dependem de cursos e usuários.
- **Decisão:** as transições de status ficam em `FluxoPublicacaoCurso` (Domain). Cursos e usuários nunca são excluídos: Publicado é final e contas são desativadas.
- **Alternativas rejeitadas:** exclusão em cascata; deixar o status ser alterado livremente.
- **Consequências:** atende o RNF02 e preserva o histórico. Exclusão e arquivamento de curso ficaram fora (ver RF03).

### ADR-011 — Azure PaaS: App Service e Azure SQL Database serverless
- **Contexto:** a Etapa 8 pede infraestrutura em nuvem; o grupo não tem servidor próprio.
- **Decisão:** API no Azure App Service e banco no Azure SQL Database (tier serverless com oferta gratuita), ambos na região Brazil South. O banco existe apenas na nuvem, sem instância local.
- **Alternativas rejeitadas:** máquina virtual com SQL Server instalado (IaaS, exige administrar o servidor); SQL Server local em cada máquina (dados divergentes entre os integrantes).
- **Consequências:** uma fonte única de dados e custo zero. A pausa automática do banco deixa o primeiro acesso lento (mitigado pelo ADR-012 e pelos timeouts dos clientes).

### ADR-012 — Resiliência à pausa do banco
- **Contexto:** o tier serverless pausa sem uso, e a primeira consulta após a pausa falha por timeout.
- **Decisão:** `EnableRetryOnFailure` (5 tentativas, até 30 s entre elas), `Connection Timeout=60` e timeouts longos nos clientes.
- **Alternativas rejeitadas:** tier provisionado sempre ligado (custo).
- **Consequências:** o sistema se recupera sozinho. Na apresentação, a aplicação deve ser aberta alguns minutos antes.

### ADR-013 — Segredos fora do repositório
- **Contexto:** credencial versionada no Git é uma falha de segurança comum.
- **Decisão:** a connection string e a chave JWT ficam nos User Secrets do Visual Studio (desenvolvimento) e nas variáveis de ambiente do App Service (produção). O `appsettings.json` versionado contém apenas marcadores. A API não sobe sem a chave JWT.
- **Alternativas rejeitadas:** `appsettings.json` com as credenciais reais.
- **Consequências:** o repositório pode ser público sem expor o banco. Cada integrante configura os próprios segredos.

### ADR-014 — CORS com origens locais e Swagger em produção
- **Contexto:** o site roda em `localhost` e chama a API publicada; os integrantes desenvolvem contra a API em produção.
- **Decisão:** as origens locais ficam liberadas também em produção, e novas origens entram por configuração (`Cors__Origens__N`). O Swagger fica ligado em produção como documentação viva; o stack trace de erro continua restrito ao ambiente de desenvolvimento.
- **Alternativas rejeitadas:** ligar o ambiente Development no Azure para ter Swagger (exporia detalhes de erro).
- **Consequências:** desenvolvimento sem republicar a cada mudança. Liberar uma origem não dá acesso a dados, porque toda rota exige JWT.

### ADR-015 — Web: manter o front-end do PIM III com uma camada `api.js`
- **Contexto:** as 47 páginas do PIM III carregavam os dados por um único ponto (`TQ.init()`).
- **Decisão:** manter HTML, CSS e JavaScript puro e criar `js/api.js` como única camada de comunicação (URL, token, erros). O token fica no `sessionStorage`.
- **Alternativas rejeitadas:** reescrever em um framework (React, Angular), o que descartaria o PIM III; `fetch` espalhado nas páginas; token no `localStorage` (persiste em computador compartilhado).
- **Consequências:** a troca da API local para o Azure alterou uma linha e nenhuma página. O token some ao fechar a aba.

### ADR-016 — Desktop em Windows Forms com padrão MVP
- **Contexto:** a versão Desktop é exclusiva do administrador e deve ser feita em C#.
- **Decisão:** Windows Forms com padrão Model-View-Presenter. Os presenters ficam em um projeto sem dependência de WinForms. O Desktop tem modelos próprios do JSON da API, injeção de dependência manual e um único `HttpClient`.
- **Alternativas rejeitadas:** lógica no code-behind dos Forms (testável só clicando); WPF (curva maior para o grupo); referenciar `TechQuest.Application` (acoplaria a compilação dos dois sistemas).
- **Consequências:** 146 casos de teste sem abrir janela. Custo: mais classes (view, presenter, interface) por tela.

### ADR-017 — Mobile em React Native com Expo, distribuído como APK
- **Contexto:** o app atende estudante e tutor no Android e deve consumir a mesma API.
- **Decisão:** React Native com Expo e TypeScript, navegação por Expo Router, token no `expo-secure-store` (Android Keystore) e APK gerado pelo EAS Build. O endereço da API do Azure é fixo no app.
- **Alternativas rejeitadas:** Kotlin nativo (outra linguagem, só Android); Flutter (Dart, sem reaproveitar o JavaScript do grupo); `AsyncStorage` para o token (texto puro).
- **Consequências:** visual e regras iguais ao Web; build sem Android Studio. Sem modo offline: toda tela consulta a API.

### ADR-018 — Tratamento de erros centralizado
- **Contexto:** sem tratamento, um erro devolve HTML com stack trace (desenvolvimento) ou um 500 vazio (produção).
- **Decisão:** middleware que converte exceções em JSON com status adequado (409 para conflito de dados, 400 para requisição inválida, 503 para timeout) e um identificador para o suporte.
- **Alternativas rejeitadas:** `try/catch` em cada controller.
- **Consequências:** os três clientes exibem mensagens consistentes; detalhes técnicos não vazam em produção.

### ADR-019 — Medalhas automáticas localizadas pelo nome
- **Contexto:** o catálogo de medalhas é uma tabela, mas os critérios automáticos são código.
- **Decisão:** `RegrasMedalhas` devolve os nomes das medalhas merecidas, e a concessão busca as medalhas por esses nomes.
- **Alternativas rejeitadas:** coluna de critério na tabela (alteraria o modelo); ids fixos no código (quebram entre bancos diferentes).
- **Consequências:** funciona em qualquer banco criado pelos scripts. Renomear uma medalha automática desliga a regra, por isso o Desktop bloqueia a renomeação dessas oito.

### ADR-020 — Testes unitários com dublês em memória
- **Contexto:** testes que dependem do banco na nuvem seriam lentos e instáveis.
- **Decisão:** xUnit sobre Domain e Application (back-end) e sobre presenters e serviços (Desktop), com repositórios e gateways falsos escritos à mão.
- **Alternativas rejeitadas:** testes de integração contra o Azure SQL; bibliotecas de mock.
- **Consequências:** suíte rápida e independente da rede. Integração ponta a ponta é verificada manualmente (PB10).

---

## 7. Matriz de rastreabilidade

| Requisito | Casos de uso (Astah) | Rotas principais | Clientes |
| :--- | :--- | :--- | :--- |
| RF01 | Cadastrar Usuário, Ativar ou Desativar Usuário, Editar Dados, Encerrar Conta | `/api/admin/usuarios`, `/api/perfil` | Web, Desktop, Mobile |
| RF02 | Efetuar Login, Validar Usuário, Exibir Erro de Autenticação, Realizar Cadastro, Mudar Senha, Mudar E-mail | `/api/auth/*`, `/api/perfil/senha`, `/api/perfil/email` | Web, Desktop, Mobile |
| RF03 | Criar Curso, Vincular Material de Estudo, Gerar Prova, Editar Material de Estudo, Editar Prova, Remover Aula ou Questão | `/api/tutor/*` | Web, Mobile |
| RF04 | Submeter para Aprovação, Validar Curso, Reprovar | `/api/tutor/cursos/{id}/submeter`, `/api/admin/cursos/*` | Web, Desktop, Mobile |
| RF05 | Matricular-se em Curso, Acessar Material | `/api/cursos/{id}/matricula`, `/api/materiais/{id}/progresso` | Web, Mobile |
| RF06 | Realizar Prova, Verificar Nota | `/api/provas/*` | Web, Mobile |
| RF07 | Emitir Certificado, Validar Certificado | `/api/perfil/certificados`, `/api/certificados/{codigo}` | Web, Mobile |
| RF08 | Gerar Medalha, Ver Medalha, Conceder Medalha, Gerenciar Medalhas | `/api/perfil/conquistas`, `/api/tutor/estudantes/*/medalhas/*`, `/api/admin/medalhas` | Web, Desktop, Mobile |
| RF09 | Solicitar Suporte | `/api/chamados` | Web, Mobile |
| RF10 | Atender Chamado Educacional, Atender Chamado Técnico | `/api/chamados/*`, `/api/admin/chamados` | Web, Desktop, Mobile |
| RF11 | Consultar Logs de Auditoria | `/api/admin/logs` | Web, Desktop |
| RF12 | Visualizar Painel Administrativo, Ver Histórico | `/api/admin/resumo`, `/api/perfil/desempenho`, `/api/perfil/historico` | Web, Desktop, Mobile |
