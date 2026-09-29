import { Component, input } from '@angular/core';

export type IconName =
  | 'accounts'
  | 'bank'
  | 'info'
  | 'search'
  | 'user'
  | 'logout'
  | 'close'
  | 'arrow-up'
  | 'arrow-down'
  | 'send'
  | 'card'
  | 'add'
  | 'chevron-right'
  | 'chevron-down'
  | 'refresh'
  | 'check'
  | 'alert'
  | 'history';

@Component({
  selector: 'app-icon',
  template: `
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.8"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      @switch (name()) {
        @case ('bank') {
          <path d="m3 9 9-6 9 6H3Zm2 11h14M3 22h18M6 11v7m6-7v7m6-7v7" />
        }
        @case ('info') {
          <circle cx="12" cy="12" r="9" />
          <path d="M12 11v6m0-10h.01" />
        }
        @case ('search') {
          <circle cx="10.5" cy="10.5" r="6.5" />
          <path d="m16 16 5 5" />
        }
        @case ('user') {
          <circle cx="12" cy="8" r="4" />
          <path d="M4 21v-2a8 8 0 0 1 16 0v2" />
        }
        @case ('logout') {
          <path d="M9 4H4v16h5m5-12 4 4-4 4m-5-4h12" />
        }
        @case ('close') {
          <path d="m6 6 12 12M6 18 18 6" />
        }
        @case ('arrow-up') {
          <path d="M12 21V3m-7 7 7-7 7 7" />
        }
        @case ('arrow-down') {
          <path d="M12 3v18m-7-7 7 7 7-7" />
        }
        @case ('send') {
          <path d="m22 2-7 20-4-9-9-4 20-7ZM11 13 22 2" />
        }
        @case ('card') {
          <rect x="3" y="4" width="18" height="16" rx="2" />
          <path d="M3 10h18M7 15h4" />
        }
        @case ('accounts') {
          <path d="M8 6h12M8 12h12M8 18h12" />
          <path d="M4 6h.01M4 12h.01M4 18h.01" />
        }
        @case ('add') {
          <path d="M12 5v14M5 12h14" />
        }
        @case ('chevron-right') {
          <path d="M9 6l6 6-6 6" />
        }
        @case ('chevron-down') {
          <path d="m6 9 6 6 6-6" />
        }
        @case ('refresh') {
          <path d="M20 12a8 8 0 1 1-2.3-5.7" />
          <path d="M20 4v4h-4" />
        }
        @case ('check') {
          <path d="M5 13l4 4L19 7" />
        }
        @case ('alert') {
          <path d="M12 9v4M12 17h.01" />
          <path d="M10.3 4 1.9 18a2 2 0 0 0 1.7 3h16.8a2 2 0 0 0 1.7-3L13.7 4a2 2 0 0 0-3.4 0Z" />
        }
        @case ('history') {
          <path d="M3 12a9 9 0 1 0 3-6.7" />
          <path d="M3 3v5h5M12 8v4l3 2" />
        }
      }
    </svg>
  `,
})
export class Icon {
  readonly name = input.required<IconName>();
}
