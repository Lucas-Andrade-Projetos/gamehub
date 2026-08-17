import { Component, inject, output, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RegisterCreds } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';
import { FormsModule } from '@angular/forms';
import { Router } from "@angular/router";
import { NavState } from '../../../types/NavState';
import { extractErrorMessage } from '../../../core/utils/http-error';
import { ChevronUpIcon } from '../../../shared/icons/chevron-up-icon';

@Component({
  selector: 'app-login',
  imports: [FormsModule, ChevronUpIcon],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private accountService = inject(AccountService);
  router = inject(Router);
  protected creds = {} as RegisterCreds;
  stateChange = output<NavState>();
  errorMessage = signal<string | null>(null);
  submitting = signal(false);

  login() {
    this.errorMessage.set(null);
    this.submitting.set(true);

    this.accountService.login(this.creds).subscribe({
      next: () => this.router.navigate(['/home']),
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.errorMessage.set(extractErrorMessage(error, 'Não foi possível entrar. Tente novamente.'));
      },
    })
  }

  close() {
    this.stateChange.emit('home');
  }

  showRegister() {
    this.stateChange.emit('register');
  }
}
