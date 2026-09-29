import { Pipe, PipeTransform } from '@angular/core';

const formatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

@Pipe({ name: 'brl' })
export class CurrencyBrlPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    return formatter.format(value ?? 0);
  }
}
