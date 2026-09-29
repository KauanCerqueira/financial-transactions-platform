import { ShortIdPipe } from './short-id.pipe';

describe('ShortIdPipe', () => {
  const pipe = new ShortIdPipe();

  it('formats a UUID as a short reference', () => {
    expect(pipe.transform('57f1b664-75a9-41f4-a233-72098443a90b')).toBe('#57F1B664');
  });

  it('returns an empty string for missing values', () => {
    expect(pipe.transform(null)).toBe('');
    expect(pipe.transform(undefined)).toBe('');
    expect(pipe.transform('')).toBe('');
  });
});
