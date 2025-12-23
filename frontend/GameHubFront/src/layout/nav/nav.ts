import { Component, EventEmitter, output, signal } from '@angular/core';
import { leftView } from '../../LeftView';

@Component({
  selector: 'app-nav',
  imports: [],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {

  enterClick = output<leftView>();

  enterClicked() {
    this.enterClick.emit('login');
  }
}
