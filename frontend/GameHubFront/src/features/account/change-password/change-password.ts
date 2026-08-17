import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../../../core/services/account-service';
import { extractErrorMessage } from '../../../core/utils/http-error';

const MIN_PASSWORD_LENGTH = 12;

@Component({
  selector: 'app-change-password',
  imports: [FormsModule],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css',
})
export class ChangePassword {
  back = output<void>();

  private accountService = inject(AccountService);

  currentPassword = '';
  newPassword = '';
  confirmNewPassword = '';

  errorMessage = signal<string | null>(null);
  submitting = signal(false);

  dirty(): boolean {
    return !!(this.currentPassword || this.newPassword || this.confirmNewPassword);
  }

  onBackClick() {
    if (!this.dirty()) {
      this.back.emit();
      return;
    }

    this.errorMessage.set(null);

    if (this.newPassword.length < MIN_PASSWORD_LENGTH) {
      this.errorMessage.set(`A nova senha deve ter pelo menos ${MIN_PASSWORD_LENGTH} caracteres.`);
      return;
    }

    if (this.newPassword !== this.confirmNewPassword) {
      this.errorMessage.set('As senhas não coincidem.');
      return;
    }

    this.submitting.set(true);

    this.accountService
      .changePassword({
        currentPassword: this.currentPassword,
        newPassword: this.newPassword,
        confirmNewPassword: this.confirmNewPassword,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.currentPassword = '';
          this.newPassword = '';
          this.confirmNewPassword = '';
          this.back.emit();
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          this.errorMessage.set(extractErrorMessage(error, 'Não foi possível alterar a senha.'));
        },
      });
  }
}
