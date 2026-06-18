import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { TokenService } from '../../services/token.service';
import { ErrorNotificationService } from '../../services/error-notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  email = '';
  password = '';

  constructor(
    private authService: AuthService,
    private tokenService: TokenService,
    private router: Router,
    private errorNotificationService: ErrorNotificationService
  ) {}

  onLogin() {
    const credentials = { email: this.email, password: this.password };
    
    this.authService.login(credentials).subscribe({
      next: (response: any) => {
        // Accept both camelCase and PascalCase token names from the backend
        const jwtToken = response?.token ?? response?.Token;

        if (!jwtToken) {
          this.errorNotificationService.showError('Login succeeded but no token was returned. Please try again.');
          return;
        }

        // Use TokenService to store the token (single source of truth)
        // TokenService uses consistent key 'jwt_token' for all token storage
        this.tokenService.setToken(jwtToken);
        
        // Show success message and navigate to dashboard
        this.errorNotificationService.showSuccess('Login successful!', 2000);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        // Extract error message from backend response or show generic message
        // Backend may return: { message: 'Invalid credentials' } or { error: 'message' }
        const errorMessage = err?.error?.message || 
                             err?.error?.error || 
                             'Login failed. Please check your email and password.';
        
        this.errorNotificationService.showError(errorMessage);
        this.errorNotificationService.logError(err, 'Login.onLogin()');
      }
    });
  }
}