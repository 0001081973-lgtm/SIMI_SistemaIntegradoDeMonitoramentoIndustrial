# SIMI — Sistema Integrado de Monitoramento Industrial

> **Atividade:** Documentação, Persistência e Publicação de API de Sensores  
> **Tecnologias:** .NET 8 · ASP.NET Core Web API · Entity Framework Core · SQLite · WPF · Swagger/OpenAPI

---

## Visão Geral

O SIMI é um sistema distribuído para monitoramento de sensores industriais em tempo real. Ele é composto por quatro componentes que se comunicam entre si:

| Componente | Tipo | Descrição |
|---|---|---|
| `Shared` | Class Library | Modelo de dados compartilhado (`SensorData`) |
| `ApiProcessamento` | ASP.NET Web API | Recebe, valida e persiste os dados dos sensores |
| `SensorSimulator` | Console App | Gera dados simulados e os envia à API a cada 2 segundos |
| `SensorInterface` | WPF Desktop App | Interface gráfica para visualização dos dados |

---

## Arquitetura

```
┌─────────────────┐        POST /api/v1/sensores        ┌─────────────────────┐
│  SensorSimulator │ ──────────────────────────────────► │   ApiProcessamento  │
│  (Console App)   │                                     │   (ASP.NET Web API) │
│                  │  Persiste em:                       │                     │
│  simulator_log.db│  simulator_log.db (local)           │  simi_sensores.db   │
└─────────────────┘                                     │  (EF Core + SQLite) │
                                                         └──────────┬──────────┘
┌─────────────────┐         GET /api/v1/sensores                   │
│  SensorInterface │ ◄──────────────────────────────────────────────┘
│  (WPF Desktop)   │
│                  │  Persiste em:
│  interface_log.db│  interface_log.db (local)
└─────────────────┘
```

---

## Sinal Industrial Adicionado

### Vibração (m/s²)

**Justificativa técnica:** O monitoramento de vibração é essencial em ambientes industriais com máquinas rotativas (motores elétricos, bombas centrífugas, compressores, ventiladores). A análise de vibração permite:

- **Detecção precoce de falhas** em rolamentos e engrenagens
- **Identificação de desbalanceamento** em eixos rotativos
- **Prevenção de paradas não programadas** — manutenção preditiva em vez de corretiva
- **Conformidade com normas** como ISO 10816 (limites de vibração em máquinas)

| Faixa | Condição |
|---|---|
| 0 – 2.8 m/s² | Normal |
| 2.8 – 7.1 m/s² | Atenção |
| 7.1 – 18 m/s² | Alerta |
| > 18 m/s² | Crítico |

O simulador gera valores entre **0.1 e 45.1 m/s²**. O limite configurável padrão é **50 m/s²**.

---

## Configuração e Execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022+ ou VS Code com extensão C#

### Passo a passo

**1. Clone o repositório**
```bash
git clone https://github.com/0001081973-lgtm/ApiSensorIOT
cd SIMI_SistemaIntegradoDeMonitoramentoIndustrial
```

**2. Inicie a API** (cria o banco SQLite automaticamente na primeira execução)
```bash
cd ApiProcessamento
dotnet run
# Swagger disponível em: http://localhost:58201
```

**3. Inicie o Simulador** (em outro terminal)
```bash
cd SensorSimulator
dotnet run
```

**4. Inicie a Interface** (Windows)
```bash
cd SensorInterface
dotnet run
```

