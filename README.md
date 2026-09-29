# Plataforma de transações financeiras

Teste técnico fullstack: uma API em .NET 10 recebe créditos e débitos, um worker processa os eventos, e uma aplicação Angular 22 permite consultar contas, extratos e o resultado de cada lançamento. Os dados são fictícios e a aplicação não movimenta dinheiro real.

## Executar a aplicação

Requisitos: Docker Desktop com Docker Compose e as portas `4200`, `8080`, `8081`, `5433`, `5672`, `6379` e `15672` livres.

```bash
docker compose up --build
```

Abra [http://localhost:4200](http://localhost:4200). O login de demonstração é `operador` / `operador123`. A API publica o Swagger em [http://localhost:8080/swagger](http://localhost:8080/swagger). As contas iniciais são Ana Souza, Bruno Lima e Carla Mendes; os saldos de abertura também aparecem no extrato como créditos.

Para encerrar, use `docker compose down`. Para apagar os dados de demonstração e recomeçar, use `docker compose down -v`.

## Arquitetura

| Camada | Responsabilidade |
|---|---|
| `backend/src/FinancialTransactions.Domain` | `Account`, `Transaction`, `Money` e regras de saldo |
| `backend/src/FinancialTransactions.Application` | Casos de uso e interfaces de persistência, cache e fila |
| `backend/src/FinancialTransactions.Infrastructure` | EF Core, PostgreSQL, Redis e RabbitMQ |
| `backend/src/FinancialTransactions.Api` | Contratos HTTP, autenticação, Swagger e tratamento de erros |
| `backend/src/FinancialTransactions.Worker` | Consumo e recuperação de eventos pendentes |
| `frontend` | Angular, NgRx, serviços HTTP e páginas de contas, extrato e lançamento |

As dependências apontam para o domínio. O frontend usa a API como fonte da verdade: o saldo é calculado e persistido no backend. O Nginx da aplicação web encaminha `/api` à API, preservando a mesma origem no navegador. O Keycloak fornece autenticação OIDC para a demonstração.

O fluxo de um lançamento é:

1. A API valida o payload, grava o evento como `PENDING` no PostgreSQL e publica uma mensagem no RabbitMQ. A resposta HTTP é `202 Accepted`; o cliente consulta `GET /api/transactions/{eventId}` até o processamento terminar.
2. O worker bloqueia a conta no PostgreSQL, aplica crédito ou débito no domínio e grava saldo e histórico na mesma transação. O índice único de `eventId` impede lançamento duplicado.
3. O resultado vira `PROCESSED` ou `REJECTED`, com motivo de negócio. O frontend mostra o estado recebido da API e consulta novamente as contas e o extrato quando necessário.

O `eventId` identifica uma tentativa de lançamento. Repetir esse identificador não cria uma segunda movimentação. O formulário conserva o evento na sessão do navegador até haver resultado definitivo; uma falha de rede ou recarga da página retoma o mesmo evento.

## Decisões e limites

- **Processamento assíncrono:** RabbitMQ desacopla o recebimento da atualização do saldo, mas exige expor o estado `PENDING` e consultar o resultado. O worker procura eventos pendentes antigos a cada 15 segundos e os republica quando a gravação ocorreu, mas a publicação falhou. Mensagens repetidas são seguras graças à idempotência no banco.
- **Concorrência:** o débito bloqueia a linha da conta com `FOR UPDATE`; dois processos não podem gastar simultaneamente o mesmo saldo. Os testes de integração exercitam duas instâncias do caso de uso com contextos de banco independentes.
- **Cache:** Redis acelera lista de contas e extratos. O caso de uso invalida a lista e troca a versão do extrato após um lançamento; o banco continua sendo a fonte de verdade.
- **Testes frontend:** o Angular usa o runner Vitest integrado ao build atual. O enunciado cita Jasmine/Karma ou Jest; escolhemos Vitest para manter o projeto alinhado ao Angular 22. Há testes de serviços com `HttpTestingController`, de componentes com `TestBed` e de estado/efeitos NgRx.
- **Identidade visual:** “FRAGA · Financeiro” é uma demonstração não oficial inspirada na paleta da Fraga Inteligência Automotiva. Não há vínculo, dados reais de clientes nem uso do logotipo como ativo.
- **Ambiente:** credenciais e URLs em `docker-compose.yml` são somente para desenvolvimento local. Antes de uso real seriam necessários segredos externos, TLS e configuração de produção do Keycloak.

## API

| Método e rota | Resultado |
|---|---|
| `GET /api/accounts` | Contas e saldos atuais |
| `GET /api/accounts/{accountId}/transactions?page=1&pageSize=10` | Extrato paginado com `balanceAfter` |
| `POST /api/transactions` | Recebe um evento, retorna `PENDING` ou estado conhecido |
| `GET /api/transactions/{eventId}` | Consulta `PENDING`, `PROCESSED` ou `REJECTED` |
| `GET /health` e `GET /health/ready` | Estado da API e do PostgreSQL |

Exemplo de evento:

```json
{
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "accountId": "11111111-1111-1111-1111-111111111111",
  "type": "CREDIT",
  "amount": 150.75,
  "occurredAt": "2026-01-30T10:15:00Z"
}
```

Erros de negócio são retornados como `ProblemDetails` com `code`, por exemplo `INSUFFICIENT_FUNDS` ou `DUPLICATE_EVENT`. No fluxo assíncrono, uma rejeição de saldo também pode aparecer no status do evento como `REJECTED` e `rejectionCode`.

## Testes e desenvolvimento local

Com .NET 10 e Node.js instalados:

```bash
dotnet build backend/FinancialTransactions.slnx
dotnet test backend/FinancialTransactions.slnx
cd frontend
npm ci
npm test -- --watch=false
npm run build
```

Os testes de integração usam Testcontainers e precisam do Docker em execução. O frontend pode ser iniciado fora do Compose com `npm start` na pasta `frontend`; a configuração de desenvolvimento encaminha `/api` para `localhost:8080`. A aplicação completa é iniciada com o Compose acima.

Os testes cobrem regras do domínio, idempotência, saldo insuficiente, recuperação de evento pendente, concorrência no banco, contratos HTTP, validação do formulário e estados do frontend.
