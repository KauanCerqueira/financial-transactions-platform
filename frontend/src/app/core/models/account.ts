export interface Account {
  id: string;
  holderName: string;
  balance: number;
  createdAt: string;
}

export interface CreateAccountRequest {
  holderName: string;
  initialBalance: number;
}
