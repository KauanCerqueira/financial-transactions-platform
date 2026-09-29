import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'accounts' },
  {
    path: 'accounts',
    title: 'Contas · FRAGA Financeiro',
    loadComponent: () => import('./features/accounts/accounts-page').then((module) => module.AccountsPage),
  },
  {
    path: 'accounts/:accountId/statement',
    title: 'Extrato · FRAGA Financeiro',
    loadComponent: () => import('./features/statement/statement-page').then((module) => module.StatementPage),
  },
  {
    path: 'transactions/new',
    title: 'Lançar transação · FRAGA Financeiro',
    loadComponent: () =>
      import('./features/transactions/new-transaction-page').then((module) => module.NewTransactionPage),
  },
  { path: '**', redirectTo: 'accounts' },
];
