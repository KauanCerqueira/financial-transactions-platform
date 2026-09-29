import { CurrencyBrlPipe } from './currency-brl.pipe';

describe('CurrencyBrlPipe', () => {
  const pipe = new CurrencyBrlPipe();

  it('formats a value as Brazilian currency', () => {
    expect(pipe.transform(1234.5)).toContain('1.234,50');
  });

  it('treats null and undefined as zero', () => {
    expect(pipe.transform(null)).toContain('0,00');
    expect(pipe.transform(undefined)).toContain('0,00');
  });
});
