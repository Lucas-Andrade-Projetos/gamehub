import { Component, inject, input, signal } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterOutlet } from "@angular/router";
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-nav',
  imports: [NgClass, RouterOutlet],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  router = inject(Router);
  loggedIn = input<boolean>();

  isRouteAuth() {
    return this.router.url === '/login' || this.router.url === '/register';
  }

  logout() {
    this.accountService.logout();
  }
}
