# 🔧 Capital Gains API - Backend

REST API in .NET 8 for calculating taxes on capital gains operations, implementing Clean Architecture w## 📁 Test Files

### **Included Scenarios**
```bash## 🏛️ Technical Decisions
## 🔧 Development Settings

### **appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CapitalGains": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

### **Debugging**
- ✅ **Swagger UI**: Interactive documentation
- ✅ **Hot Reload**: Real-time changes
- ✅ **Debug Logging**: Detailed tracing
- ✅ **Exception Handling**: Robust error handlingPatterns**
- ✅ **Clean Architecture**: Clear layer separation
- ✅ **Domain-Driven Design**: Rich domain models  
- ✅ **CQRS Pattern**: Command/query separation
- ✅ **Repository Pattern**: Persistence abstraction
- ✅ **Use Case Pattern**: Operation orchestration

### **Performance and Observability**
- ✅ **System.Text.Json**: Optimized native serialization
- ✅ **ILogger**: Configurable structured logging
- ✅ **Health Checks**: API health monitoring
- ✅ **Async/Await**: Non-blocking operations
- ✅ **Memory Management**: Optimized processing├── input.txt                      # Basic official cases
├── capital-gains-test-scenarios.txt  # 9 complete scenarios
├── comprehensive-test-scenarios.txt   # Edge cases
└── example-operations.json           # Pure JSON format
```

### **Test Scenarios**
```bash
# Test all official scenarios
dotnet run --project src/CapitalGains.Console < test-files/capital-gains-test-scenarios.txt

# Upload via API  
curl -X POST "https://localhost:5001/api/capitalgains/upload" \
     -F "file=@test-files/example-operations.json"
```verage.

> **Note**: This is the backend of the solution. For complete project information, see the [main README](../README.md).

## 🏗️ Clean Architecture

```
src/
├── CapitalGains.Domain/          # 📋 Models and business rules
│   ├── Models/                   # Entities: Portfolio, Operation
│   └── Services/                 # Domain Services: CapitalGainsCalculator
│
├── CapitalGains.Application/     # 🔄 Use cases and orchestration
│   └── UseCases/                 # ProcessCapitalGainsUseCase
│
├── CapitalGains.Infrastructure/  # 🛠️ Serialization and I/O
│   ├── Serialization/            # JSON serializers
│   └── IO/                       # File processors
│
├── CapitalGains.Console/         # 💻 Original CLI application
│   └── Program.cs                # Console entry point
│
└── CapitalGains.WebApi/          # 🌐 REST API endpoints
    ├── Controllers/              # CapitalGainsController
    ├── Models/                   # DTOs and request/response models
    └── Swagger/                  # OpenAPI documentation
```

## ⚡ Quick Start

### **Run the API**
```bash
cd capital-gains-backend

# Restore dependencies
dotnet restore

# Run in development
dotnet run --project src/CapitalGains.WebApi

# API available at:
# • HTTP:  http://localhost:5000
# • HTTPS: https://localhost:5001  
# • Swagger: https://localhost:5001/swagger
```

### **Run Console (Original)**
```bash
# Interactive execution
dotnet run --project src/CapitalGains.Console

# With input file
dotnet run --project src/CapitalGains.Console < test-files/input.txt

# Using redirection
echo '[{"operation":"buy","unit-cost":10.00,"quantity":100}]' | dotnet run --project src/CapitalGains.Console
```

### **Docker**
```bash
# REST API
docker build -f Dockerfile.WebApi -t capital-gains-api .
docker run -p 5001:5001 capital-gains-api

# Console CLI  
docker build -f Dockerfile -t capital-gains-cli .
echo '[{"operation":"buy","unit-cost":10.00,"quantity":100}]' | docker run -i capital-gains-cli
```

## 🧪 Run Tests

### **All Tests (57 total)**
```bash
dotnet test --verbosity normal
```

### **By Layer**
```bash
# Domain - 19 unit tests
dotnet test tests/CapitalGains.Domain.Tests

# Integration - 32 end-to-end tests  
dotnet test tests/CapitalGains.Integration.Tests

# WebApi - 6 API tests
dotnet test tests/CapitalGains.WebApi.Tests
```

### **With Code Coverage**
```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report"
```

## 🌐 API Endpoints

### **Swagger UI** 📚
- **URL**: `https://localhost:5001/swagger`  
- **Documentation**: Interactive with examples
- **Test**: Execute directly in the interface

