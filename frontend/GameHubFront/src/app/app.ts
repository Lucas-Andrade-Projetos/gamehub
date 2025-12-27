import { Component, inject, input, output, signal } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { Home } from "../features/home/home";
import { leftView } from '../types/LeftView';
import { AccountService } from '../core/account-service';

@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('GameHubFront');
  currentView = signal<leftView>('welcome');
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

  updateView(view: leftView) {
    this.currentView.set(view);
  }
}