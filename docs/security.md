# Segurança

## AuthorizationLevel

O endpoint de validação está configurado como `AuthorizationLevel.Function`.

Isso significa que:
- Em produção, o acesso deve ser feito com **Function Key** (header `x-functions-key` ou querystring `code=...`).
- Em desenvolvimento local, você consegue chamar normalmente.

## Boas práticas adicionais

- Coloque a Function atrás de um **API Management** quando precisar de:
  - rate limiting
  - autenticação forte (JWT/OAuth2)
  - quotas
  - versionamento

- Habilite logs e monitoração no Application Insights.
- Não comite `local.settings.json` no Git.
