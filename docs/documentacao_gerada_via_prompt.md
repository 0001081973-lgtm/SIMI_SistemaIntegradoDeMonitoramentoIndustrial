# Documentação Gerada via Prompt — SIMI API

> Este documento foi gerado utilizando engenharia de prompt aplicada a um modelo de linguagem (LLM),
> conforme requisito da atividade. O prompt utilizado e o resultado completo estão descritos abaixo.

---

## Prompt Utilizado

```
Você é um engenheiro de software especialista em APIs REST e documentação técnica.
Analise a seguinte API ASP.NET Core e gere uma documentação completa no formato Markdown,
incluindo: descrição de cada endpoint, parâmetros de entrada, tipos de resposta com exemplos
JSON, códigos HTTP, modelo de dados e casos de uso reais para um sistema industrial.

[código da API colado aqui]
```

---

## Documentação Resultante

### Visão Geral da API

A **SIMI API** (Sistema Integrado de Monitoramento Industrial) é uma Web API RESTful desenvolvida
em ASP.NET Core 8. Sua responsabilidade é centralizar o recebimento, a validação e a persistência
dos dados gerados por sensores industriais distribuídos.

A API aceita dados de múltiplas origens (simulador automatizado e interface manual), valida se os
valores estão dentro dos limites de segurança configurados e armazena tudo em um banco de dados
SQLite, garantindo rastreabilidade e histórico completo das leituras.

---

### Tecnologias e Padrões

| Item | Detalhe |
|---|---|
| Framework | ASP.NET Core 8 |
| Persistência | Entity Framework Core 8 + SQLite |
| Documentação | Swagger / OpenAPI 3.0 (Swashbuckle) |
| Arquitetura | REST · Controller-based · Dependency Injection |
| Serialização | System.Text.Json (padrão .NET 8) |

---

### Análise dos Endpoints

#### 1. Receber Dado de Sensor

**`POST /api/v1/sensores`**

**Finalidade:** Ponto de entrada principal da API. Recebe um pacote de leitura de sensor,
executa validação de limites e persiste o registro no banco se aprovado.

**Lógica de validação aplicada:**
- Se `temperatura > MaxTemperatura` (padrão: 80°C) → rejeita com `400`
- Se `pressao > MaxPressao` (padrão: 10 bar) → rejeita com `400`
- Se `vibracao > MaxVibracao` (padrão: 50 m/s²) → rejeita com `400`
- Se aprovado → persiste e retorna `201 Created` com o objeto salvo (incluindo ID gerado)

**Caso de uso:** O `SensorSimulator` chama este endpoint a cada 2 segundos com uma nova leitura gerada aleatoriamente.

**Corpo da requisição:**
```json
{
  "temperatura": 55.40,
  "pressao": 3.80,
  "umidade": 72.00,
  "vibracao": 1.95,
  "origem": "Simulator"
}
```

**Resposta de sucesso (`201 Created`):**
```json
{
  "id": 15,
  "temperatura": 55.40,
  "pressao": 3.80,
  "umidade": 72.00,
  "vibracao": 1.95,
  "origem": "Simulator",
  "timestamp": "2026-04-24T14:30:00.000Z"
}
```

**Resposta de erro (`400 Bad Request`):**
```
"Temperatura 92.5°C acima do limite de 80°C."
```

---

#### 2. Listar Todos os Registros

**`GET /api/v1/sensores`**

**Finalidade:** Retorna o histórico completo de leituras, ordenadas da mais recente para
a mais antiga. Consumido pela `SensorInterface` (WPF) para exibir os dados em tabela.

**Caso de uso:** A interface WPF chama este endpoint quando o usuário clica em "Carregar Dados".

**Resposta (`200 OK`):**
```json
[
  {
    "id": 15,
    "temperatura": 55.40,
    "pressao": 3.80,
    "umidade": 72.00,
    "vibracao": 1.95,
    "origem": "Simulator",
    "timestamp": "2026-04-24T14:32:00.000Z"
  },
  {
    "id": 14,
    "temperatura": 61.20,
    "pressao": 4.10,
    "umidade": 68.50,
    "vibracao": 2.40,
    "origem": "Simulator",
    "timestamp": "2026-04-24T14:30:00.000Z"
  }
]
```

---

#### 3. Buscar por ID

