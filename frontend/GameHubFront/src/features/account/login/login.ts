import { Component, inject } from '@angular/core';
import { RegisterCreds } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';
import { FormsModule } from '@angular/forms';
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private accountService = inject(AccountService);
  protected creds = {} as RegisterCreds;

  login() {
    this.accountService.login(this.creds).subscribe({
      next: response => {
        console.log(response);
      },
      error: error => console.log(error)
    })
  }
}
