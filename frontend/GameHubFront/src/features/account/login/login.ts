import { Component, inject, output } from '@angular/core';
import { RegisterCreds } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';
import { FormsModule } from '@angular/forms';
import { Router } from "@angular/router";
import { NavState } from '../../../types/NavState';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private accountService = inject(AccountService);
  router = inject(Router);
  protected creds = {} as RegisterCreds;
  stateChange = output<NavState>();

  login() {
    this.accountService.login(this.creds).subscribe({
      next: response => {
        console.log(response);
        this.router.navigate(['/home']);
      },
      error: error => console.log(error)
    })
  }

  close() {
    this.stateChange.emit('home');
  }

  showRegister() {
    this.stateChange.emit('register');
  }
}