### **Available Endpoints**

| Method | Endpoint | Description | Body |
|--------|----------|-------------|------|
| `GET` | `/api/capitalgains/health` | Health check | - |
| `POST` | `/api/capitalgains/calculate` | Calculate via JSON | `OperationsRequest` |
| `POST` | `/api/capitalgains/upload` | File upload | `multipart/form-data` |

### **Request Example**
```bash
curl -X POST "https://localhost:5001/api/capitalgains/calculate" \
     -H "Content-Type: application/json" \
     -d '{
       "operations": [
         {"operation": "buy", "unitCost": 10.00, "quantity": 100},
         {"operation": "sell", "unitCost": 15.00, "quantity": 50}
       ]
     }'
```

### **Response Example**
```json
{
  "operations": [
    {"operation": "buy", "unitCost": 10.00, "quantity": 100},
    {"operation": "sell", "unitCost": 15.00, "quantity": 50}
  ],
  "taxes": [
    {"tax": 0.0},
    {"tax": 0.0}
  ],
  "scenarios": []
}
```

## � Arquivos de Teste

### **Cenários Incluídos**
```bash
test-files/
├── input.txt                      # Casos básicos oficiais
├── capital-gains-test-scenarios.txt  # 9 cenários completos
├── comprehensive-test-scenarios.txt   # Casos edge
└── example-operations.json           # Formato JSON puro
```

### **Testar Cenários**
```bash
# Testar todos os cenários oficiais
dotnet run --project src/CapitalGains.Console < test-files/capital-gains-test-scenarios.txt

# Upload via API  
curl -X POST "https://localhost:5001/api/capitalgains/upload" \
     -F "file=@test-files/example-operations.json"
```

## 🏆 Quality and Coverage

### **Quality Metrics**
- ✅ **57 tests** (100% passing)
- ✅ **Coverage > 95%** of code
- ✅ **Zero warnings** as errors
- ✅ **Nullable reference types** enabled
- ✅ **Code analysis** rigorously activated

### **Test Types**
- **🧩 Unit**: Domain models and services
- **🔗 Integration**: End-to-end with all scenarios
- **🌐 API**: Controllers and middlewares
- **📊 Performance**: Processing benchmarks

## �️ Decisões Técnicas

### **Patterns Implementados**
- ✅ **Clean Architecture**: Separação clara de camadas
- ✅ **Domain-Driven Design**: Modelos ricos de domínio  
- ✅ **CQRS Pattern**: Separação command/query
- ✅ **Repository Pattern**: Abstração de persistência
- ✅ **Use Case Pattern**: Orquestração de operações

### **Performance e Observabilidade**
- ✅ **System.Text.Json**: Serialização nativa otimizada
- ✅ **ILogger**: Logging estruturado configurável
- ✅ **Health Checks**: Monitoramento de saúde da API
- ✅ **Async/Await**: Operações não-bloqueantes
- ✅ **Memory Management**: Processamento otimizado

## � Configurações de Desenvolvimento

### **appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CapitalGains": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

### **Debugging**
- ✅ **Swagger UI**: Documentação interativa
- ✅ **Hot Reload**: Alterações em tempo real
- ✅ **Debug Logging**: Rastreamento detalhado
- ✅ **Exception Handling**: Error handling robusto

## 🚀 Deploy and Production

### **Docker Production**
```bash
# Optimized build
docker build -f Dockerfile.WebApi -t capital-gains-api:prod .

# Run in production
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production capital-gains-api:prod
```

### **Environment Variables**
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="https://+:5001;http://+:5000"  
export Logging__LogLevel__Default=Warning
```

## 💡 Future Extensions

### **Ready for**
- 📊 **Observability**: Metrics, tracing, APM
- 💾 **Persistence**: Entity Framework integration
- 🔐 **Authentication**: JWT/OAuth2 middleware
- 📈 **Caching**: Redis distributed cache
- 🔄 **Message Queue**: Event-driven architecture
- 🌍 **Microservices**: Service mesh ready

---

## 📞 Technical Information

### **Requirements**
- .NET 8 SDK
- Visual Studio 2022 / VS Code / Rider
- Docker (optional)

### **Main Packages**
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="xUnit" Version="2.4.2" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

### **Solution Structure**
- 📁 **4 production** projects
- 📁 **4 test** projects  
- 📁 **1 solution** organizer
- 📁 **Multiple** configuration files

*For complete full-stack project information, see the [main README](../README.md).*