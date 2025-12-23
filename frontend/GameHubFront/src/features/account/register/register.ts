import { Component, output } from '@angular/core';
import { leftView } from '../../../LeftView';

@Component({
  selector: 'app-register',
  imports: [],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {

  viewTitle = output<leftView>();

  showLogin() {
    this.viewTitle.emit('login');
  }

  showWelcome() {
    this.viewTitle.emit('welcome');
  }
}