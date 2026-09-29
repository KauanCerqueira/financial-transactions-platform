import { chromium } from '@playwright/test';
import { mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const base = 'http://localhost:4200';
const seededAccountId = '11111111-1111-1111-1111-111111111111';
const outputDir = fileURLToPath(new URL('../../docs/prints/', import.meta.url));

mkdirSync(outputDir, { recursive: true });

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, locale: 'pt-BR' });
const page = await context.newPage();

async function capture(name) {
  await page.waitForTimeout(700);
  await page.screenshot({ path: `${outputDir}${name}.png`, fullPage: true });
  console.log(`capturado: ${name}.png`);
}

async function login() {
  await page.goto(`${base}/accounts`);
  await page.waitForURL(/localhost:8081/);
  await page.fill('#username', 'operador');
  await page.fill('#password', 'operador123');
  await page.click('#kc-login');
  await page.getByRole('heading', { name: 'Contas' }).waitFor();
}

await login();

await page.locator('.accounts-table tbody tr').first().waitFor();
await capture('01-contas');

await page.getByRole('link', { name: /Ver extrato/ }).first().click();
await page.locator('.statement-table tbody tr').first().waitFor();
await capture('02-extrato');

await page.getByRole('tab', { name: 'Informações' }).click();
await capture('03-informacoes');

await page.locator('a[href="/transactions/new"]').click();
await page.locator('.transaction-panel form').waitFor();
await capture('04-nova-transacao');

const form = page.locator('.transaction-panel form');
await form.locator('select#accountId').selectOption(seededAccountId);
await form.locator('input#amount').fill('99.90');
await form.locator('button[type="submit"]').click();
await page.locator('.toast--success').waitFor();
await page.screenshot({ path: `${outputDir}05-lancamento-sucesso.png`, fullPage: true });
console.log('capturado: 05-lancamento-sucesso.png');

await page.getByRole('button', { name: /Lançar outra/ }).click();
await form.locator('select#accountId').selectOption(seededAccountId);
await form.locator('.segmented button[data-type="DEBIT"]').click();
await form.locator('input#amount').fill('999999');
await form.locator('button[type="submit"]').click();
await page.locator('.toast--error').waitFor();
await page.screenshot({ path: `${outputDir}06-saldo-insuficiente.png`, fullPage: true });
console.log('capturado: 06-saldo-insuficiente.png');

await browser.close();
console.log(`\nPrints salvos em ${outputDir}`);
