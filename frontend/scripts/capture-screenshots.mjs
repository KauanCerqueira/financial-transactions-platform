import { chromium } from '@playwright/test';
import { mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const base = 'http://localhost:4200';
const apiBase = `${base}/api`;
const tokenUrl = 'http://localhost:8081/realms/fraga/protocol/openid-connect/token';
const demoHolder = 'Helena Ribeiro';
const dayInMilliseconds = 86_400_000;
const outputDir = fileURLToPath(new URL('../../docs/prints/', import.meta.url));

// Mesmos dados usados pelo e2e (frontend/e2e/api.ts): o primeiro lançamento é a abertura
// da conta, com a data mais antiga, para o saldo após cada lançamento seguir o extrato.
const demoAccounts = [
  {
    holderName: demoHolder,
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

mkdirSync(outputDir, { recursive: true });

async function accessToken() {
  const response = await fetch(tokenUrl, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      grant_type: 'password',
      client_id: 'financial-app',
      username: 'operador',
      password: 'operador123',
      scope: 'openid',
    }),
  });

  if (!response.ok) {
    throw new Error(`Não foi possível autenticar no Keycloak (HTTP ${response.status}).`);
  }

  return (await response.json()).access_token;
}

// A aplicação sobe vazia: quando não há contas, os prints criam o cenário de demonstração.
async function ensureDemoData() {
  const headers = { Authorization: `Bearer ${await accessToken()}`, 'Content-Type': 'application/json' };
  const accounts = await (await fetch(`${apiBase}/accounts`, { headers })).json();

  if (accounts.length > 0) {
    console.log('banco já tem contas: usando os dados existentes');
    return;
  }

  const now = Date.now();

  for (const account of demoAccounts) {
    const created = await (
      await fetch(`${apiBase}/accounts`, {
        method: 'POST',
        headers,
        body: JSON.stringify({
          holderName: account.holderName,
          initialBalance: account.initialBalance,
        }),
      })
    ).json();

    for (const movement of account.movements) {
      await fetch(`${apiBase}/transactions`, {
        method: 'POST',
        headers,
        body: JSON.stringify({
          eventId: crypto.randomUUID(),
          accountId: created.id,
          type: movement.type,
          amount: movement.amount,
          occurredAt: new Date(now - movement.daysAgo * dayInMilliseconds).toISOString(),
        }),
      });
    }
  }

  console.log(`cenário de demonstração criado (${demoAccounts.length} contas)`);
}

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, locale: 'pt-BR' });
const page = await context.newPage();

// primeira subida (Keycloak + API frios) pode passar dos 30s padrão
page.setDefaultTimeout(90_000);
page.setDefaultNavigationTimeout(90_000);

async function capture(name, target = null) {
  await page.waitForTimeout(700);

  if (target) {
    await target.screenshot({ path: `${outputDir}${name}.png` });
  } else {
    await page.screenshot({ path: `${outputDir}${name}.png`, fullPage: true });
  }

  console.log(`capturado: ${name}.png`);
}

async function login() {
  await page.goto(`${base}/accounts`);
  await page.waitForURL(/localhost:8081/);
  await page.locator('.brand-scene').waitFor();
  await capture('00-login');
  await page.fill('#username', 'operador');
  await page.fill('#password', 'operador123');
  await page.click('#kc-login');
  await page.getByRole('heading', { name: 'Contas' }).waitFor();
}

await ensureDemoData();
await login();

const demoRow = page.locator('.accounts-table tbody tr', { hasText: demoHolder });
await demoRow.waitFor();
await capture('01-contas');

await demoRow.click();
await page.locator('.statement-table tbody tr').first().waitFor();
await capture('02-extrato', page.locator('.statement-card'));

await page.getByRole('tab', { name: 'Informações' }).click();
await capture('03-informacoes', page.locator('.statement-card'));

await page.locator('a[href="/transactions/new"]').click();
await page.locator('.transaction-panel form').waitFor();
await capture('04-nova-transacao');

const form = page.locator('.transaction-panel form');
const demoAccountId = await form
  .locator('select#accountId option', { hasText: demoHolder })
  .first()
  .getAttribute('value');

await form.locator('select#accountId').selectOption(demoAccountId);
await form.locator('input#amount').fill('99.90');
await form.locator('button[type="submit"]').click();
await page.locator('.toast--success').waitFor();
await page.screenshot({ path: `${outputDir}05-lancamento-sucesso.png`, fullPage: true });
console.log('capturado: 05-lancamento-sucesso.png');

await page.getByRole('button', { name: /Lançar outra/ }).click();
await form.locator('select#accountId').selectOption(demoAccountId);
await form.locator('.segmented button[data-type="DEBIT"]').click();
await form.locator('input#amount').fill('999999');
await form.locator('button[type="submit"]').click();
await page.locator('.toast--error').waitFor();
await page.screenshot({ path: `${outputDir}06-saldo-insuficiente.png`, fullPage: true });
console.log('capturado: 06-saldo-insuficiente.png');

await page.goto(`${base}/accounts/new`);
await page.locator('.transaction-panel form').waitFor();
await capture('07-nova-conta');

await browser.close();
console.log(`\nPrints salvos em ${outputDir}`);
