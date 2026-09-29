import { expect, test } from '@playwright/test';
import { login } from './session';

test('faz login e lista as contas com o saldo consolidado', async ({ page }) => {
  await login(page);

  const table = page.locator('.accounts-table');
  await expect(table.getByText('Ana Souza')).toBeVisible();
  await expect(table.getByText('Bruno Lima')).toBeVisible();
  await expect(table.getByText('Carla Mendes')).toBeVisible();
  await expect(page.locator('.summary').getByText('Total em Contas')).toBeVisible();
  await expect(page.locator('.summary').getByText('Total de Transações')).toBeVisible();
});

test('busca por titular filtra a lista de contas', async ({ page }) => {
  await login(page);

  await page.getByRole('searchbox').fill('Bruno');

  await expect(page.locator('.accounts-table tbody tr')).toHaveCount(1);
  await expect(page.locator('.accounts-table').getByText('Bruno Lima')).toBeVisible();
});
