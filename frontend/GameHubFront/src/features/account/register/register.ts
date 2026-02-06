import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../../core/services/account-service';
import { LoginCreds, RegisterCreds } from '../../../types/user';
import { Router } from "@angular/router";
import { NavState } from '../../../types/NavState';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private accountService = inject(AccountService);
  router = inject(Router);
  protected creds = {} as RegisterCreds;
  stateChange = output<NavState>();

  register() {
    this.accountService.register(this.creds).subscribe({
      next: () => {
        this.accountService.login(this.toLoginCreds(this.creds)).subscribe({
          next: () => {
            this.router.navigate(['/home']);
          },
          error: err => {
            console.log('Auto login failed:', err);
          }
        })
      },
      error: error => {
        console.log('Registration failed:', error);
      },
    })
  }
  
  showLogin() {
    this.stateChange.emit('login');
  }

  close() {
    this.stateChange.emit('home');
  }

  private toLoginCreds(registerCreds: RegisterCreds): LoginCreds {
    const loginCreds = {} as LoginCreds;
    loginCreds.email = registerCreds.email;
    loginCreds.password = registerCreds.password;
    return loginCreds;
  }
}