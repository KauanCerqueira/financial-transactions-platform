import { Page, expect } from '@playwright/test';

export async function login(page: Page): Promise<void> {
  await page.goto('/accounts');

  await page.waitForURL(/localhost:8081/);
  await page.fill('#username', 'operador');
  await page.fill('#password', 'operador123');
  await page.click('#kc-login');

  await expect(page.getByRole('heading', { name: 'Contas' })).toBeVisible();
}
