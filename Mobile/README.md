# Mobile — Tech Quest App (PIM IV)

Aplicativo Android em **React Native com Expo e TypeScript**, para os perfis **Estudante** e **Tutor**. Tem a mesma aparência da versão Web e consome somente a **API REST publicada no Azure**. É distribuído como **APK**, gerado pelo EAS Build.

| Item | Valor |
| :--- | :--- |
| Tecnologias | React Native 0.86, Expo SDK 57, TypeScript (modo estrito) |
| Navegação | Expo Router (rotas baseadas em arquivos) |
| Plataforma | Android (pacote `br.unip.techquest`) |
| Dados | API do Azure, endereço fixo no app |
| Sessão | Token JWT cifrado no aparelho (`expo-secure-store`, Android Keystore) |
| Acesso | Estudante e Tutor; o Administrador é direcionado ao Desktop |

---

## Funcionalidades

**Estudante** — abas Home, Cursos, Conquistas, Ajuda e Perfil

- Home com nível, XP, "continue de onde parou" e prova liberada.
- Catálogo com busca e filtro, detalhe do curso e matrícula.
- Aula com leitura parcial registrada e conclusão do material.
- Prova corrigida no servidor e resultado com revisão questão a questão.
- Conquistas, certificados (copiar e compartilhar o código), minhas provas, suporte, notificações e edição de perfil.

**Tutor** — abas Painel, Alunos, Dúvidas, Cursos e Conta

- Painel com indicadores; lista de alunos com busca e filtro por curso; detalhe do aluno com mensagem direta.
- Dúvidas: responder, marcar "estou analisando" e encerrar.
- Criação de curso e editor completo: dados, aulas com pré-visualização, prova, questões e submissão para aprovação.
- Notificações com o motivo de um curso rejeitado.

**Comum aos dois:** alterar senha, alterar e-mail, encerrar conta e sair.

---

## Estrutura

```
Mobile/
├── app.json              Nome, ícone, splash e identificador Android
├── eas.json              Perfis de compilação do EAS (o perfil "apk" gera o instalável)
├── package.json          Dependências e comandos (npm run apk, typecheck, lint)
├── tsconfig.json         TypeScript estrito e atalho "@/" para a pasta src
├── eslint.config.js      Regras de qualidade de código
├── assets/               Ícones e imagem de abertura
└── src/
    ├── app/              Telas, organizadas por rota (Expo Router)
    │   ├── login.tsx, cadastro.tsx, index.tsx
    │   ├── aluno/        (abas)/ e telas do estudante: curso, aula, prova, resultado...
    │   ├── tutor/        (abas)/ e telas do tutor: curso, material, prova, questão, aluno...
    │   └── conta/        senha, e-mail e encerrar conta
    ├── api/              cliente.ts (HTTP), endpoints.ts (uma função por rota), tipos.ts
    ├── sessao/           Contexto de sessão e armazenamento seguro do token
    ├── config/           ambiente.ts: URL da API e tempo limite (45 s)
    ├── ui/, tema/        Componentes e tokens visuais copiados do style.css da Web
    ├── telas/, hooks/    Componentes de tela compartilhados e hooks (carregamento, responsividade)
    ├── estado/, util/    Estado do resultado da prova, formatação e utilitários
```

| Arquivo de configuração | Para que serve |
| :--- | :--- |
| `package.json` | Bibliotecas usadas e comandos do projeto |
| `package-lock.json` | Trava a versão exata de cada biblioteca para todo o grupo (não editar à mão) |
| `app.json` | Nome do app, ícone, tela de abertura e identificador Android |
| `eas.json` | Como o EAS compila; o perfil `apk` gera o arquivo instalável |
| `tsconfig.json` | Checagem de tipos e o atalho `@/` para `src` |
| `eslint.config.js` | Regras de padrão de código |

---

## Como gerar o APK

O app é compilado nos servidores do Expo (EAS Build), sem Android Studio nem emulador.

### Pré-requisitos

- Node.js 20 ou superior.
- Conta gratuita em [expo.dev](https://expo.dev).

### Passos

```bash
cd Mobile
npm install
npx eas-cli@latest login
npm run apk
```

Na primeira vez, o EAS pergunta se deve criar o projeto na sua conta e gerar a chave de assinatura Android: responda **Y** nas duas. Em cerca de 10 a 20 minutos aparecem um link e um QR code para baixar o `.apk`.

Para instalar, abra o link no celular, baixe o arquivo e permita a instalação de "fonte desconhecida" quando o Android pedir.

> O link de download do Expo não é permanente. A versão entregue do APK fica publicada na [página de Releases do repositório](https://github.com/WillSennaa/Pim-4/releases/latest). Sempre que o código do app mudar, gere um novo APK e publique uma nova release.

### Baixar o APK pronto

Quem só quer instalar o app não precisa compilar: baixe o `TechQuest-v1.0.0.apk` na [release mais recente](https://github.com/WillSennaa/Pim-4/releases/latest) e instale no celular Android.

### Verificações de código

```bash
npm run typecheck   # TypeScript estrito, sem erros
npm run lint        # ESLint do Expo, sem erros
```

### Contas de demonstração

| Papel | E-mail | Senha |
| :--- | :--- | :--- |
| Estudante | `felipe123@gmail.com` | `aluno123@` |
| Tutor | `tutor123@gmail.com` | `tutor123@` |

Ao entrar com a conta de administrador, o app recusa e indica a versão Desktop.

---

## Integração com a API

- **Endereço fixo:** `src/config/ambiente.ts` aponta para a API do Azure; o endereço vai embutido no APK.
- **Camada única:** nenhuma tela chama `fetch` diretamente. O `api/cliente.ts` anexa o token, aplica o tempo limite de 45 segundos e padroniza os erros, no mesmo formato do `api.js` da Web.
- **Sessão expirada:** uma resposta 401 limpa a sessão e os guardas de rota levam ao login.
- **Sem banco local:** cada tela busca os dados ao abrir e cada ação grava direto no servidor, então Web, Desktop e Mobile mostram sempre o mesmo estado.
- **CORS:** não se aplica ao app nativo (é uma regra do navegador), por isso a API não precisou de ajuste.

---

## Responsividade

- Abaixo de 768 de largura, a navegação é por abas na parte inferior e as telas ficam em 1 coluna.
- A partir de 768, as abas viram menu lateral, como na Web, e as telas passam para 2 ou 3 colunas.
- O layout se ajusta também ao girar o aparelho.

---

## Decisões principais

As justificativas completas estão em [`Docs/README.md`](../Docs/README.md#6-registro-de-decisões-de-arquitetura-adr) (ADR-017).

- **React Native com Expo:** TypeScript próximo do JavaScript já usado no grupo e APK sem Android Studio.
- **Token no SecureStore (Android Keystore)**, e não no `AsyncStorage`, que grava em texto puro.
- **Somente a API do Azure:** o app não tem modo local nem API simulada.
- **Correção da prova no servidor:** a prova chega ao celular sem o gabarito.

---

## Problemas comuns

| Sintoma | Causa provável | Solução |
| :--- | :--- | :--- |
| `npm run apk` pede login | Sessão do EAS não iniciada | `npx eas-cli@latest login` |
| Android bloqueia a instalação | Instalação de fonte desconhecida desativada | Permitir para o navegador ou gerenciador de arquivos usado |
| Primeira tela demora a carregar | Banco serverless saindo da pausa | Aguardar; o tempo limite é de 45 segundos |
| Login de administrador recusado | Perfil não atendido pelo Mobile | Usar a versão Desktop ou Web |
