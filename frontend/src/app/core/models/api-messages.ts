const messagesByCode: Record<string, string> = {
  INSUFFICIENT_FUNDS: 'Saldo insuficiente para esta movimentação.',
  DUPLICATE_EVENT: 'Este evento já foi processado.',
  ACCOUNT_NOT_FOUND: 'Conta não encontrada.',
  DOMAIN_ERROR: 'O evento não atende às regras de negócio.',
  NETWORK_ERROR: 'Não foi possível falar com o servidor. Verifique a conexão.',
};

export function messageForCode(code: string | null | undefined, fallback: string): string {
  return (code && messagesByCode[code]) || fallback;
}
