import { Component, output, signal } from '@angular/core';
import { leftView } from '../../../LeftView';

@Component({
  selector: 'app-login',
  imports: [],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  viewTitle = output<leftView>();

  showRegister() {
    this.viewTitle.emit('register');
  }

  showWelcome() {
    this.viewTitle.emit('welcome');
  }
}
