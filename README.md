# 🛡️ API Sentinel — Observabilidade e Diagnóstico Inteligente de Falhas

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![n8n](https://img.shields.io/badge/n8n-FF6D5A?style=for-the-badge&logo=n8n&logoColor=white)
![Google Gemini](https://img.shields.io/badge/Google_Gemini-8E75B2?style=for-the-badge&logo=googlebard&logoColor=white)
![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)
![MIT License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

## 🎯 Objetivo e Proposta de Valor

### O problema

Em ambientes corporativos baseados em microsserviços, falhas críticas podem gerar grandes volumes de logs e aumentar o tempo necessário para diagnóstico e resolução de incidentes (*Mean Time to Resolution — MTTR*).

Além disso, processos de registro e notificação executados diretamente no fluxo principal da requisição podem adicionar latência desnecessária à aplicação.

### A solução

O **API Sentinel** implementa um ecossistema de observabilidade e diagnóstico de falhas.

A aplicação:

* Padroniza respostas de erro utilizando a especificação **RFC 7807 — Problem Details**.
* Intercepta exceções de forma centralizada através de middleware.
* Envia o contexto do erro de forma assíncrona utilizando o padrão *Fire-and-Forget*.
* Encaminha os dados para um fluxo automatizado no **n8n**.
* Utiliza o **Google Gemini** para analisar o erro e gerar um diagnóstico técnico.
* Envia o resultado do diagnóstico para um canal técnico do **Discord**.

### 🤖 Diagnóstico com IA

O fluxo de automação recebe informações como:

* Nome do serviço
* Timestamp
* Tipo da exceção
* Mensagem de erro
* Stack trace

O **Google Gemini**, atuando como modelo de linguagem de um AI Agent no n8n, analisa esse contexto e produz um diagnóstico em Markdown com três pontos: **causa provável** do erro, **onde investigar** (arquivo/linha indicados na stack trace) e **sugestão de correção**.

---

## 🛠️ Tecnologias Utilizadas

* **Runtime & Linguagem:** .NET 10 e C# 14
* **Framework Web:** ASP.NET Core
* **Documentação:** OpenAPI / Swagger (Swashbuckle.AspNetCore)
* **Resiliência & Observabilidade:**

  * ASP.NET Core Middleware
  * RFC 7807 — Problem Details
  * `IHttpClientFactory`
  * Comunicação assíncrona *Fire-and-Forget*
* **Conteinerização:** Docker e Docker Compose
* **Orquestração:** n8n
* **Inteligência Artificial:** Google Gemini
* **Comunicação:** Discord Webhooks

---

## 🏗️ Domínio da API e Arquitetura Modular

A aplicação simula o backend de um **Gateway de Pagamentos**, representado pelo projeto `PaymentFlow.Api`.

O cenário foi escolhido por representar um domínio no qual falhas de infraestrutura exigem rastreabilidade e diagnóstico rápidos.

### Por que a divisão em dois projetos?

A solução foi estruturada buscando **separação de responsabilidades**, **baixo acoplamento** e **reutilização do módulo de observabilidade**.

### `PaymentFlow.Api`

Aplicação principal responsável pelo fluxo da API e pelas regras relacionadas ao domínio de pagamentos.

**Responsabilidades:**

* Regras de negócio
* Validações
* Controllers
* Endpoints HTTP
* Integração com o módulo de diagnóstico

**Endpoints principais:**

| Método | Endpoint                   | Descrição                                                         |
| ------ | -------------------------- | ------------------------------------------------------------------ |
| `POST` | `/api/v1/payments/process` | Simula uma falha crítica não tratada (`NullReferenceException`) para acionar a esteira de observabilidade |
| `GET`  | `/api/v1/orders`           | Retorna uma lista simulada de pedidos (dados estáticos em memória), usada para validar o "caminho feliz" sem interferência do middleware |

### `PaymentFlow.Diagnostics`

Biblioteca interna responsável pela infraestrutura de observabilidade.

**Principais componentes:**

* `SentinelExceptionMiddleware` — middleware global de captura de exceções
* `HttpTelemetrySender` — serviço de despacho HTTP assíncrono para o webhook do n8n
* `ErrorTelemetryPayload` — modelo de dados da telemetria
* `SentinelOptions` — opções de configuração (`ServiceName`, `WebhookUrl`, `Enabled`) vinculadas à seção `Sentinel` do `appsettings.json`
* `SentinelServiceExtensions` — extensões (`AddSentinelDiagnostics`, `UseSentinelDiagnostics`) para integração com o `Program.cs` de qualquer aplicação ASP.NET Core

### ♻️ Reutilização

O módulo `PaymentFlow.Diagnostics` foi desenvolvido de forma independente da regra de negócio da aplicação — não referencia nenhum tipo do domínio de pagamentos.

Hoje ele é uma **biblioteca interna**, referenciada via `ProjectReference`, sem metadados de empacotamento configurados. Empacotá-la como um **pacote NuGet interno** reutilizável em outros microsserviços é um passo futuro possível, listado em "Próximos Passos".

---

## 🤖 Fluxo de Automação no n8n

O **n8n** atua como orquestrador do fluxo de observabilidade, através do workflow `API Sentinel - Error Observability`.

```text
┌─────────────────────┐
│   PaymentFlow.Api   │
└──────────┬──────────┘
           │
           │ Exceção
           ▼
┌─────────────────────┐
│ SentinelException   │
│     Middleware      │
└──────────┬──────────┘
           │
           │ Telemetria
           ▼
┌─────────────────────┐
│    n8n Webhook       │
│   /payment-errors    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│    Filter Node       │
│ (statusCode >= 500)  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ AI Agent (Gemini     │
│  Chat Model)          │
└──────────┬──────────┘
           │
           │ Diagnóstico
           ▼
┌─────────────────────┐
│   Discord Webhook    │
└─────────────────────┘
```

### Etapas do fluxo

1. **Webhook Node**

   Recebe a telemetria enviada pela API através do endpoint `POST /payment-errors`.

2. **Filter Node**

   Filtra os eventos recebidos por severidade: só permite que incidentes com `statusCode >= 500` prossigam pelo fluxo.

3. **AI Agent — Google Gemini**

   Analisa o contexto da exceção e a *stack trace*, respondendo em Markdown com três pontos:

   * Causa provável
   * Onde investigar (arquivo/linha)
   * Sugestão de correção

4. **Discord Node**

   Formata o resultado da análise e publica o diagnóstico no canal técnico da equipe, via Webhook do Discord (sem necessidade de criar um bot).

---

## 📸 Demonstração Visual e Evidências

### 1. Documentação Interativa e Resposta Padronizada

A API disponibiliza sua documentação através do Swagger.

Ao executar o endpoint `POST /api/v1/payments/process`, uma exceção é lançada intencionalmente.

O middleware intercepta a falha e retorna uma resposta estruturada seguindo **RFC 7807 — Problem Details**, utilizando `application/problem+json` e status **HTTP 500**.

![Retorno HTTP 500 RFC 7807 no Swagger](docs/images/swagger-error.png)

---

### 2. Orquestração Orientada a Eventos

O n8n recebe a telemetria através do Webhook, filtra por severidade, encaminha o contexto para o AI Agent (Google Gemini) e posteriormente envia o diagnóstico para o Discord.

![Fluxo de Execução no n8n](docs/images/n8n-flow.png)

---

### 3. Diagnóstico Inteligente da Causa Provável

O Google Gemini analisa a *stack trace* e os metadados da exceção.

O resultado inclui a causa provável, o ponto de investigação e a sugestão de correção.

![Alerta do Incidente no Discord — Diagnóstico](docs/images/discord-alert-1.png)

![Alerta do Incidente no Discord — Correção Recomendada](docs/images/discord-alert-2.png)

![Alerta do Incidente no Discord — Observabilidade](docs/images/discord-alert-3.png)

---

## 📁 Estrutura do Repositório

```text
api-sentinel-observability/
├── docs/
│   └── images/
│       ├── swagger-error.png
│       ├── n8n-flow.png
│       ├── discord-alert-1.png
│       ├── discord-alert-2.png
│       └── discord-alert-3.png
│
├── n8n/
│   └── workflow.json
│
├── src/
│   ├── PaymentFlow.Api/
│   │   ├── Controllers/
│   │   ├── Properties/
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   │
│   └── PaymentFlow.Diagnostics/
│       ├── Extensions/
│       ├── Middlewares/
│       ├── Models/
│       ├── Options/
│       └── Services/
│
├── docker-compose.yml
├── Dockerfile
├── .dockerignore
├── PaymentFlow.slnx
└── README.md
```

> ⚠️ O repositório ainda não possui um `.gitignore`. Recomenda-se criar um e remover as pastas `bin/` e `obj/` do controle de versão.

---

## 🚀 Como Executar o Projeto

### Pré-requisitos

Antes de executar o projeto, certifique-se de ter instalado:

* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Docker Desktop](https://www.docker.com/) instalado e em execução (opcional, caso não vá rodar via CLI)
* Uma instância local ou em nuvem do [n8n](https://n8n.io/)

---

### 📥 1. Clonar o Repositório

```bash
git clone https://github.com/Maiquel-Devs/api-sentinel-observability.git
cd api-sentinel-observability
```

---

### 🐳 2a. Executar com Docker

```bash
docker compose up --build
```

A API fica disponível em `http://localhost:5086` (porta do container `8080` mapeada para `5086` no host, conforme `docker-compose.yml`).

Para interromper os containers:

```bash
docker compose down
```

> O `docker-compose.yml` define `ASPNETCORE_ENVIRONMENT=Development`, então o Swagger fica habilitado mesmo rodando em container.

---

### 💻 2b. Executar via .NET CLI (sem Docker)

```bash
dotnet restore PaymentFlow.slnx
dotnet run --project src/PaymentFlow.Api/PaymentFlow.Api.csproj
```

* HTTP: `http://localhost:5086`
* HTTPS: `https://localhost:7173`

---

### 🔑 3. Configurar as Credenciais Externas

O workflow do n8n utiliza duas integrações externas:

* Google Gemini
* Discord Webhook

#### Google Gemini

1. Acesse o [Google AI Studio](https://aistudio.google.com/).
2. Clique em **Get API key**.
3. Gere uma nova API Key.
4. No n8n, abra o nó **Google Gemini Chat Model**.
5. Crie uma nova credencial.
6. Informe a API Key.

> **⚠️ Segurança:** nunca adicione sua API Key diretamente ao código-fonte ou ao repositório Git.

#### Discord Webhook

A integração utiliza um **Webhook do Discord**, não sendo necessário criar um bot.

1. Abra o servidor do Discord.
2. Acesse **Configurações do Servidor → Integrações → Webhooks**.
3. Clique em **Novo Webhook**.
4. Defina um nome para o webhook.
5. Selecione o canal que receberá os alertas.
6. Clique em **Copiar URL do Webhook**.
7. No n8n, abra o nó **Discord**.
8. Configure uma credencial do tipo Webhook.
9. Informe a URL copiada.

> **⚠️ Segurança:** a URL do webhook deve ser tratada como uma credencial secreta. Nunca publique essa URL no repositório.

---

### 🔄 4. Importar o Workflow no n8n

O workflow utilizado pelo projeto está disponível em:

```text
n8n/workflow.json
```

No n8n:

1. Abra a área de workflows.
2. Selecione a opção de importação.
3. Importe o arquivo `n8n/workflow.json`.
4. Configure as credenciais do Google Gemini.
5. Configure as credenciais do Discord.
6. Ative o workflow.

---

### ⚙️ 5. Configurar a URL do Webhook na API

A URL do webhook do n8n é lida da seção `Sentinel:WebhookUrl` em `appsettings.json`.

> **⚠️ Importante:** essa URL **não deve ficar hardcoded** no `appsettings.json` versionado. Configure-a via variável de ambiente (`Sentinel__WebhookUrl`) ou `appsettings.Production.json` fora do controle de versão.

---

### 🧪 6. Testar a Esteira de Observabilidade

Com a API e o workflow do n8n em execução, envie uma requisição para:

```http
POST /api/v1/payments/process
```

O fluxo esperado é:

```text
API
 ↓
Exceção
 ↓
SentinelExceptionMiddleware
 ↓
HTTP 500 + Problem Details
 ↓
Envio assíncrono da telemetria (fire-and-forget)
 ↓
n8n Webhook (/payment-errors)
 ↓
Filter (statusCode >= 500)
 ↓
AI Agent + Google Gemini
 ↓
Diagnóstico
 ↓
Discord
```

A resposta da API permanece padronizada para o cliente, enquanto o diagnóstico é processado de forma independente pelo pipeline de observabilidade.

---

## 🔐 Segurança

O projeto foi desenvolvido com a preocupação de não expor credenciais diretamente no código-fonte.

**Nunca versionar:**

* API Keys
* URLs de Webhooks
* Tokens
* Senhas
* Credenciais de serviços externos

Recomenda-se utilizar variáveis de ambiente, secrets do ambiente de execução ou o sistema de credenciais do próprio n8n.

> ⚠️ **Pendência conhecida:** atualmente a URL do webhook do n8n está fixa em `appsettings.json`. Corrigir antes de tratar este projeto como referência de segurança para produção (ver seção 5).

---

## 🧪 Testes

O repositório ainda não possui um projeto de testes automatizados. Testes futuros deveriam validar principalmente:

* Comportamento do middleware diante de exceções.
* Estrutura das respostas `Problem Details`.
* Status HTTP retornado pela API.
* Disparo da telemetria.
* Integridade do payload enviado ao n8n.
* Processamento do workflow.
* Geração do diagnóstico pela IA.

---

## 📌 Próximos Passos

Possíveis evoluções para o projeto (nenhuma implementada hoje):

* Adicionar testes automatizados de integração.
* Adicionar métricas com OpenTelemetry.
* Implementar correlação através de `TraceId` / `CorrelationId`.
* Adicionar persistência dos incidentes.
* Implementar diferentes níveis de severidade (hoje o Filter só corta em `statusCode >= 500`).
* Criar dashboards de observabilidade.
* Transformar `PaymentFlow.Diagnostics` em um pacote NuGet reutilizável.
* Adicionar suporte a outros canais de notificação.
* Criar um `.gitignore` e remover `bin/`/`obj/` do controle de versão.
* Remover credenciais/URLs hardcoded do `appsettings.json`.

---

## 👨‍💻 Autor

**Maiquel Mafra**

Estudante de Engenharia de Software e desenvolvedor interessado em backend, arquitetura de software, observabilidade, automação e inteligência artificial aplicada ao desenvolvimento de sistemas.

**GitHub:** [Maiquel-Devs](https://github.com/Maiquel-Devs)

---

## 📄 Licença

Este projeto está disponível sob a **licença MIT**, definida no arquivo [`LICENSE`](LICENSE).