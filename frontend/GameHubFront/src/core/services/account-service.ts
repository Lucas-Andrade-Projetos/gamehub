import { inject, Injectable, signal } from '@angular/core';
import { ChangePasswordCreds, LoginCreds, RegisterCreds, UpdateProfileCreds, User } from '../../types/user';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  router = inject(Router);
  currentUser = signal<User | null>(null);

  baseUrl = 'https://localhost:5001/api/';

  register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds);
  }

  login(creds: LoginCreds) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds).pipe(
      tap(user => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }

  updateProfile(payload: UpdateProfileCreds) {
    return this.http.put<User>(this.baseUrl + 'account/profile', payload).pipe(
      tap(user => this.setCurrentUser(user))
    );
  }

  changePassword(payload: ChangePasswordCreds) {
    return this.http.put<void>(this.baseUrl + 'account/password', payload);
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user))
    this.currentUser.set(user)
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.router.navigate(['/home']);
  }
}
