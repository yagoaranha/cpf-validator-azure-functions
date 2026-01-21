# API

Base path: `/api`

## 1) Health check

**GET** `/api/health`

Resposta (200):

```json
{
  "status": "ok",
  "service": "cpf-validator",
  "timestampUtc": "2026-01-16T00:00:00+00:00"
}
```

## 2) Validate CPF

### GET (rota)

**GET** `/api/cpf/validate/{cpf}`

Exemplo:

`/api/cpf/validate/52998224725`

### GET (querystring)

**GET** `/api/cpf/validate?cpf=529.982.247-25`

### POST (JSON)

**POST** `/api/cpf/validate`

Body:

```json
{ "cpf": "529.982.247-25" }
```

### Resposta

- **200** quando o CPF foi processado com sucesso (válido ou inválido).
- **400** apenas quando o CPF não foi informado ou não pode ser normalizado para 11 dígitos.

Exemplo (200 - válido):

```json
{
  "input": "529.982.247-25",
  "normalizedCpf": "52998224725",
  "isValid": true,
  "reasonCode": null,
  "reason": null,
  "requestId": "...",
  "timestampUtc": "2026-01-16T00:00:00+00:00"
}
```

Exemplo (200 - inválido):

```json
{
  "input": "52998224726",
  "normalizedCpf": "52998224726",
  "isValid": false,
  "reasonCode": "CPF_CHECKDIGIT_2",
  "reason": "Segundo dígito verificador inválido.",
  "requestId": "...",
  "timestampUtc": "2026-01-16T00:00:00+00:00"
}
```