**`GET /api/v1/sensores/{id}`**

**Finalidade:** Recupera um registro específico. Útil para auditorias, rastreabilidade
e integração com sistemas externos que precisam verificar um dado específico.

**Caso de uso:** Um sistema de BI externo precisa verificar a leitura com ID 42 para
cruzar com um evento de alarme registrado no mesmo horário.

**Exemplo de requisição:**
```
GET /api/v1/sensores/42
```

**Resposta (`200 OK`):**
```json
{
  "id": 42,
  "temperatura": 78.90,
  "pressao": 9.20,
  "umidade": 40.00,
  "vibracao": 12.50,
  "origem": "Simulator",
  "timestamp": "2026-04-24T10:15:00.000Z"
}
```

**Resposta (`404 Not Found`):**
```
"Registro com ID 42 não encontrado."
```

---

#### 4. Filtrar por Origem

**`GET /api/v1/sensores/por-origem/{origem}`**

**Finalidade:** Separa as leituras por fonte de dados. Permite analisar individualmente
os dados gerados pelo simulador versus os inseridos manualmente pela interface, facilitando
comparações e auditoria de qualidade.

**Caso de uso:** Equipe de QA quer verificar apenas os dados inseridos manualmente pela
interface para validar a consistência das entradas humanas.

**Exemplo de requisição:**
```
GET /api/v1/sensores/por-origem/Interface
```

**Resposta (`200 OK`):**
```json
[
  {
    "id": 7,
    "temperatura": 30.00,
    "pressao": 2.00,
    "umidade": 55.00,
    "vibracao": 0.80,
    "origem": "Interface",
    "timestamp": "2026-04-24T09:00:00.000Z"
  }
]
```

---

#### 5. Último Registro

**`GET /api/v1/sensores/ultimo`**

**Finalidade:** Fornece acesso rápido à leitura mais recente sem carregar todo o histórico.
Ideal para dashboards em tempo real e sistemas de alarme que precisam checar o estado atual.

**Caso de uso:** Um painel de monitoramento faz polling a cada 5 segundos neste endpoint
para exibir os valores atuais dos sensores em tempo real.

**Resposta (`200 OK`):**
```json
{
  "id": 120,
  "temperatura": 42.10,
  "pressao": 3.50,
  "umidade": 65.00,
  "vibracao": 2.10,
  "origem": "Simulator",
  "timestamp": "2026-04-24T14:59:58.000Z"
}
```

**Resposta (`404 Not Found`):**
```
"Nenhum dado de sensor registrado."
```

---

#### 6. Remover Registro

**`DELETE /api/v1/sensores/{id}`**

**Finalidade:** Remove permanentemente um registro do banco. Destinado a operações
administrativas, correção de dados inválidos ou conformidade com políticas de retenção.

**Caso de uso:** Um operador identificou que o registro ID 5 foi inserido com valores
incorretos devido a um sensor defeituoso e precisa removê-lo do histórico.

**Exemplo de requisição:**
```
DELETE /api/v1/sensores/5
```

**Resposta (`204 No Content`):** corpo vazio — remoção bem-sucedida.

**Resposta (`404 Not Found`):**
```
"Registro com ID 5 não encontrado."
```

---

### Diagrama de Fluxo de Dados

```
SensorSimulator                     ApiProcessamento                   SensorInterface
      │                                    │                                  │
      │── POST /api/v1/sensores ──────────►│                                  │
      │                                    │── Valida limites                 │
      │                                    │── Persiste no SQLite             │
      │◄── 201 Created ───────────────────│                                  │
      │                                    │                                  │
      │                                    │◄── GET /api/v1/sensores ─────────│
      │                                    │── Consulta SQLite                │
      │                                    │── 200 OK + lista ───────────────►│
      │                                    │                                  │── Persiste local
      │                                    │                                  │── Exibe na UI
```

---

### Resumo dos Códigos HTTP

| Código | Significado | Quando ocorre |
|---|---|---|
| `200 OK` | Sucesso | GET com resultado |
| `201 Created` | Criado | POST bem-sucedido |
| `204 No Content` | Sem conteúdo | DELETE bem-sucedido |
| `400 Bad Request` | Requisição inválida | Dado fora dos limites |
| `404 Not Found` | Não encontrado | ID inexistente ou banco vazio |
