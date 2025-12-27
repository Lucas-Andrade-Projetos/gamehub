import { Component, inject, input, output } from '@angular/core';
import { leftView } from '../../types/LeftView';
import { AccountService } from '../../core/account-service';

@Component({
  selector: 'app-nav',
  imports: [],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  enterClick = output<leftView>();
  loggedIn = input<boolean>();

  enterClicked() {
    this.enterClick.emit('login');
  }

  logout() {
    this.accountService.logout();
  }
}
