import { expect, test } from '@playwright/test';
import { login } from './session';

test('seleciona a conta na lista e mostra o extrato abaixo', async ({ page }) => {
  await login(page);

  await expect(page.locator('.statement-head h2')).toHaveText('Ana Souza');

  await page.locator('.accounts-table tbody tr').nth(1).click();

  await expect(page.locator('.statement-head h2')).toHaveText('Bruno Lima');
  await expect(page.locator('.statement-head .identifier')).toHaveText('#22222222');
  await expect(page.locator('.statement-head__balance strong')).toContainText('R$');
});

test('filtra o extrato por tipo e mostra o resumo do período', async ({ page }) => {
  await login(page);

  await expect(page.locator('.statement-summary').first()).toContainText('Créditos');
  await expect(page.locator('.statement-summary').first()).toContainText('Resultado do período');

  await page.locator('.statement-filters select').selectOption('DEBIT');
  await expect(page.getByText('Nenhum lançamento encontrado')).toBeVisible();

  await page.locator('.statement-filters select').selectOption('CREDIT');
  await expect(page.locator('.statement-table tbody tr').first()).toBeVisible();
});

test('mostra a aba de informações com dados reais da conta', async ({ page }) => {
  await login(page);

  await page.getByRole('tab', { name: 'Informações' }).click();

  const info = page.locator('.info-grid');
  await expect(info.getByText('Titular')).toBeVisible();
  await expect(info.getByText('Ana Souza')).toBeVisible();
  await expect(info.getByText('#11111111')).toBeVisible();
  await expect(info.getByText('Criada em')).toBeVisible();
});
