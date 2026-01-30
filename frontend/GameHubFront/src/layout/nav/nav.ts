import { Component, inject, input } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterLink } from "@angular/router";

@Component({
  selector: 'app-nav',
  imports: [RouterLink],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  router = inject(Router);
  loggedIn = input<boolean>();

  state() {
    return this.router.url !== '/welcome';
  }

  logout() {
    this.accountService.logout();
  }
}
