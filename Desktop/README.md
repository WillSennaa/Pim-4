# Desktop — Tech Quest Administrador (PIM IV)

Aplicação **Windows Forms em C# .NET 8**, exclusiva do perfil **Administrador**, para a gestão da plataforma. Consome a mesma API REST publicada no Azure que a Web e o Mobile usam; não acessa o banco diretamente.

| Item | Valor |
| :--- | :--- |
| Plataforma | Windows, .NET 8 (`net8.0-windows`), Windows Forms |
| Padrão de apresentação | MVP (Model-View-Presenter) |
| Comunicação | `HttpClient` + JSON, token JWT no cabeçalho |
| Acesso | Somente contas de Administrador |
| Testes | xUnit (146 casos), sem rede e sem abrir janelas |

---

## Funcionalidades

| Tela | O que faz | Rotas usadas |
| :--- | :--- | :--- |
| **Login** | Autentica e recusa, com mensagem, contas de Tutor ou Estudante | `POST /api/auth/login` |
| **Painel** | Nove indicadores da plataforma; atalho para aprovações, destacado quando há pendências | `GET /api/admin/resumo` |
| **Aprovações de cursos** | Fila de cursos pendentes, janela de revisão com abas (visão geral, aulas, prova com gabarito), aprovar ou rejeitar com motivo (mínimo de 10 caracteres) | `/api/admin/cursos` |
| **Cursos** | Catálogo completo nos quatro estados (Rascunho, Pendente, Publicado, Rejeitado) | `GET /api/admin/cursos` |
| **Usuários** | Lista com filtros por papel e situação; criar usuário com senha gerada de forma segura e copiada para a área de transferência; ativar e desativar | `/api/admin/usuarios` |
| **Chamados técnicos** | Fila de suporte técnico: alterar a situação (em andamento, resolvido) e responder | `/api/admin/chamados`, `/api/chamados` |
| **Medalhas** | Catálogo de medalhas; renomear pede confirmação, porque as medalhas gerais automáticas são reconhecidas pelo nome | `/api/admin/medalhas` |
| **Logs** | Registros de auditoria com limite configurável | `GET /api/admin/logs` |
| **Minha conta** | Troca de senha e de e-mail | `/api/perfil` |

O menu lateral mostra um selo com a quantidade de cursos aguardando aprovação. Quando o token expira, o sistema volta para a tela de login.

---

## Estrutura

A solução `TechQuest.Desktop.slnx` tem seis projetos. Só o projeto de UI depende do Windows Forms; todo o resto é `net8.0` comum e pode ser testado sem janela.

```
Desktop/
├── TechQuest.Desktop.slnx
├── TechQuest.Desktop.Modelos/       Classes que espelham o JSON da API
├── TechQuest.Desktop.Aplicacao/     Serviços, sessão do administrador, validações,
│                                    interfaces dos gateways e exceções
├── TechQuest.Desktop.Integracao/    ApiCliente (HTTP + JWT) e um gateway por área da API
├── TechQuest.Desktop.Apresentacao/  Presenters e interfaces das telas (MVP), sem WinForms
├── TechQuest.Desktop.UI/            Formulários, telas, componentes e Program.cs
│   ├── Formularios/                 Login, Principal, Revisão de curso, Cadastro de usuário, Medalha
│   ├── Telas/                       Painel, Aprovações, Cursos, Usuários, Chamados, Medalhas,
│   │                                Logs, Minha conta
│   ├── Componentes/, Estilo/        Cartões, grade padrão, faixa de erro, tema e fontes
│   └── appsettings.json             Endereço da API
└── TechQuest.Desktop.Tests/         Testes xUnit com dublês
```

| Projeto | Depende de | Responsabilidade |
| :--- | :--- | :--- |
| Modelos | — | Contrato de dados com a API |
| Aplicacao | Modelos | Regras do cliente (sessão, validação, contagem de pendências) |
| Integracao | Aplicacao | Comunicação HTTP e tradução de status em exceções |
| Apresentacao | Aplicacao | Decide o que cada tela mostra e faz |
| UI | todos | Desenha as telas e monta os objetos (raiz de composição) |

---

## Como executar

### Pré-requisitos

- Windows 10 ou 11.
- Visual Studio 2022 (com suporte a `.slnx`) e a carga de trabalho **Desenvolvimento para desktop com .NET**.

### Passos

1. Abra `Desktop/TechQuest.Desktop.slnx` no Visual Studio.
2. Defina `TechQuest.Desktop.UI` como projeto de inicialização.
3. Pressione F5.
4. Entre com a conta de administrador: `admin123@gmail.com` / `admin123@`.

Não é necessário configurar segredos: a URL da API é pública, e a senha do banco e a chave JWT ficam apenas no servidor.

### Configuração (`TechQuest.Desktop.UI/appsettings.json`)

```json
{
  "Api": {
    "UrlBase": "https://tqbackendapi-cpb2edg0gxcsfggz.brazilsouth-01.azurewebsites.net/",
    "TimeoutSegundos": 60
  }
}
```

Para usar uma API rodando no Visual Studio, troque `UrlBase` por `https://localhost:7160/`. O tempo limite de 60 segundos cobre a saída da pausa do banco serverless.

---

## Testes

```bash
dotnet test TechQuest.Desktop.Tests
```

Cobrem a sessão do administrador, os serviços, todos os presenters (login, painel, aprovações, gestão, suporte, medalhas, conta) e o `ApiCliente`. Usam gateways e telas falsas (`Dubles/`), então não precisam de rede nem abrem janelas.

---

## Decisões principais

As justificativas completas estão em [`Docs/README.md`](../Docs/README.md#6-registro-de-decisões-de-arquitetura-adr) (ADR-016).

- **MVP:** os formulários só desenham e repassam cliques; a lógica fica nos presenters e é testada sem janela.
- **Modelos próprios:** o Desktop conhece a API só pelo JSON e não referencia `TechQuest.Application`.
- **Injeção de dependência manual** em `Program.cs`: um usuário e um grafo pequeno não justificam um container.
- **Um único `HttpClient`** para o programa inteiro: criar um por requisição esgota as portas do sistema.
- **Selo de pendências pelo padrão Observer:** o contador avisa o menu quando a fila de aprovações muda.

---

## Problemas comuns

| Sintoma | Causa provável | Solução |
| :--- | :--- | :--- |
| "Arquivo de configuração não encontrado" | `appsettings.json` não copiado para a pasta de saída | Conferir no Visual Studio que o arquivo está como "Copiar se for mais novo" |
| Login recusado com conta válida | Conta de Tutor ou Estudante | O Desktop é exclusivo do Administrador |
| Primeira operação demora | Banco serverless saindo da pausa | Aguardar; o tempo limite é de 60 segundos |
| Testes não aparecem no Test Explorer | Pacotes não restaurados | **Restaurar pacotes NuGet** e recompilar |
