# Back-end — Tech Quest API (PIM IV)

API REST em **ASP.NET Core 8 (C#)** que concentra todas as regras de negócio da plataforma. É consumida pelas três aplicações (Web, Desktop e Mobile) e é a única que acessa o banco de dados.

| Item | Valor |
| :--- | :--- |
| Framework | .NET 8 (LTS), ASP.NET Core com Controllers |
| Banco | Azure SQL Database via Entity Framework Core 8 |
| Autenticação | JWT (HMAC-SHA256, 8 horas) + senhas com BCrypt (fator 11) |
| Documentação | Swagger em `/swagger` |
| Testes | xUnit (89 casos) |
| Produção | Azure App Service `TQBackendAPI` — https://tqbackendapi-cpb2edg0gxcsfggz.brazilsouth-01.azurewebsites.net |

---

## Estrutura

A solução `TechQuest.slnx` segue uma arquitetura em camadas, com as dependências apontando para dentro (o Domain não conhece nenhum outro projeto).

```
BackEnd/
├── TechQuest.slnx
├── TechQuest.Domain/           Entidades e regras de negócio puras, sem dependências
│   ├── Entidades/              Usuario, Curso, Prova, Questao, Progresso, Certificado...
│   └── Regras/                 RegrasGamificacao, FluxoPublicacaoCurso, RegrasConclusaoCurso,
│                               RegrasMedalhas, RegrasQuestao
├── TechQuest.Application/      Casos de uso
│   ├── Servicos/               AutenticacaoService, CursoService, ProvaService, TutorService,
│   │                           AdminService, ChamadoService, ConclusaoCursoService...
│   ├── Dtos/                   Contratos de entrada e saída da API
│   └── Interfaces/             Repositórios, Unit of Work e serviços de segurança
├── TechQuest.Infrastructure/   Implementações técnicas
│   ├── Persistencia/           TechQuestDbContext (Fluent API), repositórios, UnidadeDeTrabalho
│   └── Seguranca/              JwtTokenService, BCryptHashService
├── TechQuest.API/              Porta de entrada HTTP
│   ├── Controllers/            Auth, Cursos, Materiais, Provas, Perfil, Certificados,
│   │                           Chamados, Tutor, Admin
│   ├── Middlewares/            TratamentoDeErrosMiddleware
│   ├── Extensoes/              Leitura do usuário a partir do token
│   ├── Program.cs              Injeção de dependência, JWT, CORS, Swagger, pipeline
│   └── TechQuest.API.http      Requisições prontas para testar no Visual Studio
└── TechQuest.Tests/            Testes xUnit do domínio e dos serviços
```

| Projeto | Depende de | Responsabilidade |
| :--- | :--- | :--- |
| Domain | — | Regras que não mudam com a tecnologia |
| Application | Domain | Orquestra os casos de uso e define as interfaces |
| Infrastructure | Application, Domain | Banco, JWT e hash de senha |
| API | Application, Infrastructure | HTTP, autenticação, CORS, Swagger, erros |
| Tests | Application, Domain | Testes sem banco, com dublês em memória |

---

## Como executar localmente

### Pré-requisitos

- Visual Studio 2022 (com suporte a `.slnx`) e a carga de trabalho **ASP.NET e desenvolvimento Web**, ou o .NET 8 SDK.
- Acesso ao banco `TechQuestDB` no Azure SQL. O IP da sua máquina precisa estar liberado no firewall do servidor (portal do Azure → servidor SQL → Rede).

### 1. Configurar os segredos

A connection string e a chave do JWT **não ficam no `appsettings.json`**. No Visual Studio, clique com o botão direito em `TechQuest.API` → **Gerenciar Segredos do Usuário** e cole:

```json
{
  "ConnectionStrings": {
    "TechQuestDB": "Server=tcp:<servidor>.database.windows.net,1433;Initial Catalog=TechQuestDB;User ID=<usuario>;Password=<senha>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"
  },
  "Jwt": {
    "Chave": "<texto aleatório com no mínimo 32 caracteres>"
  }
}
```

Pela linha de comando, o equivalente é `dotnet user-secrets set "Jwt:Chave" "<valor>" --project TechQuest.API`.

> Sem a chave JWT a API não inicia. O `Connection Timeout=60` cobre o tempo que o banco serverless leva para sair da pausa.

### 2. Rodar

- **Visual Studio:** defina `TechQuest.API` como projeto de inicialização e pressione F5. O Swagger abre em `https://localhost:7160/swagger`.
- **Linha de comando:** `dotnet run --project TechQuest.API`.

### 3. Testar rapidamente

1. `GET /api/health` → deve responder `{ "status": "ok", "banco": "conectado" }`.
2. `POST /api/auth/login` com uma conta de demonstração.
3. Copie o `token`, clique em **Authorize** no Swagger e cole (sem escrever "Bearer").

Também é possível usar o `TechQuest.API.http` no Visual Studio, que captura o token automaticamente entre as requisições. Ajuste a variável `@host` para a porta em que a API está rodando.

### Contas de demonstração

| Papel | E-mail | Senha |
| :--- | :--- | :--- |
| Estudante | `felipe123@gmail.com` | `aluno123@` |
| Tutor | `tutor123@gmail.com` | `tutor123@` |
| Administrador | `admin123@gmail.com` | `admin123@` |

---

## Rotas da API

Todas as rotas, exceto as marcadas como públicas, exigem o cabeçalho `Authorization: Bearer <token>`.

| Controller | Base | Acesso | Operações |
| :--- | :--- | :--- | :--- |
| Auth | `/api/auth` | Público | `POST login`, `POST registrar` |
| Cursos | `/api/cursos` | Autenticado | `GET` catálogo publicado, `GET {id}` detalhe, `POST {id}/matricula` (Estudante) |
| Materiais | `/api/materiais` | Estudante | `PUT {id}/progresso` |
| Provas | `/api/provas` | Autenticado | `GET {id}` (sem gabarito), `POST {id}/tentativas` (Estudante) |
| Perfil | `/api/perfil` | Autenticado | `GET`/`PUT` perfil, `GET historico`, `conquistas`, `certificados`, `desempenho`; `PUT senha`, `PUT email`, `POST desativar` |
| Certificados | `/api/certificados` | **Público** | `GET {codigo}` valida o certificado |
| Chamados | `/api/chamados` | Autenticado | `GET`, `POST`, `GET recebidos`, `POST {id}/responder`, `PATCH {id}/status`, `POST {id}/fechar` |
| Tutor | `/api/tutor` | Tutor | Cursos, aulas, prova, questões, submissão, alunos e concessão de medalha |
| Admin | `/api/admin` | Administrador | Resumo, aprovação e rejeição de cursos, usuários, chamados técnicos, medalhas e logs |
| Health | `/api/health` | **Público** | Verifica a conexão com o banco |

O contrato completo (corpos de requisição e resposta) está no Swagger.

---

## Testes

```bash
dotnet test TechQuest.Tests
```

Os testes cobrem as regras de gamificação, o fluxo de publicação, a conclusão de curso, as medalhas, as questões, o comportamento das entidades e o serviço de conclusão de curso. Eles usam repositórios falsos em memória (`Dubles/`) e **não acessam o banco nem a rede**.

> Se os testes não aparecerem no Test Explorer, verifique se algum `.csproj` tem `--` dentro de um comentário XML (é inválido e impede o projeto de carregar) e rode **Restaurar pacotes NuGet**.

---

## Publicação no Azure

A API é publicada pelo Visual Studio com o perfil `Properties/PublishProfiles/TQBackendAPI - Web Deploy.pubxml` (botão direito em `TechQuest.API` → **Publicar**).

No App Service (**Configurações → Variáveis de ambiente**) devem existir:

| Nome | Tipo | Valor |
| :--- | :--- | :--- |
| `TechQuestDB` | Connection string (SQLAzure) | Connection string do banco |
| `Jwt__Chave` | App setting | Mesma regra da chave local (mínimo 32 caracteres) |
| `Cors__Origens__0` (opcional) | App setting | URL de um front-end publicado |

No servidor SQL, a opção **"Permitir que serviços e recursos do Azure acessem este servidor"** deve estar ligada.

---

## Decisões principais

As justificativas completas estão em [`Docs/README.md`](../Docs/README.md#6-registro-de-decisões-de-arquitetura-adr).

- **Camadas com dependência para dentro** (ADR-003): regras testáveis sem banco.
- **EF Core com Repository e Unit of Work** (ADR-004): o mapeamento preserva os nomes do banco do PIM III.
- **Correção da prova no servidor** (ADR-008): o gabarito nunca sai da API durante a prova.
- **XP e nível calculados** (ADR-007): não existem colunas de XP no banco.
- **Middleware de erros** (ADR-018): toda falha vira JSON padronizado; stack trace só em desenvolvimento.
- **Swagger ligado em produção** (ADR-014): documentação viva para os clientes e para a banca.

---

## Problemas comuns

| Sintoma | Causa provável | Solução |
| :--- | :--- | :--- |
| API não inicia: "Jwt:Chave não configurada" | Segredos não definidos | Configurar os User Secrets (passo 1) |
| `/api/health` falha ou dá timeout | IP fora do firewall do Azure SQL | Adicionar o IP atual em Rede no portal |
| Primeira requisição demora dezenas de segundos | Banco serverless saindo da pausa | Aguardar; o `EnableRetryOnFailure` tenta de novo sozinho |
| 401 em rota protegida | Token ausente, expirado ou com "Bearer" duplicado | Fazer login de novo e colar só o token |
| 403 em rota protegida | Papel sem permissão (ex.: estudante em `/api/admin`) | Usar a conta do papel correto |
