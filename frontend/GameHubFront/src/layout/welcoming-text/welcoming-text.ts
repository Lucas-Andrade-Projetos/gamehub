import { Component, output } from '@angular/core';

type leftView = 'login' | 'register' | 'welcome';
@Component({
  selector: 'app-welcoming-text',
  imports: [],
  templateUrl: './welcoming-text.html',
  styleUrl: './welcoming-text.css',
})
export class WelcomingText {
  onMouseMove(event: MouseEvent) {
    const element = event.currentTarget as HTMLElement;
    const position = element.getBoundingClientRect();
    const x = event.clientX - position.left;
    const y = event.clientY - position.top;

    const rotateX = ((y - position.height / 2) / position.height) * -20;
    const rotateY = ((x - position.width / 2) / position.width) * 20;

    element.style.transform = `rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
    element.style.textShadow = `${rotateX / 3}px ${rotateY / 3}px 10px rgba(0,0,0,0.6)`;
  }

  onMouseLeave(event: MouseEvent) {
    const element = event.currentTarget as HTMLElement;
    element.style.transform = 'rotateX(0deg) rotateY(0deg)';
  }
}
