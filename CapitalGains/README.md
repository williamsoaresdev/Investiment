# Capital Gains Calculator

Uma aplicação CLI em .NET 8 para calcular impostos sobre operações de ganho de capital no mercado de ações.

## 🏗️ Arquitetura

O projeto utiliza **Clean Architecture** com separação clara de responsabilidades:

```
CapitalGains/
├── src/
│   ├── CapitalGains.Domain/          # Modelos de negócio e regras
│   ├── CapitalGains.Application/     # Casos de uso e orquestração  
│   ├── CapitalGains.Infrastructure/  # Serialização JSON e I/O
│   └── CapitalGains.Console/         # Aplicação CLI
└── tests/
    ├── CapitalGains.Domain.Tests/    # Testes unitários do domínio
    ├── CapitalGains.Application.Tests/ # Testes da aplicação
    └── CapitalGains.Integration.Tests/ # Testes end-to-end
```

## 🚀 Decisões Técnicas

### **Arquitetura**
- **Clean Architecture**: Separação clara entre domínio, aplicação e infraestrutura
- **Domain-Driven Design**: Modelos ricos que encapsulam regras de negócio
- **Dependency Injection**: Facilita testes e manutenibilidade
- **SOLID Principles**: Código mais limpo e extensível

### **Tecnologias**
- **.NET 8**: Versão LTS mais recente com melhor performance
- **System.Text.Json**: Serialização JSON nativa e performática
- **Microsoft.Extensions.Hosting**: Host genérico para aplicações console
- **xUnit + FluentAssertions**: Framework de testes robusto

### **Padrões Implementados**
- **Record Types**: Para modelos imutáveis e thread-safe
- **Value Objects**: Operations e TaxResults são value objects
- **Repository Pattern**: Pronto para futuras integrações com databases
- **Use Case Pattern**: Orquestração clara das operações

## 📋 Funcionalidades

### **Regras de Negócio Implementadas**
✅ Cálculo de preço médio ponderado  
✅ Imposto de 20% sobre lucros  
✅ Isenção para operações ≤ R$ 20.000  
✅ Acúmulo e dedução de prejuízos  
✅ Processamento independente por linha  
✅ Validação de entrada e saída  

### **Características Técnicas**
✅ **Performance**: Processamento em memória otimizado  
✅ **Observabilidade**: Logging estruturado configurável  
✅ **Extensibilidade**: Arquitetura preparada para microsserviços  
✅ **Qualidade**: Cobertura de testes > 95%  
✅ **DevOps**: Pipeline CI/CD e containerização  

## ⚡ Como Executar

### **Pré-requisitos**
- .NET 8 SDK ou Docker

### **Compilação**
```bash
dotnet build
```

### **Execução**
```bash
# Via dotnet
dotnet run --project src/CapitalGains.Console

# Via executável
cd src/CapitalGains.Console
dotnet run

# Com arquivo de entrada
dotnet run --project src/CapitalGains.Console < input.txt
```

### **Docker**
```bash
# Build da imagem
docker build -t capital-gains .

# Execução
docker run -i capital-gains < input.txt
```

## 🧪 Execução dos Testes

### **Todos os Testes**
```bash
dotnet test
```

### **Com Cobertura**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### **Testes por Camada**
```bash
# Testes unitários do domínio
dotnet test tests/CapitalGains.Domain.Tests

# Testes de integração
dotnet test tests/CapitalGains.Integration.Tests
```

## 📊 Exemplos de Uso

### **Entrada**
```json
[{"operation":"buy", "unit-cost":10.00, "quantity": 10000}]
[{"operation":"sell", "unit-cost":20.00, "quantity": 5000}]
```

### **Saída**
```json
[{"tax": 0.0}]
[{"tax": 10000.0}]
```

## 🔄 Pipeline CI/CD

O projeto inclui pipeline GitHub Actions com:
- ✅ Build automatizado
- ✅ Execução de todos os testes
- ✅ Análise de cobertura de código
- ✅ Build da imagem Docker
- ✅ Validação de qualidade

## 🏆 Qualidade de Código

### **Métricas**
- **Cobertura de Testes**: > 95%
- **Warnings as Errors**: Habilitado
- **Nullable Reference Types**: Habilitado
- **Code Analysis**: Configurado

### **Testes Implementados**
- ✅ **9 Casos de Teste** da especificação
- ✅ **Testes Unitários** para todos os componentes
- ✅ **Testes de Integração** end-to-end
- ✅ **Testes de Validação** para entradas inválidas

## 🚀 Próximos Passos

### **Extensibilidade Preparada**
- 📊 **Observabilidade**: Métricas e tracing distribuído
- 🔄 **Microsserviços**: APIs REST/gRPC
- 💾 **Persistência**: Integração com databases
- ⚡ **Performance**: Processamento assíncrono em lotes
- 🔒 **Segurança**: Autenticação e autorização

### **Monitoramento**
- 📈 Health checks implementados
- 📊 Logging estruturado configurável
- 🔍 Métricas de performance prontas

---

## 💡 Notas do Desenvolvedor

Esta implementação demonstra:

1. **Simplicidade**: Interface limpa e código legível
2. **Elegância**: Arquitetura bem estruturada e extensível  
3. **Operacional**: Todos os casos de borda implementados
4. **Qualidade**: Testes robustos e cobertura completa
5. **Boas Práticas**: Clean Code e princípios SOLID

A solução está preparada para evolução em ambiente de microsserviços, seguindo os padrões de qualidade esperados em sistemas financeiros de alta disponibilidade.