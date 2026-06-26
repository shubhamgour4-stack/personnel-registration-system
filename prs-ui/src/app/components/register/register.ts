import { Component } from '@angular/core';
import { RouterLink } from '@angular/router'; // <-- 1. Import the router tool

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink], // <-- 2. Tell the component it is allowed to use it
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {

}