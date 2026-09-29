import { ensureDemoData } from './api';

export default async function globalSetup(): Promise<void> {
  await ensureDemoData();
}
