import { Component, input } from '@angular/core';

@Component({
  selector: 'app-statement-page',
  templateUrl: './statement-page.html',
  styleUrl: './statement-page.scss',
})
export class StatementPage {
  readonly accountId = input.required<string>();
}
