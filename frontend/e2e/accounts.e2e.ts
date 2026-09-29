import { expect, test } from '@playwright/test';
import { demoAccountNames } from './api';
import { login } from './session';

test('faz login e lista as contas com o saldo consolidado', async ({ page }) => {
  await login(page);

  const table = page.locator('.accounts-table');
  for (const holderName of demoAccountNames) {
    await expect(table.getByText(holderName)).toBeVisible();
  }

  await expect(page.locator('.summary').getByText('Total em Contas')).toBeVisible();
  await expect(page.locator('.summary').getByText('Total de Transações')).toBeVisible();
});

test('busca por titular filtra a lista de contas', async ({ page }) => {
  await login(page);

  await page.getByRole('searchbox').fill('Marcos');

  await expect(page.locator('.accounts-table tbody tr')).toHaveCount(1);
  await expect(page.locator('.accounts-table').getByText('Marcos Tavares')).toBeVisible();
});
