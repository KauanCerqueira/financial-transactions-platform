import { request } from '@playwright/test';

const tokenUrl = 'http://localhost:8081/realms/fraga/protocol/openid-connect/token';
const apiUrl = 'http://localhost:4200/api';
const operator = { username: 'operador', password: 'operador123', clientId: 'financial-app' };
const dayInMilliseconds = 86_400_000;

export interface Account {
  id: string;
  holderName: string;
  balance: number;
  createdAt: string;
}

interface Movement {
  daysAgo: number;
  type: 'CREDIT' | 'DEBIT';
  amount: number;
}

interface DemoAccount {
  holderName: string;
  initialBalance: number;
  movements: Movement[];
}

// Dados de demonstração usados pelos testes ponta a ponta: o primeiro lançamento é a abertura
// da conta, com a data mais antiga, para o saldo após cada lançamento seguir o extrato.
export const demoAccounts: DemoAccount[] = [
  {
    holderName: 'Helena Ribeiro',
    initialBalance: 0,
    movements: [
      { daysAgo: 30, type: 'CREDIT', amount: 4200 },
      { daysAgo: 25, type: 'DEBIT', amount: 320 },
      { daysAgo: 22, type: 'DEBIT', amount: 89.9 },
      { daysAgo: 20, type: 'CREDIT', amount: 1500 },
      { daysAgo: 17, type: 'DEBIT', amount: 240 },
      { daysAgo: 14, type: 'DEBIT', amount: 75.25 },
      { daysAgo: 12, type: 'CREDIT', amount: 320 },
      { daysAgo: 9, type: 'DEBIT', amount: 410 },
      { daysAgo: 6, type: 'DEBIT', amount: 62.3 },
      { daysAgo: 4, type: 'CREDIT', amount: 900 },
      { daysAgo: 2, type: 'DEBIT', amount: 180 },
      { daysAgo: 1, type: 'DEBIT', amount: 55 },
    ],
  },
  {
    holderName: 'Marcos Tavares',
    initialBalance: 0,
    movements: [
      { daysAgo: 30, type: 'CREDIT', amount: 1800.5 },
      { daysAgo: 18, type: 'CREDIT', amount: 1200 },
      { daysAgo: 10, type: 'DEBIT', amount: 320 },
      { daysAgo: 3, type: 'DEBIT', amount: 199.99 },
    ],
  },
  {
    holderName: 'Sofia Andrade',
    initialBalance: 0,
    movements: [
      { daysAgo: 28, type: 'CREDIT', amount: 2600 },
      { daysAgo: 20, type: 'CREDIT', amount: 450 },
      { daysAgo: 12, type: 'DEBIT', amount: 1200 },
      { daysAgo: 5, type: 'CREDIT', amount: 300 },
    ],
  },
];

export const demoAccountNames = demoAccounts.map((account) => account.holderName);

async function accessToken(): Promise<string> {
  const context = await request.newContext();

  const response = await context.post(tokenUrl, {
    form: {
      grant_type: 'password',
      client_id: operator.clientId,
      username: operator.username,
      password: operator.password,
      scope: 'openid',
    },
  });

  const body = (await response.json()) as { access_token: string };
  await context.dispose();

  return body.access_token;
}

// Os testes provisionam o próprio cenário. É idempotente por titular: rodar de novo não duplica,
// e contas criadas na mão (ou por outro teste) não atrapalham.
export async function ensureDemoData(): Promise<void> {
  const context = await request.newContext();
  const headers = { Authorization: `Bearer ${await accessToken()}` };

  try {
    const accounts = (await (await context.get(`${apiUrl}/accounts`, { headers })).json()) as Account[];
    const now = Date.now();

    for (const account of demoAccounts) {
      if (accounts.some((existing) => existing.holderName === account.holderName)) {
        continue;
      }

      const created = (await (
        await context.post(`${apiUrl}/accounts`, {
          headers,
          data: { holderName: account.holderName, initialBalance: account.initialBalance },
        })
      ).json()) as Account;

      for (const movement of account.movements) {
        await context.post(`${apiUrl}/transactions`, {
          headers,
          data: {
            eventId: crypto.randomUUID(),
            accountId: created.id,
            type: movement.type,
            amount: movement.amount,
            occurredAt: new Date(now - movement.daysAgo * dayInMilliseconds).toISOString(),
          },
        });
      }
    }
  } finally {
    await context.dispose();
  }
}
