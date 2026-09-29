import { Pipe, PipeTransform } from '@angular/core';

const formatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'medium' });

@Pipe({ name: 'dateBr' })
export class DateBrPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return value ? formatter.format(new Date(value)) : '';
  }
}
