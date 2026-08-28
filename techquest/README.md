# Tech Quest — Plataforma de Aprendizagem (versão 2)

Site do PIM III — UNIP ADS. **Versão atualizada** com:
- Todas as páginas do estudante agora são **responsivas desktop** (sidebar lateral aparece automaticamente)
- **2 cursos** disponíveis: Fundamentos de C# e Banco de Dados/Modelagem
- **2 provas distintas** (uma de C#, outra com as questões de OOP/RUP/Brainstorming/etc)
- **Sistema completo de Certificados** (listagem, detalhe, impressão em PDF)
- **Central de Ajuda + FAQ** alimentado pelo JSON
- **Página de Suporte** com formulário de chamado
- **Termos de Uso** e **Política de Privacidade** (LGPD)
- Todos os botões e links agora levam a páginas funcionais

---

## Como rodar

Abra o terminal **dentro da pasta `techquest`** e execute:

```
python -m http.server 8080
```

Acesse no navegador: **http://localhost:8080**

Para parar: `Ctrl + C` no terminal.

---

## Estrutura completa (33 páginas HTML)

```
techquest/
├── index.html                    Landing page
├── css/style.css                 Todo o visual
├── js/app.js                     Lógica (login, quiz, modais, sidebar)
├── data/
│   ├── mock.json                 Personas, cursos, certificados, FAQ
│   └── questoes.json             Banco de questões das 2 provas
└── pages/
    ├── login.html                Login
    ├── cadastro.html             Cadastro
    │
    ├── estudante/  (25 páginas)
    │   ├── home.html
    │   ├── cursos.html           ← NOVA (lista de todos os cursos)
    │   ├── curso.html            Curso de C#
    │   ├── curso-bd.html         ← NOVA (curso de Banco de Dados)
    │   ├── aula.html             Aula de C#
    │   ├── aula-bd.html          ← NOVA (aula de BD/RUP/OOP)
    │   ├── prova.html            Prova de C# (10 questões)
    │   ├── prova-bd.html         ← NOVA (prova das suas 5 questões + complementares)
    │   ├── resultado-aprovado.html
    │   ├── resultado-aprovado-bd.html  ← NOVA
    │   ├── resultado-reprovado.html
    │   ├── conquistas.html
    │   ├── certificados.html     ← NOVA (listagem de certificados)
    │   ├── certificado-detalhe.html  ← NOVA (visualizar/baixar)
    │   ├── perfil.html
    │   ├── editar-perfil.html
    │   ├── alterar-email.html
    │   ├── alterar-senha.html
    │   ├── historico.html
    │   ├── notificacoes.html
    │   ├── configuracoes.html
    │   ├── ajuda.html            ← NOVA (FAQ + Central de Ajuda)
    │   ├── suporte.html          ← NOVA (formulário de chamado)
    │   ├── termos.html           ← NOVA (Termos de Uso)
    │   └── privacidade.html      ← NOVA (Política LGPD)
    │
    ├── tutor/      (3 páginas)
    │   ├── home.html             Dashboard do tutor
    │   ├── alunos.html
    │   └── duvidas.html
    │
    └── admin/      (2 páginas)
        ├── home.html             Dashboard admin
        └── usuarios.html
```

---

## Roteiro de demonstração

1. **Landing** → mostre a proposta visual
2. **Login** (qualquer e-mail/senha entra)
3. **Home do Felipe** → mostra os 2 cursos em andamento, conquistas, atalho de certificados
4. **Cursos** → 2 cursos listados, ambos clicáveis
5. **Fundamentos de C#** → escolha o módulo 3 (Arrays e Listas), entre na aula, faça a prova
6. **Banco de Dados** → escolha o módulo 3 (Análise de Sistemas), entre na aula, faça a prova
   (esta prova usa as 5 questões do JSON original + 3 complementares de BD/SQL)
7. **Certificados** → mostre o certificado já emitido de C# (clique em "Ver" e use "Baixar PDF")
8. **Central de Ajuda + Suporte + Termos + Privacidade** → mostre que todos os botões funcionam
9. **Trocar Persona (botão flutuante)** → vai para Tutor (Ana) ou Admin (Carlos)

---

## Como alterar conteúdo facilmente

| O que mudar | Arquivo |
|---|---|
| Nome, e-mail, dados do Felipe/Ana/Carlos | `data/mock.json` (seção `personas`) |
| Cursos disponíveis, módulos | `data/mock.json` (seção `cursos`) |
| Questões das provas | `data/questoes.json` |
| Certificados (emitidos/disponíveis) | `data/mock.json` (seção `certificados`) |
| FAQ (perguntas frequentes) | `data/mock.json` (seção `faq`) |
| Cores, fontes, espaçamentos | `css/style.css` (no topo, dentro de `:root`) |
| Texto de uma página específica | HTML daquela página |

---

## Justificativas técnicas para a apresentação

- **HTML5 + CSS3 + JavaScript puro (vanilla)**: sem frameworks, refletindo o que foi ensinado este semestre.
- **Arquitetura mobile-first responsiva**: o CSS prioriza o celular e expande pra desktop via `@media queries`.
- **Variáveis CSS (`:root`)**: cores e espaçamentos centralizados.
- **Sidebar injetada via JavaScript**: a sidebar do estudante é adicionada automaticamente pelo `app.js` em todas as páginas, evitando duplicação de código (princípio DRY).
- **Dados em JSON**: desacopla dados de interface, simulando uma API real.
- **Fetch API + Promise.all**: carrega múltiplos arquivos JSON em paralelo.
- **LocalStorage e SessionStorage**: persistência leve de preferências (persona escolhida) e estado temporário (resultado da prova).
- **Window.print() + CSS @media print**: certificado é impresso em PDF direto pelo navegador.
