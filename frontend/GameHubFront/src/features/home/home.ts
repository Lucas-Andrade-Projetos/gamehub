import { Component } from '@angular/core';
import { WelcomingText } from "../../layout/welcoming-text/welcoming-text";
import { HomeFooter } from "../../layout/home-footer/home-footer";

@Component({
  selector: 'app-home',
  imports: [WelcomingText, HomeFooter],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {

}
