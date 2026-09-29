import { Routes } from '@angular/router';
import { autoLoginPartialRoutesGuard } from 'angular-auth-oidc-client';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'accounts' },
  {
    path: 'accounts',
    title: 'Contas · FRAGA Financeiro',
    canActivate: [autoLoginPartialRoutesGuard],
    loadComponent: () =>
      import('./features/accounts/financial-workspace').then((module) => module.FinancialWorkspace),
  },
  {
    path: 'accounts/new',
    title: 'Nova conta · FRAGA Financeiro',
    canActivate: [autoLoginPartialRoutesGuard],
    loadComponent: () =>
      import('./features/accounts/financial-workspace').then((module) => module.FinancialWorkspace),
  },
  {
    path: 'accounts/:accountId/statement',
    title: 'Extrato · FRAGA Financeiro',
    canActivate: [autoLoginPartialRoutesGuard],
    data: { standalone: true },
    loadComponent: () => import('./features/statement/statement-page').then((module) => module.StatementPage),
  },
  {
    path: 'transactions/new',
    title: 'Lançar transação · FRAGA Financeiro',
    canActivate: [autoLoginPartialRoutesGuard],
    loadComponent: () =>
      import('./features/accounts/financial-workspace').then((module) => module.FinancialWorkspace),
  },
  { path: '**', redirectTo: 'accounts' },
];
