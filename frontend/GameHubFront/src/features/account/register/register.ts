import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../../../core/services/account-service';
import { LoginCreds, RegisterCreds } from '../../../types/user';
import { Router } from "@angular/router";
import { NavState } from '../../../types/NavState';
import { extractErrorMessage } from '../../../core/utils/http-error';
import { ChevronUpIcon } from '../../../shared/icons/chevron-up-icon';

@Component({
  selector: 'app-register',
  imports: [FormsModule, ChevronUpIcon],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private accountService = inject(AccountService);
  router = inject(Router);
  protected creds = {} as RegisterCreds;
  stateChange = output<NavState>();
  errorMessage = signal<string | null>(null);
  submitting = signal(false);

  register() {
    this.errorMessage.set(null);
    this.submitting.set(true);

    this.accountService.register(this.creds).subscribe({
      next: () => {
        this.accountService.login(this.toLoginCreds(this.creds)).subscribe({
          next: () => this.router.navigate(['/home']),
          error: () => {
            this.submitting.set(false);
            this.errorMessage.set('Conta criada, mas não foi possível entrar automaticamente. Tente fazer login.');
          },
        })
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.errorMessage.set(extractErrorMessage(error, 'Não foi possível criar a conta. Tente novamente.'));
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