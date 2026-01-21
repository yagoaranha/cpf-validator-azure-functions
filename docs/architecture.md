# Arquitetura

## Objetivo

Disponibilizar um microsserviço simples e altamente escalável para validação de CPF, com baixo custo operacional, rodando em **Azure Functions (serverless)**.

## Componentes

- **Azure Functions (Consumption Plan / Y1)**
  - Executa sob demanda, escala automaticamente.
  - Sem necessidade de VM/Container sempre ligado.
- **Storage Account**
  - Necessário para runtime de Functions.
- **Application Insights**
  - Telemetria (logs, métricas, tracing).

## Fluxo

1. Cliente chama endpoint HTTP (GET/POST).
2. Function normaliza o CPF (remove caracteres não numéricos).
3. Validador aplica o algoritmo de dígitos verificadores.
4. Resposta JSON com `isValid` e motivos de falha (quando aplicável).

## Por que serverless?

- **Custo**: modelo pay-per-execution (ideal para serviços de utilidade).
- **Escalabilidade**: escala horizontal automática.
- **Manutenção**: menos componentes para operar.
