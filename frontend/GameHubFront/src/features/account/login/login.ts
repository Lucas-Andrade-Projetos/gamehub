import { Component, inject, output } from '@angular/core';
import { leftView } from '../../../types/LeftView';
import { RegisterCreds } from '../../../types/user';
import { AccountService } from '../../../core/account-service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private accountService = inject(AccountService);
  viewTitle = output<leftView>();
  protected creds = {} as RegisterCreds;

  login() {
    this.accountService.login(this.creds).subscribe({
      next: response => {
        console.log(response);
        this.cancel();
      },
      error: error => console.log(error)
    })
  }

  cancel() {
    this.viewTitle.emit('welcome');
  }

  showRegister() {
    this.viewTitle.emit('register');
  }
}
