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

test('filtra o extrato por tipo e o resumo acompanha o filtro', async ({ page }) => {
  await login(page);

  const summary = page.locator('.statement-summary');
  await expect(summary).toContainText('Créditos');
  await expect(summary).toContainText('Resultado do período');

  await page.locator('.statement-filters select').selectOption('DEBIT');
  await expect(page.locator('.statement-table tbody tr').first()).toContainText('Débito');
  await expect(summary.locator('.statement-summary__item').first()).toContainText('R$ 0,00');

  await page.locator('.statement-filters select').selectOption('CREDIT');
  await expect(page.locator('.statement-table tbody tr').first()).toContainText('Crédito');
  await expect(summary.locator('.statement-summary__item').nth(1)).toContainText('R$ 0,00');
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
