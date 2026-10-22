# Gestão do Projeto — Tech Quest (PIM IV)

Este documento registra como o grupo se comunicou durante o desenvolvimento do PIM IV e quais riscos foram identificados, avaliados e tratados ao longo do projeto.

---

## 1. Plano de Comunicação

A comunicação do grupo combinou canais **síncronos** (conversa em tempo real) e **assíncronos** (mensagens consultadas depois), cada um com uma finalidade definida. O canal principal foi o **Discord**, complementado pelo **WhatsApp** e por encontros **presenciais** na faculdade.

| Canal | Tipo | Finalidade | Frequência | Participantes |
|---|---|---|---|---|
| Discord — chamadas de voz | Síncrono | Discussões técnicas e desenvolvimento em conjunto | Conforme demanda das sprints | Membros disponíveis no momento |
| Discord — chat de texto | Assíncrono | Registro de anotações, decisões e capturas de tela (prints) do andamento | Contínua | Todo o grupo |
| WhatsApp | Assíncrono | Alinhar detalhes com quem não pôde participar das chamadas | Sempre que necessário | Todo o grupo |
| Reuniões presenciais ("daily") | Síncrono | Alinhamento rápido do andamento, discussão de possibilidades e escolha de caminhos | Nos intervalos das aulas | Todo o grupo |

### 1.1 Funcionamento

As **chamadas de voz do Discord** foram o meio principal de trabalho conjunto. Como o compartilhamento de tela nem sempre era viável durante as chamadas, o **chat de texto do Discord** passou a funcionar como registro do projeto: anotações, decisões e prints do código e das telas ficavam salvos ali, servindo de histórico consultável por todos.

O **WhatsApp** cumpriu o papel de canal de contingência: quando algum integrante não podia participar de uma chamada, os detalhes eram repassados por ele, garantindo que ninguém ficasse desatualizado.

As **dailies presenciais**, inspiradas na *Daily Scrum* do framework Scrum, aconteciam nos intervalos das aulas na própria faculdade. Eram conversas curtas em que o grupo discutia possibilidades, comparava caminhos técnicos e decidia a direção preferida antes de partir para a implementação.

### 1.2 Justificativa da escolha

O Discord foi escolhido por ser gratuito, por oferecer salas de voz permanentes (basta entrar, sem agendar reunião ou gerar link) e por manter o histórico completo das mensagens, o que o torna adequado tanto para conversa quanto para registro. O WhatsApp foi mantido como complemento por ser o aplicativo que todos os integrantes já utilizam diariamente, o que reduz o tempo de resposta para avisos rápidos. Os encontros presenciais aproveitaram a rotina acadêmica comum do grupo, sem exigir horários extras.

### 1.3 Alternativas consideradas

- **Microsoft Teams:** ferramenta mais voltada ao ambiente corporativo, com reuniões estruturadas por agendamento; considerada mais pesada que o necessário para um grupo pequeno.
- **Slack:** o plano gratuito limita o histórico de mensagens visível e restringe chamadas em grupo, o que prejudicaria justamente o uso do chat como registro.
- **Google Meet:** exige criar e compartilhar um link a cada reunião e não oferece um chat persistente entre reuniões.
- **Somente WhatsApp:** inadequado para sessões longas de trabalho em conjunto e com histórico difícil de organizar por assunto.

---

## 2. Gerenciamento de Riscos

### 2.1 Metodologia

Foi adotada a **análise qualitativa de riscos** com matriz de probabilidade × impacto 5×5, conforme o *PMBOK® Guide* (PMI) e alinhada à norma ABNT NBR ISO 31000. Cada risco recebe uma nota de 1 a 5 em probabilidade (P) e em impacto (I), e a **exposição** é o produto entre elas (P × I), variando de 1 a 25.

**Escala de probabilidade**

| Nível | Classificação | Chance estimada de ocorrência |
|:---:|---|---|
| 1 | Muito baixa | até 10% |
| 2 | Baixa | 10% a 30% |
| 3 | Média | 30% a 50% |
| 4 | Alta | 50% a 70% |
| 5 | Muito alta | acima de 70% |

**Escala de impacto** (efeito sobre prazo, escopo e qualidade)

| Nível | Classificação | Efeito no projeto |
|:---:|---|---|
| 1 | Desprezível | Ajuste rápido, sem efeito perceptível |
| 2 | Baixo | Pequeno retrabalho, sem afetar o cronograma |
| 3 | Moderado | Atraso de alguns dias ou funcionalidade entregue parcialmente |
| 4 | Alto | Compromete uma das versões do sistema (web, desktop ou mobile) |
| 5 | Crítico | Compromete a entrega final ou a apresentação do projeto |

**Classificação da exposição**

| Exposição (P × I) | Classificação |
|:---:|---|
| 1 a 4 | 🟩 Baixa |
| 5 a 12 | 🟨 Média |
| 15 a 25 | 🟥 Alta |

