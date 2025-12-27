import { Component, inject, output } from '@angular/core';
import { leftView } from '../../../types/LeftView';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../../core/account-service';
import { RegisterCreds } from '../../../types/user';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private accountService = inject(AccountService);
  protected creds = {} as RegisterCreds;
  viewTitle = output<leftView>();

  showLogin() {
    this.viewTitle.emit('login');
  }

  showWelcome() {
    this.viewTitle.emit('welcome');
  }

  register() {
    this.accountService.register(this.creds).subscribe({
      next: response => {
        console.log(response);
        this.cancel();
      },
      error: error => {
        console.log('Registration failed:', error);
      },
    })
  }

  cancel() {
    this.viewTitle.emit('login');
  }
}