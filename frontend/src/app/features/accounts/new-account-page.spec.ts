import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { Account } from '../../core/models/account';
import { NewAccountPage } from './new-account-page';
import { AccountsState } from './state/accounts.reducer';

const created: Account = {
  id: 'a9',
  holderName: 'Maria Silva',
  balance: 900,
  createdAt: '2026-01-30T11:00:00Z',
};

function stateWith(overrides: Partial<AccountsState> = {}): AccountsState {
  return {
    accounts: [],
    summary: null,
    selectedAccountId: null,
    status: 'loaded',
    error: null,
    createStatus: 'idle',
    createError: null,
    createdAccount: null,
    ...overrides,
  };
}

describe('NewAccountPage', () => {
  function setup(overrides: Partial<AccountsState> = {}) {
    TestBed.configureTestingModule({
      imports: [NewAccountPage],
      providers: [
        provideRouter([]),
        provideMockStore({ initialState: { accounts: stateWith(overrides) } }),
      ],
    });

    const store = TestBed.inject(MockStore);
    const dispatch = spyOn(store, 'dispatch');
    const fixture = TestBed.createComponent(NewAccountPage);
    fixture.detectChanges();

    return { fixture, component: fixture.componentInstance, dispatch };
  }

  it('does not dispatch when the holder is empty', () => {
    const { component, dispatch } = setup();
    dispatch.calls.reset();

    component.submit();

    expect(dispatch).not.toHaveBeenCalled();
    expect(component.form.controls.holderName.touched).toBe(true);
  });

  it('does not dispatch when the holder has only spaces', () => {
    const { component, dispatch } = setup();
    component.form.patchValue({ holderName: '   ' });
    dispatch.calls.reset();

    component.submit();

    expect(dispatch).not.toHaveBeenCalled();
    expect(component.form.controls.holderName.invalid).toBe(true);
  });

  it('does not dispatch when the initial balance is negative', () => {
    const { component, dispatch } = setup();
    component.form.patchValue({ holderName: 'Maria Silva', initialBalance: -5 });
    dispatch.calls.reset();

    component.submit();

    expect(dispatch).not.toHaveBeenCalled();
    expect(component.form.controls.initialBalance.invalid).toBe(true);
  });

  it('dispatches the creation with the trimmed holder and the initial balance', () => {
    const { component, dispatch } = setup();
    component.form.patchValue({ holderName: '  Maria Silva  ', initialBalance: 900 });
    dispatch.calls.reset();

    component.submit();

    expect(dispatch).toHaveBeenCalledWith(
      jasmine.objectContaining({
        type: '[Accounts] Create',
        holderName: 'Maria Silva',
        initialBalance: 900,
      }),
    );
  });

  it('treats an empty initial balance as zero', () => {
    const { component, dispatch } = setup();
    component.form.patchValue({ holderName: 'Maria Silva', initialBalance: null as unknown as number });
    dispatch.calls.reset();

    component.submit();

    expect(dispatch).toHaveBeenCalledWith(jasmine.objectContaining({ initialBalance: 0 }));
  });

  it('shows the created account instead of the form', () => {
    const { fixture } = setup({ createStatus: 'created', createdAccount: created });
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.result')).toBeTruthy();
    expect(element.textContent).toContain('Conta criada');
    expect(element.textContent).toContain('Maria Silva');
    expect(element.textContent).toContain('900,00');
    expect(element.querySelector('form')).toBeNull();
  });

  it('resets the form and the state to create another account', () => {
    const { fixture, component, dispatch } = setup({ createStatus: 'created', createdAccount: created });
    fixture.componentInstance.form.patchValue({ holderName: 'Maria Silva', initialBalance: 900 });
    dispatch.calls.reset();

    component.createAnother();

    expect(dispatch).toHaveBeenCalledWith(jasmine.objectContaining({ type: '[Accounts] Reset create' }));
    expect(component.form.controls.holderName.value).toBe('');
    expect(component.form.controls.initialBalance.value).toBe(0);
  });

  it('renders the creation error coming from the store', () => {
    const { fixture } = setup({
      createStatus: 'error',
      createError: { message: 'O titular é obrigatório.', code: 'HTTP_400' },
    });
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.state--error')).toBeTruthy();
    expect(element.textContent).toContain('O titular é obrigatório.');
  });
});
