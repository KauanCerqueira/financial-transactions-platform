import { Page, expect } from '@playwright/test';

export const seededAccountId = '11111111-1111-1111-1111-111111111111';

export async function login(page: Page): Promise<void> {
  await page.goto('/accounts');

  await page.waitForURL(/localhost:8081/);
  await page.fill('#username', 'operador');
  await page.fill('#password', 'operador123');
  await page.click('#kc-login');

  await expect(page.getByRole('heading', { name: 'Contas' })).toBeVisible();
}
