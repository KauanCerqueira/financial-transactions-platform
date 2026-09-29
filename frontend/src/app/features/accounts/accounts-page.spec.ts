import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideMockStore } from '@ngrx/store/testing';
import { Account } from '../../core/models/account';
import { LoadStatus } from '../../core/models/load-status';
import { AccountsPage } from './accounts-page';

const accounts: Account[] = [
  { id: 'a1', holderName: 'Ana Souza', balance: 1774.2, createdAt: '2026-01-30T10:00:00Z' },
];

describe('AccountsPage', () => {
  function setup(status: LoadStatus, error: string | null = null) {
    TestBed.configureTestingModule({
      imports: [AccountsPage],
      providers: [
        provideRouter([]),
        provideMockStore({
          initialState: {
            accounts: {
              accounts: status === 'loaded' ? accounts : [],
              summary: status === 'loaded' ? { accounts: 1, totalBalance: 1774.2, transactions: 3 } : null,
              status,
              error,
            },
          },
        }),
      ],
    });

    const fixture = TestBed.createComponent(AccountsPage);
    fixture.detectChanges();

    return fixture;
  }

  it('renders the account rows with the balance when loaded', () => {
    const fixture = setup('loaded');
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelectorAll('.table tbody tr').length).toBe(1);
    expect(element.textContent).toContain('Ana Souza');
    expect(element.textContent).toContain('1.774,20');
    expect(element.textContent).toContain('Total de Transações');
    expect(fixture.componentInstance.summary()?.transactions).toBe(3);
  });

  it('shows the error state with a retry action', () => {
    const fixture = setup('error', 'Não foi possível falar com o servidor.');
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.state--error')).toBeTruthy();
    expect(element.textContent).toContain('Não foi possível falar com o servidor.');
  });

  it('shows skeletons while loading', () => {
    const fixture = setup('loading');

    expect(
      (fixture.nativeElement as HTMLElement).querySelectorAll('.skeleton').length,
    ).toBeGreaterThan(0);
  });

  it('FiltrarContas_BuscaPorTitularOuId_PreservaSaldoConsolidado', () => {
    const fixture = setup('loaded');
    fixture.componentRef.setInput('searchText', 'ANA');
    fixture.detectChanges();
    expect(fixture.componentInstance.filteredAccounts()).toEqual(accounts);

    fixture.componentRef.setInput('searchText', 'a1');
    fixture.detectChanges();
    expect(fixture.componentInstance.filteredAccounts()).toEqual(accounts);

    fixture.componentRef.setInput('searchText', 'inexistente');
    fixture.detectChanges();
    expect(fixture.componentInstance.filteredAccounts()).toEqual([]);
    expect(fixture.componentInstance.totalBalance()).toBe(1774.2);
    expect(fixture.nativeElement.textContent).toContain('Nenhuma conta encontrada');
  });
});
