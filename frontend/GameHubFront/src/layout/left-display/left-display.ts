import { Component, input, output, signal } from '@angular/core';
import { Register } from '../../features/account/register/register';
import { Login } from '../../features/account/login/login';
import { WelcomingText } from '../welcoming-text/welcoming-text';
import { leftView } from '../../LeftView';

@Component({
  selector: 'app-left-display',
  imports: [Register, Login, WelcomingText],
  templateUrl: './left-display.html',
  styleUrl: './left-display.css',
})
export class LeftDisplay {
  currentView = input<leftView>();
  viewToUpdate = output<leftView>();
}