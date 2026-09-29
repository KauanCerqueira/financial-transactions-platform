import { Pipe, PipeTransform } from '@angular/core';

const ShortLength = 8;

@Pipe({ name: 'shortId' })
export class ShortIdPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) {
      return '';
    }

    const compact = value.replace(/-/g, '').slice(0, ShortLength).toUpperCase();

    return `#${compact}`;
  }
}
