# Infra (Bicep)

Este diretório contém um template Bicep minimalista para provisionar:
- Storage Account
- Application Insights
- Consumption Plan (Y1)
- Azure Function App **Linux** (.NET Isolated)

## Deploy

```bash
# login
az login

# criar resource group
az group create -n <RG_NAME> -l <LOCATION>

# deploy
az deployment group create \
  -g <RG_NAME> \
  -f infra/bicep/main.bicep \
  -p functionAppName=<UNIQUE_FUNCTION_APP_NAME> \
     storageAccountName=<UNIQUE_STORAGE_NAME>
```

Observações:
- Nomes de Function App e Storage precisam ser globalmente únicos.
- O template cria Function App em **Linux Consumption (Y1)**.
