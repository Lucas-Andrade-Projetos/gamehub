import { Component, output } from '@angular/core';
import { NavState } from '../../types/NavState';

@Component({
  selector: 'app-login-btn',
  imports: [],
  templateUrl: './login-btn.html',
  styleUrl: './login-btn.css',
})
export class LoginBtn {
  stateChange = output<NavState>();
  
  showLogin() {
    this.stateChange.emit('login');
  }
}
