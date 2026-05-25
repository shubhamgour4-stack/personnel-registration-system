import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; // <-- 1. Import Forms Tool
import { AuthService } from '../../services/auth'; // <-- 2. Import your Bridge

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, FormsModule], // <-- 3. Activate Forms
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  // Variables to hold what the user types
  email = '';
  password = '';

  constructor(private authService: AuthService, private router: Router) {}

  onLogin() {
    const credentials = { email: this.email, password: this.password };
    
    // Call the .NET API
    this.authService.login(credentials).subscribe({
      next: (response: any) => {
        // If successful, save the token and go to the dashboard!
        localStorage.setItem('jwt_token', response.token);
        alert('Login Successful!');
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        alert('Login Failed: Please check your email and password.');
        console.error(err);
      }
    });
  }
}