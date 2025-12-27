import { inject, Injectable } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../types/user';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);

  baseUrl = 'https://localhost:5001/api/';

  register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds);
  }

  login(creds: LoginCreds) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds);
  }
}
