import { expect, test } from '@playwright/test';
import { login, seededAccountId } from './session';

test('abre o extrato em tela própria e filtra por tipo', async ({ page }) => {
  await login(page);

  await page.getByRole('link', { name: /Ver extrato/ }).first().click();

  await expect(page).toHaveURL(/\/accounts\/.+\/statement/);
  await expect(page.getByRole('link', { name: /Voltar para contas/ })).toBeVisible();
  await expect(page.locator('.statement-table tbody tr').first()).toBeVisible();

  await page.locator('.statement-filters select').selectOption('DEBIT');
  await expect(page.getByText('Nenhum lançamento encontrado')).toBeVisible();

  await page.locator('.statement-filters select').selectOption('CREDIT');
  await expect(page.locator('.statement-table tbody tr').first()).toBeVisible();
});

test('mostra a aba de informações com dados reais da conta', async ({ page }) => {
  await login(page);

  await page.getByRole('link', { name: /Ver extrato/ }).first().click();
  await page.getByRole('tab', { name: 'Informações' }).click();

  const info = page.locator('.info-grid');
  await expect(info.getByText('Titular')).toBeVisible();
  await expect(info.getByText('ID da conta')).toBeVisible();
  await expect(info.getByText(seededAccountId)).toBeVisible();
  await expect(info.getByText('Criada em')).toBeVisible();
});
