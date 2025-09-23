# Capital Gains Web API

Esta é uma API REST para cálculo de impostos sobre ganhos de capital, evoluída a partir da aplicação console original.

## Endpoints Disponíveis

### 1. Health Check
```
GET /api/capitalgains/health
```
Verifica se a API está funcionando corretamente.

**Resposta:**
```json
{
  "status": "healthy",
  "timestamp": "2025-09-23T10:30:00.000Z"
}
```

### 2. Calcular Impostos via Requisição JSON
```
POST /api/capitalgains/calculate
Content-Type: application/json
```

Processa uma lista de operações enviadas diretamente no corpo da requisição.

**Payload de exemplo:**
```json
{
  "operations": [
    {"operation": "buy", "unit-cost": 10.00, "quantity": 100},
    {"operation": "sell", "unit-cost": 15.00, "quantity": 50},
    {"operation": "sell", "unit-cost": 15.00, "quantity": 50}
  ]
}
```

**Resposta:**
```json
{
  "taxes": [
    {"tax": 0.0},
    {"tax": 0.0},
    {"tax": 0.0}
  ]
}
```

### 3. Upload de Arquivo
```
POST /api/capitalgains/upload
Content-Type: multipart/form-data
```

Permite fazer upload de um arquivo `.txt` ou `.json` contendo operações para processar.

**Formatos de arquivo suportados:**

#### Arquivo JSON (.json):
```json
[{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50}]
```

#### Arquivo TXT (.txt):
Cada linha deve conter um array JSON válido:
```
[{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50}]
[{"operation":"buy","unit-cost":20.00,"quantity":200},{"operation":"sell","unit-cost":25.00,"quantity":100}]
```

**Resposta:**
```json
{
  "taxes": [
    {"tax": 0.0},
    {"tax": 0.0},
    {"tax": 0.0},
    {"tax": 0.0}
  ]
}
```

## Como Executar a API

1. **Compilar o projeto:**
```bash
dotnet build src/CapitalGains.WebApi
```

2. **Executar a API:**
```bash
dotnet run --project src/CapitalGains.WebApi
```

A API estará disponível em:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

3. **Acessar a documentação Swagger:**
Abra o navegador em http://localhost:5000 para ver a interface do Swagger UI.

## Exemplos de Uso

### Usando cURL

#### Health Check:
```bash
curl -X GET "http://localhost:5000/api/capitalgains/health"
```

#### Calcular via JSON:
```bash
curl -X POST "http://localhost:5000/api/capitalgains/calculate" \
  -H "Content-Type: application/json" \
  -d '{
    "operations": [
      {"operation": "buy", "unit-cost": 10.00, "quantity": 100},
      {"operation": "sell", "unit-cost": 15.00, "quantity": 50}
    ]
  }'
```

#### Upload de arquivo:
```bash
curl -X POST "http://localhost:5000/api/capitalgains/upload" \
  -F "file=@operations.json"
```

### Usando PowerShell

#### Calcular via JSON:
```powershell
$body = @{
    operations = @(
        @{ operation = "buy"; "unit-cost" = 10.00; quantity = 100 },
        @{ operation = "sell"; "unit-cost" = 15.00; quantity = 50 }
    )
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "http://localhost:5000/api/capitalgains/calculate" `
                  -Method POST `
                  -Body $body `
                  -ContentType "application/json"
```

## Tratamento de Erros

A API retorna os seguintes códigos de status HTTP:

- **200 OK**: Requisição processada com sucesso
- **400 Bad Request**: Dados inválidos ou malformados
- **500 Internal Server Error**: Erro interno do servidor

Exemplos de respostas de erro:

```json
// 400 Bad Request
"Operations list cannot be empty"

// 400 Bad Request  
"Only .txt and .json files are supported"

// 500 Internal Server Error
"Internal server error occurred while processing the request"
```

## Estrutura dos Dados

### Operation (Operação)
- **operation**: string - Tipo da operação ("buy" ou "sell")
- **unit-cost**: decimal - Preço unitário da ação
- **quantity**: integer - Quantidade de ações

### TaxResult (Resultado do Imposto)
- **tax**: decimal - Valor do imposto calculado

## Testes

Para executar os testes da API:

```bash
dotnet test tests/CapitalGains.WebApi.Tests
```