### Configurações (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=simi_sensores.db"
  },
  "ApiConfig": {
    "MaxTemperatura": 80,
    "MaxPressao": 10,
    "MaxVibracao": 50
  }
}
```

---


### Modelo de Dados — `SensorData`

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | `int` | Sim | Identificador único (gerado pelo banco) |
| `temperatura` | `double` | Sim | Temperatura em °C |
| `pressao` | `double` | Sim | Pressão em bar |
| `umidade` | `double` | Sim | Umidade relativa em % |
| `vibracao` | `double` | Sim | Vibração em m/s² |
| `origem` | `string` | Sim | Origem do dado (`"Simulator"` ou `"Interface"`) |
| `timestamp` | `datetime` | Sim | Data/hora UTC da coleta |

**Exemplo de objeto:**
```json
{
  "id": 42,
  "temperatura": 65.30,
  "pressao": 4.75,
  "umidade": 58.10,
  "vibracao": 3.22,
  "origem": "Simulator",
  "timestamp": "2026-04-24T14:30:00Z"
}
```

---

### Endpoints

---

#### `POST /api/v1/sensores`

Recebe e persiste um novo registro de sensor.

**Request Body:**
```json
{
  "temperatura": 65.30,
  "pressao": 4.75,
  "umidade": 58.10,
  "vibracao": 3.22,
  "origem": "Simulator"
}
```

**Respostas:**

| Código | Descrição |
|---|---|
| `201 Created` | Registro criado. Retorna o objeto com ID atribuído. |
| `400 Bad Request` | Dado fora dos limites configurados. |

**Exemplo de resposta `201`:**
```json
{
  "id": 1,
  "temperatura": 65.30,
  "pressao": 4.75,
  "umidade": 58.10,
  "vibracao": 3.22,
  "origem": "Simulator",
  "timestamp": "2026-04-24T14:30:00Z"
}
```

**Exemplo de resposta `400`:**
```
"Temperatura 92.5°C acima do limite de 80°C."
```

---

#### `GET /api/v1/sensores`

Retorna todos os registros de sensores, ordenados do mais recente para o mais antigo.

**Parâmetros:** nenhum

**Respostas:**

| Código | Descrição |
|---|---|
| `200 OK` | Lista de registros (pode ser vazia `[]`). |

**Exemplo de resposta `200`:**
```json
[
  {
    "id": 2,
    "temperatura": 70.10,
    "pressao": 5.00,
    "umidade": 60.00,
    "vibracao": 4.10,
    "origem": "Simulator",
    "timestamp": "2026-04-24T14:32:00Z"
  },
  {
    "id": 1,
    "temperatura": 65.30,
    "pressao": 4.75,
    "umidade": 58.10,
    "vibracao": 3.22,
    "origem": "Simulator",
    "timestamp": "2026-04-24T14:30:00Z"
  }
]
```

---

#### `GET /api/v1/sensores/{id}`

Retorna um registro específico pelo seu ID.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `int` | ID do registro |

**Respostas:**

| Código | Descrição |
|---|---|
| `200 OK` | Registro encontrado. |
| `404 Not Found` | Registro não existe. |

**Exemplo:**
```
GET /api/v1/sensores/1
```

---

#### `GET /api/v1/sensores/por-origem/{origem}`

Retorna registros filtrados pela origem dos dados.

**Parâmetros de rota:**

| Parâmetro | Tipo | Valores | Descrição |
|---|---|---|---|
| `origem` | `string` | `Simulator`, `Interface` | Filtro de origem (case-insensitive) |

**Respostas:**

| Código | Descrição |
|---|---|
| `200 OK` | Lista filtrada (pode ser vazia). |

**Exemplo:**
```
GET /api/v1/sensores/por-origem/Simulator
```

---

#### `GET /api/v1/sensores/ultimo`

Retorna o registro mais recente recebido pela API.

**Parâmetros:** nenhum

**Respostas:**

| Código | Descrição |
|---|---|
| `200 OK` | Registro mais recente. |
| `404 Not Found` | Nenhum dado registrado ainda. |

**Exemplo:**
```
GET /api/v1/sensores/ultimo
```

---

#### `DELETE /api/v1/sensores/{id}`

Remove um registro pelo ID.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `int` | ID do registro a remover |

**Respostas:**

| Código | Descrição |
|---|---|
| `204 No Content` | Registro removido com sucesso. |
| `404 Not Found` | Registro não encontrado. |

---

## Persistência de Dados

### API — `simi_sensores.db`

Gerenciado via **Entity Framework Core 8** com provider SQLite.  
Criado automaticamente ao iniciar a API (`db.Database.EnsureCreated()`).

**Tabela `Sensores`:**

| Coluna | Tipo SQLite | Restrição |
|---|---|---|
| `Id` | `INTEGER` | `PRIMARY KEY AUTOINCREMENT` |
| `Temperatura` | `REAL` | `NOT NULL` |
| `Pressao` | `REAL` | `NOT NULL` |
| `Umidade` | `REAL` | `NOT NULL` |
| `Vibracao` | `REAL` | `NOT NULL` |
| `Origem` | `TEXT` | `NOT NULL, MAX 100` |
| `Timestamp` | `TEXT` | `NOT NULL` |

### SensorSimulator — `simulator_log.db`

Banco SQLite local criado com `Microsoft.Data.Sqlite` (sem ORM).  
Persiste **cada leitura gerada antes de enviá-la à API**, garantindo rastreabilidade mesmo em caso de falha de rede.

### SensorInterface — `interface_log.db`

Banco SQLite local criado com `Microsoft.Data.Sqlite`.  
Persiste **todos os registros recebidos da API** usando `INSERT OR IGNORE` para evitar duplicatas por ID.

---

## Estrutura do Projeto

```
SIMI_SistemaIntegradoDeMonitoramentoIndustrial/
│
├── Shared/                         
│   ├── SensorData.cs               
│   └── Shared.csproj
│
├── ApiProcessamento/              
│   ├── Config/
│   │   └── ApiConfig.cs            
│   ├── Controllers/
│   │   └── SensorController.cs     
│   ├── Data/
│   │   └── SensorDbContext.cs      
│   ├── Program.cs                  
│   ├── appsettings.json
│   └── ApiProcessamento.csproj
│
├── SensorSimulator/                
│   ├── Program.cs                  
│   └── SensorSimulator.csproj
│
├── SensorInterface/                
│   ├── Command/RelayCommand.cs
│   ├── Model/MainViewModel.cs      
│   ├── ViewModels/BaseViewModel.cs
│   ├── Views/MainWindow.xaml      
│   └── SensorInterface.csproj
│
├── .gitignore
├── README.md
└── SIMI_SistemaIntegradoDeMonitoramentoIndustrial.sln
```
