import { Component, computed, inject, input, signal } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { RouterLink } from "@angular/router";
import { NgClass } from '@angular/common';
import { LoginBtn } from "../login-btn/login-btn";
import { Login } from "../../features/account/login/login";
import { Register } from "../../features/account/register/register";
import { ProfileEdit } from "../../features/account/profile-edit/profile-edit";
import { ChangePassword } from "../../features/account/change-password/change-password";
import { NavState } from '../../types/NavState';
import { UserBustIcon } from '../../shared/icons/user-bust-icon';

type ProfileView = 'summary' | 'edit' | 'password';

@Component({
  selector: 'app-nav',
  imports: [NgClass, RouterLink, LoginBtn, Login, Register, ProfileEdit, ChangePassword, UserBustIcon],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  loggedIn = input<boolean>();
  state = signal<NavState>('home');
  profileView = signal<ProfileView>('summary');

  headerClasses = computed(() => {
    if (this.accountService.currentUser()) {
      const expanded = this.profileView() !== 'summary';
      return expanded ? 'max-w-1/3 left-[33%] h-[66vh] overflow-y-auto' : 'max-w-1/3 left-[33%]';
    }

    return this.isStateAuth() ? 'max-w-1/5 left-[15%]' : 'max-w-1/7 left-[18%]';
  });

  isStateAuth() {
    return this.state() === 'login' || this.state() === 'register';
  }

  setState(state: NavState) {
    this.state.set(state);
  }

  logout() {
    this.setState('home')
    this.profileView.set('summary');
    this.accountService.logout();
  }
}
