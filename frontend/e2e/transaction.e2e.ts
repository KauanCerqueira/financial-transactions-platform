import { expect, test } from '@playwright/test';
import { login, seededAccountId } from './session';

test('lança um crédito e confirma com o toast de sucesso', async ({ page }) => {
  await login(page);

  await page.locator('a[href="/transactions/new"]').click();

  const form = page.locator('.transaction-panel form');
  await expect(form).toBeVisible();
  await form.locator('select#accountId').selectOption(seededAccountId);
  await form.locator('input#amount').fill('12.34');
  await form.locator('button[type="submit"]').click();

  await expect(page.locator('.toast--success')).toContainText('sucesso');
});

test('rejeita débito acima do saldo com a mensagem de negócio', async ({ page }) => {
  await login(page);

  await page.locator('a[href="/transactions/new"]').click();

  const form = page.locator('.transaction-panel form');
  await expect(form).toBeVisible();
  await form.locator('select#accountId').selectOption(seededAccountId);
  await form.locator('.segmented button[data-type="DEBIT"]').click();
  await form.locator('input#amount').fill('999999');
  await form.locator('button[type="submit"]').click();

  await expect(page.locator('.toast--error')).toContainText('Saldo insuficiente');
});
