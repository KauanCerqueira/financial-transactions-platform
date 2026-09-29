import { Pipe, PipeTransform } from '@angular/core';

const formatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' });

@Pipe({ name: 'dateTimeBr' })
export class DateTimeBrPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return value ? formatter.format(new Date(value)) : '';
  }
}
