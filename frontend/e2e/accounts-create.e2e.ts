import { expect, test } from '@playwright/test';
import { login } from './session';

test('cria uma conta nova e deixa ela selecionada', async ({ page }) => {
  await login(page);

  const holderName = `Conta Teste ${Date.now()}`;

  await page.getByRole('link', { name: /Nova Conta/ }).click();

  const panel = page.locator('.transaction-panel');
  await expect(panel.getByRole('heading', { name: 'Nova Conta' })).toBeVisible();

  await panel.locator('input#holderName').fill(holderName);
  await panel.locator('input#initialBalance').fill('900');
  await panel.locator('button[type="submit"]').click();

  await expect(page.locator('.toast--success')).toContainText(holderName);
  await expect(panel.getByText('Conta criada')).toBeVisible();
  await expect(panel.locator('.result__row').nth(2)).toContainText('900,00');

  // a conta nova já fica selecionada: o extrato dela aparece logo abaixo
  await expect(page.locator('.statement-head h2')).toHaveText(holderName);
  await expect(page.locator('.accounts-table').getByText(holderName)).toBeVisible();
});

test('avisa quando o titular não foi informado', async ({ page }) => {
  await login(page);

  await page.getByRole('link', { name: /Nova Conta/ }).click();

  const panel = page.locator('.transaction-panel');
  await panel.locator('input#initialBalance').fill('100');
  await panel.locator('button[type="submit"]').click();

  await expect(panel.getByText('Informe o nome do titular.')).toBeVisible();
  await expect(page).toHaveURL(/\/accounts\/new$/);
});
