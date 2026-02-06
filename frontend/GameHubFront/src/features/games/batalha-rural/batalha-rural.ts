import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-batalha-rural',
  imports: [],
  templateUrl: './batalha-rural.html',
  styleUrl: './batalha-rural.css',
})
export class BatalhaRural {
  router = inject(Router)

  returnHome() {
    this.router.navigate(['/home']);
  }
}
