import { Component, input, output, signal } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { Home } from "../features/home/home";
import { leftView } from '../LeftView';

@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('GameHubFront');

  currentView = signal<leftView>('welcome');

  updateView(view: leftView) {
    this.currentView.set(view);
  }
}