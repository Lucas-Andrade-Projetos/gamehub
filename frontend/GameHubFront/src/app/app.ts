import { Component, inject, signal } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { AccountService } from '../core/services/account-service';
import { RouterOutlet } from '@angular/router';
import { isTokenExpired } from '../core/utils/jwt';

@Component({
  selector: 'app-root',
  imports: [Nav, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('GameHubFront');
  private accountService = inject(AccountService)

  async ngOnInit() {
    this.setCurrentUser();
  }

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;

    let user;
    try {
      user = JSON.parse(userString);
    } catch {
      localStorage.removeItem('user');
      return;
    }

    if (!user?.token || isTokenExpired(user.token)) {
      localStorage.removeItem('user');
      return;
    }

    this.accountService.currentUser.set(user);
  }
}