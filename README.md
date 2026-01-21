# CPF Validator (Azure Functions)

Microsserviço **serverless** para validação de CPF (Brasil), com foco em **baixo custo**, **alta disponibilidade** e **escalabilidade automática** usando **Azure Functions**.

## Ferramentas necessárias

- Visual Studio Code ou Visual Studio 2022
- .NET 8 SDK
- Azure CLI
- Azure Functions Core Tools

Recomendado para desenvolvimento local:
- Azurite (emulador do Azure Storage) — pode rodar via Docker ou via npm.

## Stack e arquitetura

- **Azure Functions v4** com **.NET 8 (isolated worker)**
- Stateless (HTTP Trigger) + injeção de dependência
- Telemetria pronta para Application Insights

Documentação adicional:
- `docs/architecture.md`
- `docs/api.md`
- `docs/security.md`

## Estrutura do repositório

- `src/CpfValidator.FunctionApp`: Azure Functions App
- `tests/CpfValidator.Tests`: testes unitários do algoritmo de validação
- `infra/bicep`: IaC (Bicep) para provisionar recursos
- `.github/workflows`: CI e deploy manual

## Endpoints

Base path: `/api`

- `GET /api/health`
- `GET /api/cpf/validate/{cpf}`
- `GET /api/cpf/validate?cpf=...`
- `POST /api/cpf/validate` (JSON: `{ "cpf": "..." }`)

Veja exemplos em `docs/api.md`.

## Rodar localmente

### 1) Criar local.settings.json

Copie o arquivo de exemplo:

```bash
cp src/CpfValidator.FunctionApp/local.settings.json.example src/CpfValidator.FunctionApp/local.settings.json
```

### 2) Iniciar Azurite (Storage emulator)

Opção A (Docker):

```bash
docker run --rm -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

Opção B (npm):

```bash
npm i -g azurite
azurite
```

### 3) Subir a Function

```bash
cd src/CpfValidator.FunctionApp
func start
```

Observação sobre autenticação: o endpoint de validação usa `AuthorizationLevel.Function`. Ao iniciar, o Core Tools imprime as URLs já com `?code=...`; use esse `code` para testar.

## Testes

Na raiz do repositório:

```bash
dotnet test
```

## Deploy (infra + app)

### 1) Provisionar recursos (Bicep)

Veja `infra/bicep/README.md`.

Resumo:

```bash
az login
az group create -n <RG_NAME> -l <LOCATION>

az deployment group create \
  -g <RG_NAME> \
  -f infra/bicep/main.bicep \
  -p functionAppName=<UNIQUE_FUNCTION_APP_NAME> \
     storageAccountName=<UNIQUE_STORAGE_NAME>
```

### 2) Publicar a Function

Exemplo (deploy rápido usando Azure Functions Core Tools):

```bash
cd src/CpfValidator.FunctionApp
az login
func azure functionapp publish <NOME_DO_FUNCTION_APP>
```

### 3) CI/CD

- CI: `.github/workflows/ci.yml`
- Deploy manual: `.github/workflows/deploy-azure.yml`

Para o workflow de deploy, configure autenticação no GitHub via:
- OIDC (recomendado) **ou**
- secret `AZURE_CREDENTIALS` com Service Principal.

## Observações

- `local.settings.json` não deve ser commitado (já está no `.gitignore`).
- Se seu ambiente exigir, ajuste versões de pacotes no `.csproj` (target é **Azure Functions v4 + .NET 8**).
- Este serviço é stateless e adequado para plano de consumo (Y1), reduzindo custo quando não há tráfego.

## Licença

MIT
