import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Icon } from './shared/ui/icon/icon';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButton, Icon],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  logout(): void {
    this.oidcSecurityService.logoff().subscribe();
  }
}
