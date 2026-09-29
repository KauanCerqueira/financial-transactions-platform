import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Icon } from './shared/ui/icon/icon';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly router = inject(Router);

  searchAccounts(query: string): void {
    void this.router.navigate(['/accounts'], { queryParams: { q: query || null } });
  }

  logout(): void {
    this.oidcSecurityService.logoff().subscribe();
  }
}
