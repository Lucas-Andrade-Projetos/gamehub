import { Component, inject, input } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { RouterLink, RouterLinkActive } from "@angular/router";

@Component({
  selector: 'app-nav',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  loggedIn = input<boolean>();

  logout() {
    this.accountService.logout();
  }
}
