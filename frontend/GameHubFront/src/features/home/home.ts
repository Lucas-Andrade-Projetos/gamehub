import { Component, input, output, signal } from '@angular/core';
import { LeftDisplay } from "../../layout/left-display/left-display";
import { leftView } from '../../LeftView';

@Component({
  selector: 'app-home',
  imports: [LeftDisplay],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  currentView = input<leftView>();
  viewToUpdate = output<leftView>();
}
