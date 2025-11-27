# API Contracts: Constitution Compliance

**Feature**: 001-constitution-compliance
**Date**: 2025-11-26

---

## Route Changes (US3: Kebab-case)

### Before → After

| Controller | Rota Anterior | Rota Nova |
|------------|---------------|-----------|
| ConsultaDinamicaController | `/api/ConsultaDinamica/*` | `/api/consulta-dinamica/*` |
| QueryBuilderTestController | `/api/QueryBuilderTest/*` | `/api/query-builder-test/*` |
| MetadadosController | `/api/metadados/*` | `/api/metadados/*` (sem mudanca) |

---

## Endpoints Padronizados

### /api/consulta-dinamica

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| POST | `/api/consulta-dinamica/executar` | Executa consulta dinamica |

### /api/metadados

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/metadados` | Lista todos metadados |
| GET | `/api/metadados/{id}` | Obtem metadado por ID |
| GET | `/api/metadados/tabela/{nomeTabela}` | Obtem metadado por nome da tabela |
| POST | `/api/metadados` | Cria novo metadado |
| PUT | `/api/metadados/{id}` | Atualiza metadado |
| DELETE | `/api/metadados/{id}` | Desativa metadado |

### /api/query-builder-test

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/query-builder-test/*` | Endpoints de teste |

---

## Error Response Contract (US4: Middleware)

### Validation Error (400 Bad Request)

```json
{
  "errors": [
    {
      "key": "NomeTabela",
      "message": "Nome da tabela e obrigatorio"
    }
  ]
}
```

### Business Error (400 Bad Request via NotificationContext)

```json
{
  "errors": [
    {
      "key": "Metadado",
      "message": "Metadado com este nome ja existe"
    }
  ]
}
```

### Internal Error (500 Internal Server Error)

```json
{
  "error": "Erro interno do servidor"
}
```

---

## CancellationToken Support (US2)

Todos os endpoints agora suportam cancelamento via `HttpContext.RequestAborted`.

Comportamento esperado:
- Se cliente desconectar, operacoes em andamento serao canceladas
- Operacoes de banco usam `CommandDefinition` com `cancellationToken`
- Retorno 499 (Client Closed Request) se cancelado
