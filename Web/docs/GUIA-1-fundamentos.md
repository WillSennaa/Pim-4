# 📘 Guia 1 — Fundamentos absolutos
> Leia HOJE (sexta) ou amanhã de manhã. Tempo: ~1h30. Sem isso, o resto não faz sentido.

---

## 🧠 A grande ideia: 3 linguagens, 3 papéis diferentes

Pensa em uma página web como **uma casa**:

| Linguagem | Papel | Analogia da casa |
|---|---|---|
| **HTML** | Estrutura / conteúdo | Os tijolos, paredes, telhado, portas |
| **CSS** | Aparência / visual | A pintura, a decoração, os móveis |
| **JavaScript** | Comportamento / interação | A energia elétrica, encanamento, automações |

Sem HTML, não tem casa. Sem CSS, a casa é feia e desorganizada. Sem JS, a casa não tem luz, água, nem nada que funcione quando você clica.

**No nosso projeto:**
- HTML está nas **47 páginas `.html`** dentro de `pages/` e no `index.html`
- CSS está em **1 único arquivo**: `css/style.css`
- JavaScript está em **1 único arquivo**: `js/app.js`

> 💡 **Por que só 1 arquivo CSS e 1 arquivo JS?** Porque todas as páginas usam o mesmo visual e a mesma lógica. Em vez de copiar e colar em cada uma, a gente escreve uma vez só e todas elas "incluem" esses arquivos. Isso se chama **DRY** (*Don't Repeat Yourself* — "não se repita").

---

## 1️⃣ HTML — A estrutura

HTML quer dizer **HyperText Markup Language** (linguagem de marcação de hipertexto). Ele usa **tags** entre `<` e `>` para marcar o que cada pedaço da página é.

### Anatomia de uma tag

```html
<h1>Olá mundo</h1>
```

- `<h1>` é a **tag de abertura**
- `Olá mundo` é o **conteúdo**
- `</h1>` é a **tag de fechamento** (com a barra `/`)

A maioria das tags abre e fecha. Algumas são "auto-fechadas" (não têm conteúdo dentro), como `<img>` e `<input>`.

### Tags que aparecem MUITO no nosso projeto

| Tag | Para que serve | Exemplo |
|---|---|---|
| `<div>` | Caixa genérica, agrupa coisas | `<div class="card">...</div>` |
| `<span>` | Caixa genérica em linha (não quebra) | `<span class="badge">NOVO</span>` |
| `<h1>` a `<h6>` | Títulos (do maior pro menor) | `<h1>Tech Quest</h1>` |
| `<p>` | Parágrafo de texto | `<p>Comece sua jornada.</p>` |
| `<a>` | Link (âncora) | `<a href="login.html">Entrar</a>` |
| `<button>` | Botão clicável | `<button>Salvar</button>` |
| `<input>` | Campo de formulário | `<input type="email">` |
| `<form>` | Agrupa campos de formulário | `<form>...</form>` |
| `<img>` | Imagem | `<img src="logo.png">` |
| `<i>` | Ícone (no nosso caso, do Tabler Icons) | `<i class="ti ti-home"></i>` |

### Atributos

Tags podem ter **atributos**, que são informações extras dentro da tag de abertura:

```html
<a href="login.html" class="btn">Entrar</a>
```

- `href="login.html"` → para onde o link leva
- `class="btn"` → o "rótulo" que o CSS usa para identificar e estilizar esse elemento

> 🔑 **`class` é a palavra mais importante para você decorar.** É a ponte entre HTML e CSS. Toda vez que você quiser estilizar algo, você coloca uma `class` no HTML e usa essa mesma `class` no CSS pra dizer como ela deve aparecer.

### Estrutura mínima de uma página HTML

Toda página `.html` do projeto começa assim:

```html
<!DOCTYPE html>                  <!-- Diz ao navegador: isto é HTML5 -->
<html lang="pt-BR">              <!-- Idioma da página -->
<head>                           <!-- Cabeçalho: metadados (não aparece na tela) -->
  <meta charset="UTF-8">         <!-- Codificação (acentos funcionarem) -->
  <title>Tech Quest</title>      <!-- Nome na aba do navegador -->
  <link rel="stylesheet" href="css/style.css">  <!-- Inclui o CSS -->
</head>
<body>                           <!-- Corpo: o que aparece na tela -->
  <h1>Olá!</h1>
</body>
</html>
```

> 💡 O `<head>` é onde a página **se prepara** (carrega CSS, define título, etc). O `<body>` é onde está **o que o usuário vê**.

---

## 2️⃣ CSS — A aparência

CSS quer dizer **Cascading Style Sheets** (folhas de estilo em cascata). É a linguagem que **decora** o HTML.

### Anatomia de uma regra CSS

```css
.card {
  background: white;
  padding: 16px;
  border-radius: 8px;
}
```

Desmembrando:

- `.card` é o **seletor** — diz QUEM vai ser estilizado
- `{ }` delimitam o **bloco de declarações**
- `background: white;` é uma **declaração** (propriedade + valor)
- O **ponto** antes de `card` significa "qualquer elemento com a `class="card"` no HTML"

> 🎯 Casa o HTML `<div class="card">` com o CSS `.card { ... }`. Esse pareamento é o coração do CSS.

### Tipos de seletor que você vai ver no projeto

| Seletor | Significa | Exemplo no nosso código |
|---|---|---|
| `.algo` | Tudo com a classe `algo` | `.btn` estiliza todo `<button class="btn">` |
| `#algo` | O elemento com `id="algo"` (único) | `#modalSair` estiliza só esse modal |
| `tag` | Todas as tags daquele tipo | `body { ... }` estiliza o body |
| `.a .b` | Tudo com classe `b` dentro de algo com classe `a` | `.card .titulo` |
| `.a:hover` | Quando o mouse passa por cima | `.btn:hover` muda quando passa o mouse |

### Propriedades mais usadas no projeto

| Propriedade | O que faz |
|---|---|
| `color` | Cor do **texto** |
| `background` | Cor (ou imagem) de **fundo** |
| `padding` | Espaço **interno** (entre a borda e o conteúdo) |
| `margin` | Espaço **externo** (entre este elemento e os vizinhos) |
| `border` | Borda |
| `border-radius` | Cantos arredondados |
| `font-size` | Tamanho da letra |
| `font-weight` | Espessura da letra (400 = normal, 700 = negrito) |
| `display: flex` | Layout flexível (organiza filhos lado a lado ou em coluna) |
| `width` / `height` | Largura / altura |

### Variáveis CSS (`var(--algo)`)

No topo do nosso `style.css` (seção 1), você vê isso:

```css
:root {
  --color-primary: #2563EB;
  --space-md: 12px;
}
```

E em outros lugares:

```css
.btn--primary {
  background: var(--color-primary);
  padding: var(--space-md);
}
```

**O que é isso?** São **variáveis** — apelidos para valores. Em vez de escrever `#2563EB` (o azul da Tech Quest) em 100 lugares, a gente define UMA vez como `--color-primary` e usa em todo lugar com `var(--color-primary)`. Se um dia quisermos mudar a cor, mudamos só no topo e o site inteiro muda.

> 🎯 **Esse é um ponto FORTE para falar na apresentação.** Mostra que você pensou em manutenibilidade.

---

## 3️⃣ JavaScript — O comportamento

JS é a linguagem que faz o site **reagir**. Quando você clica num botão e algo acontece, JS está por trás.

### Conceitos mínimos que você precisa saber

**Variáveis** — guardam valores:
```javascript
let nome = "Felipe";          // pode mudar depois
const idade = 25;             // não pode mudar
```

**Funções** — blocos de código com nome que fazem algo quando "chamados":
```javascript
function dizerOi() {
  alert("Oi!");
}
dizerOi();   // <- aqui chamamos a função, e ela executa
```

**Eventos** — quando algo acontece (clique, envio de formulário, página carregada):
```javascript
window.addEventListener('DOMContentLoaded', function () {
  // este código roda assim que a página carrega
});
```

**Selecionar elementos do HTML** — pra mexer com eles via JS:
```javascript
const botao = document.getElementById('meuBotao');
botao.textContent = 'Texto novo';   // muda o que aparece dentro
```

### `onclick` — o ataque mais frequente do JS no nosso código

No HTML você vai ver muito isso:
```html
<button onclick="abrirModal('modalSair')">Sair</button>
```

**Tradução:** "quando este botão for clicado, execute a função `abrirModal` passando o texto `modalSair`."

A função `abrirModal` está definida no `app.js`. É uma função que **adiciona uma classe `active` no modal** com aquele ID, fazendo ele aparecer (porque o CSS diz: modal só fica visível se tiver a classe `active`).

---

## 4️⃣ Como os 3 trabalham juntos — exemplo real do projeto

Vamos pegar **o botão "Começar" da home do estudante** e ver tudo funcionando junto.

### HTML (em `home.html`):
```html
<button class="btn btn--secondary btn--full mt-md"
        onclick="abrirModal('modalIniciarProva')">
  Começar
</button>
```

**O que cada parte faz:**
- `<button>` → cria um botão clicável
- `class="btn btn--secondary btn--full mt-md"` → aplica 4 estilos do CSS
- `onclick="abrirModal('modalIniciarProva')"` → quando clicado, chama o JS

### CSS (em `style.css`):
```css
.btn {
  padding: 12px 20px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
}
.btn--secondary {
  background: var(--color-warning);
  color: white;
}
.btn--full { width: 100%; }
.mt-md { margin-top: 12px; }
```

**Resultado visual:** um botão amarelo, largura total, com cantos arredondados e espaço acima.

### JavaScript (em `app.js`):
```javascript
function abrirModal(id) {
  const m = document.getElementById(id);
  if (m) m.classList.add('active');
}
```

**O que faz:** pega o elemento com `id="modalIniciarProva"` e adiciona a classe `active` nele.

### E o modal?
No HTML existe:
```html
<div class="modal-overlay" id="modalIniciarProva">
  <div class="modal">...</div>
</div>
```

E no CSS:
```css
.modal-overlay {
  display: none;        /* invisível por padrão */
}
.modal-overlay.active {
  display: flex;        /* aparece quando tem a classe active */
}
```

**🎬 O ciclo completo:**
1. Usuário clica no botão "Começar"
2. O `onclick` dispara a função `abrirModal('modalIniciarProva')`
3. JavaScript encontra a `<div>` com esse id e adiciona a classe `active`
4. Com a classe `active`, o CSS faz a `<div>` mudar de `display: none` para `display: flex`
5. O modal aparece na tela

> 🔥 **Decora esse fluxo.** Se você souber explicar esse exemplo, você sabe explicar 80% do site. Praticamente todo botão/modal/menu funciona assim.

---

## 5️⃣ JSON — o formato dos dados

Você vai ver `data/mock.json` e `data/questoes.json`. JSON significa **JavaScript Object Notation** — um formato pra organizar **dados** (não código).

```json
{
  "nome": "Felipe",
  "idade": 25,
  "cursos": ["C#", "Banco de Dados"]
}
```

**Regras:**
- Tudo entre chaves `{ }` é um **objeto** (conjunto de chave: valor)
- Tudo entre colchetes `[ ]` é uma **lista** (array)
- Chaves são sempre entre aspas `"..."`
- Valores podem ser: texto `"..."`, número `25`, lista `[...]` ou outro objeto `{...}`

**Como o JSON é usado no nosso projeto:**
1. O JavaScript (`app.js`) faz um `fetch('data/mock.json')` — busca o arquivo
2. Recebe o conteúdo como dados estruturados
3. Usa esses dados pra preencher a tela dinamicamente (ex: a lista de cursos vem daqui)

> 🎯 **Importante na apresentação:** isso simula um **banco de dados / API**. Em produção, em vez de ler um arquivo `.json`, o JS faria a mesma coisa contra uma URL de servidor. A lógica seria idêntica.

---

## ✅ Checklist do Guia 1

Antes de seguir pro Guia 2, confirma que você sabe responder:

- [ ] Qual a diferença entre HTML, CSS e JavaScript?
- [ ] O que é uma tag? E um atributo?
- [ ] O que é uma classe (`class`)?
- [ ] Como o CSS "encontra" o elemento HTML que vai estilizar?
- [ ] O que faz `var(--color-primary)`?
- [ ] O que é uma função em JavaScript?
- [ ] O que acontece quando o usuário clica num botão com `onclick="algo()"`?
- [ ] O que é JSON e pra que serve no projeto?

Se respondeu sim pra todas, está pronto pro Guia 2!

---

**🎯 Próximo:** Guia 2 — Estrutura do projeto (entender pasta por pasta, arquivo por arquivo, sem precisar entender cada linha).
