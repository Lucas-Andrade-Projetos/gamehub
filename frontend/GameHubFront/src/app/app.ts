import { Component, signal } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { Home } from "../features/home/home";

@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('GameHubFront');
}
