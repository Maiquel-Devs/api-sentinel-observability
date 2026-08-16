# 🛡️ API Sentinel - Observabilidade e Diagnóstico Inteligente de Falhas

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![n8n](https://img.shields.io/badge/n8n-FF6D5A?style=for-the-badge&logo=n8n&logoColor=white)
![Google Gemini](https://img.shields.io/badge/Google_Gemini-8E75B2?style=for-the-badge&logo=googlebard&logoColor=white)
![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)

## 🎯 Objetivo e Proposta de Valor

* **O Problema:** Em ambientes corporativos de microsserviços, falhas críticas não tratadas geram *logs* extensos e aumentam o tempo médio de resolução (*Mean Time to Resolution - MTTR*). Muitas vezes, o processo de registrar e notificar esses erros onera diretamente a latência da requisição para o usuário final.
* **A Solução:** O **API Sentinel** implementa um ecossistema de resiliência e observabilidade ativa. A aplicação padroniza as respostas de falha utilizando a especificação **RFC 7807 (Problem Details)** e despacha o contexto do erro em segundo plano através do padrão *Fire-and-Forget*.
* **Diagnóstico com IA:** Um fluxo de automação integrado ao **Google Gemini** analisa a *stack trace* e o contexto do erro em tempo real, gerando um diagnóstico de causa raiz e plano de ação diretamente no canal técnico do **Discord**.

---

## 🛠️ Tecnologias Utilizadas

* **Runtime & Linguagem:** .NET 10 e C# 13.
* **Framework Web:** ASP.NET Core (Minimal APIs / Controllers com documentação OpenAPI/Swagger).
* **Resiliência & Padrões:**
  * **ASP.NET Core Middleware:** Interceptação global de falhas no pipeline de execução HTTP.
  * **RFC 7807 (Problem Details):** Padronização formal das respostas de erro da API.
  * **Fire-and-Forget com `IHttpClientFactory`:** Despacho assíncrono e não bloqueante da telemetria.
* **Conteinerização:** Docker (Multi-stage build) e Docker Compose para paridade e isolamento de ambiente.
* **Orquestração & Automação:** n8n (Workflow Automation orientado a eventos).
* **Inteligência Artificial:** Google Gemini (LLM para análise de *stack trace* e diagnóstico de causa raiz).
* **Comunicação & Alertas:** Webhook integration com o Discord.

---

## 🏗️ Domínio da API e Arquitetura Modular

A aplicação simula o backend de um **Gateway de Pagamentos** (`PaymentFlow.Api`), um cenário financeiro crítico onde qualquer indisponibilidade exige rastreabilidade imediata sem comprometer o fluxo do cliente.

### Por que a divisão em dois projetos na pasta `src/`?

A solução foi estruturada aplicando o **Princípio da Responsabilidade Única (SRP)** e desacoplamento arquitetural:

* **`PaymentFlow.Api` (Aplicação Principal):**
  * Responsável estritamente pelas regras de negócio, validações de domínio e controladores HTTP.
  * **Endpoints expostos:**
    * `POST /api/v1/payments/process`: Simula uma falha severa de infraestrutura/gateway (HTTP 500) para acionar intencionalmente a esteira de telemetria e o agente de IA.
    * `GET /api/v1/orders/{id}`: Endpoint transacional para consulta e rastreio de pedidos.

* **`PaymentFlow.Diagnostics` (SDK Modular de Observabilidade):**
  * Biblioteca isolada e reutilizável contendo o `SentinelExceptionMiddleware` e o serviço de despacho HTTP assíncrono.
  * **Vantagem de Design:** Atua como um módulo *plug-and-play* que pode ser empacotado como um pacote NuGet interno e acoplado a qualquer outro microsserviço da empresa sem alterar a regra de negócio do core.

---

## 🤖 Fluxo de Automação no n8n

O **n8n** atua como o orquestrador orientado a eventos, conectando o disparo da API à inteligência artificial e ao canal de comunicação:

* **Webhook Node:** Escuta requisições HTTP `POST` recebidas da API e extrai o payload com os metadados do erro (*Service Name*, *Timestamp*, *Exception Type*, *Stack Trace*).
* **Filter Node:** Valida a integridade do payload recebido para garantir que apenas exceções válidas avancem no fluxo.
* **AI Agent (Google Gemini):** Recebe o contexto completo do erro. Através de um *system prompt* focado em Engenharia de Software (SRE), o modelo analisa a *stack trace*, identifica a causa raiz provável e gera recomendações práticas de mitigação.
* **Discord Node:** Formata os dados retornados pela IA em um *embed* visual rico e publica o alerta diretamente no canal técnico da equipe.

---

## 📸 Demonstração Visual e Evidências

### 1. Documentação Interativa & Resposta Padronizada (RFC 7807)
A API expõe seus contratos com documentação XML nativa no Swagger. Ao invocar o endpoint `POST /api/v1/Payments/process`, o middleware intercepta a exceção e retorna imediatamente o payload estruturado em `application/problem+json` com status **HTTP 500**:

![Retorno HTTP 500 RFC 7807 no Swagger](docs/images/swagger-error.png)

---

### 2. Orquestração Orientada a Eventos (n8n)
O fluxo recebe a telemetria via Webhook, valida o payload no nó de filtro, consulta o **Google Gemini** através do nó de AI Agent e despacha o alerta formatado para o canal de operações:

![Fluxo de Execução no n8n](docs/images/n8n-flow.png)

---

### 3. Diagnóstico Inteligente de Causa Raiz (Google Gemini & Discord)
O modelo analisa o *stack trace* e os metadados do erro, identificando a linha exata da falha, a provável causa raiz e gerando um plano de ação detalhado com código C# de correção:

![Alerta do Incidente no Discord - Diagnóstico](docs/images/discord-alert-1.png)

![Alerta do Incidente no Discord - Correção Recomendada](docs/images/discord-alert-2.png)

![Alerta do Incidente no Discord - Dica de Observabilidade](docs/images/discord-alert-3.png)

---

## 📁 Estrutura do Repositório

```text
api-sentinel/
├── docs/
│   └── images/                   # Evidências visuais de execução
├── n8n/
│   └── workflow.json             # Fluxo exportado para importação rápida no n8n
├── src/
│   ├── PaymentFlow.Api/          # Camada de aplicação (Controllers e rotas)
│   │   ├── Controllers/
│   │   ├── Properties/
│   │   ├── appsettings.json
│   │   └── Program.cs
│   └── PaymentFlow.Diagnostics/  # SDK/Biblioteca de Observabilidade e Middleware
│       ├── Extensions/
│       ├── Middleware/
│       ├── Models/
│       └── Services/
├── docker-compose.yml            # Orquestração do container da API
├── Dockerfile                    # Multi-stage build otimizado (.NET 10)
├── .gitignore
└── README.md

