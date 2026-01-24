import { Component, inject, input, output, signal } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { Home } from "../features/home/home";
import { AccountService } from '../core/services/account-service';

@Component({
  selector: 'app-root',
  imports: [Nav, Home],
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
    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);
  }
}