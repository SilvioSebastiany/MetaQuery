# Quickstart: Validacao das Mudancas

**Feature**: 001-constitution-compliance
**Date**: 2025-11-26

---

## Pre-requisitos

- Docker rodando com Oracle XE
- .NET 9.0 SDK
- VS Code com extensao REST Client

---

## 1. Build e Start

```powershell
# Build do projeto
dotnet build src/MetaQuery.Api/MetaQuery.Api.csproj

# Iniciar API
dotnet run --project src/MetaQuery.Api/MetaQuery.Api.csproj
```

A API estara disponivel em: `http://localhost:5249`

---

## 2. Testar Rotas Kebab-case (US3)

### Verificar rota consulta-dinamica
```http
GET http://localhost:5249/api/consulta-dinamica
```
**Esperado**: 200 OK ou 404 (rota existe)

### Verificar rota antiga NAO funciona
```http
GET http://localhost:5249/api/ConsultaDinamica
```
**Esperado**: 404 Not Found

### Verificar query-builder-test
```http
GET http://localhost:5249/api/query-builder-test
```
**Esperado**: 200 OK ou endpoint especifico

---

## 3. Testar NotificationContext (US1)

### Criar metadado com dados invalidos
```http
POST http://localhost:5249/api/metadados
Content-Type: application/json

{
  "nomeTabela": "",
  "nomeExibicao": "",
  "esquema": "",
  "colunaChavePrimaria": ""
}
```

**Esperado (ANTES da mudanca)**:
```
500 Internal Server Error com stack trace de ArgumentException
```

**Esperado (DEPOIS da mudanca)**:
```json
{
  "errors": [
    { "key": "NomeTabela", "message": "Nome da tabela e obrigatorio" },
    { "key": "NomeExibicao", "message": "Nome de exibicao e obrigatorio" },
    { "key": "Esquema", "message": "Esquema e obrigatorio" },
    { "key": "ColunaChavePrimaria", "message": "Coluna chave primaria e obrigatoria" }
  ]
}
```

---

## 4. Testar Exception Middleware (US4)

### Verificar que erros sao tratados globalmente
```http
GET http://localhost:5249/api/metadados/99999
```

**Esperado**:
- 404 Not Found (nao 500)
- Sem stack trace exposto
- Resposta JSON padronizada

---

## 5. Testar CancellationToken (US2)

### Teste manual
1. Inicie uma requisicao longa
2. Cancele no meio (Ctrl+C no cliente)
3. Verifique logs da API: deve mostrar `OperationCanceledException`

### Verificar nos logs
```powershell
# Buscar logs de cancelamento
dotnet run --project src/MetaQuery.Api/MetaQuery.Api.csproj 2>&1 | Select-String "Cancel"
```

---

## 6. Rodar Testes Unitarios

```powershell
dotnet test tests/MetaQuery.Tests/MetaQuery.Tests.csproj --verbosity normal
```

**Esperado**: Todos os testes passando

---

## 7. Checklist Rapido

- [ ] `GET /api/consulta-dinamica` responde
- [ ] `GET /api/ConsultaDinamica` retorna 404
- [ ] `GET /api/query-builder-test` responde
- [ ] `POST /api/metadados` com dados vazios retorna 400 com lista de erros
- [ ] Nenhum stack trace exposto em respostas de erro
- [ ] Testes unitarios passam
