import { Component, inject, input, signal } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { RouterLink } from "@angular/router";
import { NgClass } from '@angular/common';
import { LoginBtn } from "../login-btn/login-btn";
import { Login } from "../../features/account/login/login";
import { Register } from "../../features/account/register/register";
import { NavState } from '../../types/NavState';

@Component({
  selector: 'app-nav',
  imports: [NgClass, RouterLink, LoginBtn, Login, Register],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  loggedIn = input<boolean>();
  state = signal<NavState>('home');

  isStateAuth() {
    return this.state() === 'login' || this.state() === 'register';
  }

  setState(state: NavState) {
    this.state.set(state);
  }

  logout() {
    this.setState('home')
    this.accountService.logout();
  }
}
