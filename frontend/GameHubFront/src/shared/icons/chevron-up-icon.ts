import { Component, input } from '@angular/core';

@Component({
  selector: 'app-icon-chevron-up',
  template: `
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"
      [class]="svgClass()">
      <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 18.75 7.5-7.5 7.5 7.5" />
      <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 7.5-7.5 7.5 7.5" />
    </svg>
  `,
})
export class ChevronUpIcon {
  svgClass = input('size-5 shadow-2xl');
}
