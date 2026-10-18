# Web — Tech Quest (PIM IV)

Aplicação Web da plataforma, com acesso para os **três perfis** (Estudante, Tutor e Administrador). É o site do PIM III (HTML5, CSS3 e JavaScript puro, sem framework) com os dados simulados em JSON substituídos por chamadas à **API REST publicada no Azure**.

| Item | Valor |
| :--- | :--- |
| Tecnologias | HTML5, CSS3 (BEM + variáveis CSS), JavaScript puro |
| Páginas | 49 arquivos HTML (`index.html` + `pages/`) |
| Dados | API REST (`BackEnd`), via `fetch` na camada `js/api.js` |
| Autenticação | Token JWT guardado no `sessionStorage` |
| Responsividade | Mobile-first, com menu lateral em telas largas |

---

## Estrutura

```
Web/
├── index.html                Página inicial (landing page)
├── css/
│   └── style.css             Todo o visual da aplicação
├── js/
│   ├── api.js                Cliente da API: URL, token, sessão expirada e erros (PIM IV)
│   └── app.js                Lógica das páginas: login, menus, cursos, prova, painéis
├── pages/
│   ├── login.html, cadastro.html
│   ├── conta/                Perfil comum aos papéis
│   ├── estudante/  (28)      Home, catálogo, curso, aula, prova, resultado, conquistas,
│   │                         certificados, histórico, minhas provas, suporte, ajuda...
│   ├── tutor/      (7)       Painel, alunos, detalhe do aluno, dúvidas, cursos,
│   │                         criar e editar curso
│   └── admin/      (10)      Painel, aprovações, cursos, detalhe do curso, usuários,
│                             criar usuário, chamados, relatórios, configurações
├── data/                     mock.json: apenas apresentação (ícones, cores e FAQ)
├── docs/                     Guia de estudo de HTML/CSS/JS
├── injetar-api-js.ps1        Insere o <script src="api.js"> em todas as páginas
├── reparar-codificacao.ps1   Corrige páginas com acentuação corrompida (UTF-8 duplo)
└── _backup_html_20260924_110850/   Cópia das páginas antes da injeção do api.js
```

---

## Como executar

A Web roda localmente e consome a API que já está no Azure. Não é preciso rodar o back-end.

**Opção 1 — Python**

```bash
cd Web
python -m http.server 8080
```

Acesse `http://localhost:8080`.

**Opção 2 — VS Code**

Abra a pasta `Web` e use a extensão **Live Server** (porta 5500 ou 5501).

> As portas 8080, 5500 e 5501 de `localhost` e `127.0.0.1` estão liberadas no CORS da API. Outra porta ou abrir o arquivo direto (`file://`) faz as requisições serem bloqueadas pelo navegador.

### Contas de demonstração

| Papel | E-mail | Senha |
| :--- | :--- | :--- |
| Estudante | `felipe123@gmail.com` | `aluno123@` |
| Tutor | `tutor123@gmail.com` | `tutor123@` |
| Administrador | `admin123@gmail.com` | `admin123@` |

---

## Integração com a API (`js/api.js`)

Nenhuma página faz `fetch` diretamente. Toda chamada passa por `API.request`, que:

1. monta a URL a partir do endereço da API;
2. envia o token no cabeçalho `Authorization: Bearer`;
3. trata a sessão expirada (resposta 401 leva de volta ao login);
4. padroniza as mensagens de erro para as telas.

### Trocar entre a API na nuvem e a API local

O padrão é a API do Azure (`AMBIENTE_ATIVO = 'nuvem'`).

- **Permanente:** em `js/api.js`, troque `AMBIENTE_ATIVO` para `'local'`.
- **Temporário, sem editar arquivo:** no console do navegador,

```js
localStorage.setItem('tq_api_base', 'https://localhost:7160')   // usa a API local
localStorage.removeItem('tq_api_base')                          // volta para o Azure
```

### Sessão

O token fica no `sessionStorage`: some quando a aba é fechada. Isso evita que a próxima pessoa use a conta de quem saiu sem clicar em "Sair", o caso típico do laboratório da faculdade.

O papel do usuário vem do token emitido pela API. Esconder menus por perfil no JavaScript é só organização da interface: quem bloqueia o acesso é a API, que recusa com 403 uma rota de outro papel.

---

## Funcionalidades por perfil

| Perfil | O que faz na Web |
| :--- | :--- |
| **Estudante** | Cadastro, catálogo com busca, matrícula, aulas com progresso, prova corrigida no servidor, resultado questão a questão, XP e nível, conquistas, certificados (imprimir/PDF e código de validação), histórico, minhas provas, suporte e notificações |
| **Tutor** | Painel, criação e edição de cursos, aulas, prova e questões, submissão para aprovação, dúvidas dos alunos, acompanhamento dos alunos |
| **Administrador** | Painel de indicadores, fila de aprovações com revisão do curso completo, cursos, usuários (criar, ativar e desativar), chamados técnicos, relatórios e configurações |
| **Todos** | Login, edição de perfil, troca de senha e de e-mail |

---

## Scripts auxiliares (PowerShell)

| Script | Uso |
| :--- | :--- |
| `injetar-api-js.ps1` | Adiciona o `api.js` antes do `app.js` em todas as páginas, respeitando a profundidade das pastas. Pode rodar mais de uma vez: páginas que já têm o script são ignoradas. |
| `reparar-codificacao.ps1` | Corrige páginas gravadas com codificação dupla (acentos aparecendo como "Ã©"). Só altera arquivos realmente corrompidos. |

Execute na pasta `Web`: `.\injetar-api-js.ps1`.

---

## Decisões principais

As justificativas completas estão em [`Docs/README.md`](../Docs/README.md#6-registro-de-decisões-de-arquitetura-adr).

- **Manter o front-end do PIM III** em vez de reescrever em framework (ADR-015).
- **Camada única `api.js`:** a troca da API local para o Azure mudou uma linha e nenhuma página.
- **Token no `sessionStorage`**, e não no `localStorage` (ADR-015).
- **CORS com origens locais liberadas** também em produção (ADR-014).

---

## Problemas comuns

| Sintoma | Causa provável | Solução |
| :--- | :--- | :--- |
| Erro de CORS no console | Página aberta como `file://` ou em porta não liberada | Usar `http://localhost:8080` ou o Live Server |
| Login demora muito na primeira vez | Banco serverless saindo da pausa | Aguardar alguns segundos e tentar de novo |
| Volta para o login sozinho | Token expirado (8 horas) ou aba fechada | Entrar de novo |
| Acentos estranhos em uma página | Arquivo salvo com codificação errada | Rodar `reparar-codificacao.ps1` |
