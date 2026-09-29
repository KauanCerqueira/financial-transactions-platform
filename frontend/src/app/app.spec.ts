import { of } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        { provide: OidcSecurityService, useValue: { logoff: () => of(null) } },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the brand', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('.brand__logo')?.getAttribute('src')).toBe(
      '/images/fraga-logo-color.png',
    );
    expect(compiled.querySelector('.brand__logo')?.getAttribute('alt')).toContain('FRAGA');
  });
});