As respostas possíveis a cada risco seguem as estratégias do PMBOK: **evitar** (eliminar a causa), **mitigar** (reduzir probabilidade ou impacto), **transferir** (passar a responsabilidade a terceiros) e **aceitar** (assumir o risco e monitorar).

### 2.2 Registro de riscos

| ID | Risco | P | I | Exposição | Resposta | Ação | Status |
|:---:|---|:---:|:---:|:---:|:---:|---|:---:|
| R01 | Lacunas no modelo de dados herdado do PIM III (tabelas ausentes, cardinalidade invertida) | 5 | 3 | 🟥 15 | Mitigar | Script SQL aditivo, apenas com `ALTER TABLE ... ADD`, sem alterar o modelo original | Ocorreu — tratado |
| R02 | Esgotamento de créditos ou suspensão dos serviços Azure | 3 | 5 | 🟥 15 | Mitigar | Uso de camadas básicas de serviço, apenas a API hospedada no App Service, acompanhamento de custos | Ativo — monitorado |
| R03 | Atraso na documentação ABNT em relação ao prazo final | 3 | 5 | 🟥 15 | Mitigar | Desenvolvimento concluído antes, documentação tratada como etapa dedicada | Ativo |
| R04 | Falha de integração entre web, desktop, mobile e API | 3 | 4 | 🟨 12 | Evitar | API construída e validada primeiro, contrato JSON único para todos os clientes | Mitigado |
| R05 | Indisponibilidade de integrantes ou divisão desigual de tarefas | 3 | 4 | 🟨 12 | Mitigar | Divisão do trabalho por frentes, alinhamento pelo Discord, WhatsApp e dailies | Mitigado |
| R06 | Curva de aprendizado em tecnologias novas (.NET, React Native, Azure) | 4 | 3 | 🟨 12 | Mitigar | Desenvolvimento incremental, uma camada por vez | Mitigado |
| R07 | Indisponibilidade do Azure SQL (banco único, sem instância local) | 2 | 4 | 🟨 8 | Aceitar | Confiança no SLA do provedor; scripts versionados permitem recriar o banco | Aceito |
| R08 | Vazamento de credenciais (string de conexão, senhas de usuários) | 2 | 5 | 🟨 10 | Mitigar | Senhas armazenadas com hash (RNF01), segredos mantidos fora do repositório | Mitigado |
| R09 | Falha na geração ou instalação do APK do aplicativo mobile | 3 | 3 | 🟨 9 | Mitigar | Testes de instalação em aparelho físico antes da apresentação | Ativo |
| R10 | Perda de código ou conflitos entre versões | 2 | 4 | 🟨 8 | Mitigar | Versionamento com Git e commits frequentes no repositório | Mitigado |

### 2.3 Matriz de probabilidade × impacto

Cada célula mostra a exposição (P × I) e os riscos posicionados nela.

| Probabilidade ↓ / Impacto → | 1 — Desprezível | 2 — Baixo | 3 — Moderado | 4 — Alto | 5 — Crítico |
|---|:---:|:---:|:---:|:---:|:---:|
| **5 — Muito alta** | 🟨 5 | 🟨 10 | 🟥 15<br>**R01** | 🟥 20 | 🟥 25 |
| **4 — Alta** | 🟩 4 | 🟨 8 | 🟨 12<br>**R06** | 🟥 16 | 🟥 20 |
| **3 — Média** | 🟩 3 | 🟨 6 | 🟨 9<br>**R09** | 🟨 12<br>**R04, R05** | 🟥 15<br>**R02, R03** |
| **2 — Baixa** | 🟩 2 | 🟩 4 | 🟨 6 | 🟨 8<br>**R07, R10** | 🟨 10<br>**R08** |
| **1 — Muito baixa** | 🟩 1 | 🟩 2 | 🟩 3 | 🟩 4 | 🟨 5 |

### 2.4 Análise

Três riscos foram classificados com exposição alta. O **R01** foi o único que de fato se concretizou: as lacunas do banco herdado do PIM III apareceram durante a integração, e a resposta adotada (script aditivo) permitiu completar o modelo sem descaracterizar a modelagem já entregue. Os riscos **R02** e **R03** permanecem ativos até a entrega final, pois dependem, respectivamente, da continuidade dos serviços em nuvem e da conclusão da documentação. Os demais riscos, de exposição média, foram mitigados principalmente pela ordem de desenvolvimento adotada (API primeiro, depois os clientes) e pela estrutura de comunicação descrita na seção 1.

---

## Referências

ASSOCIAÇÃO BRASILEIRA DE NORMAS TÉCNICAS. **NBR ISO 31000**: gestão de riscos — diretrizes. Rio de Janeiro: ABNT, 2018.

PROJECT MANAGEMENT INSTITUTE. **Um guia do conhecimento em gerenciamento de projetos (Guia PMBOK)**. 7. ed. Newtown Square: PMI, 2021.

SCHWABER, K.; SUTHERLAND, J. **The Scrum Guide**. 2020. Disponível em: https://scrumguides.org. Acesso em: out. 2026.
