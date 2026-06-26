import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PersonnelService } from '../../services/personnel.service';
import { TokenService } from '../../services/token.service';
import { ErrorNotificationService } from '../../services/error-notification.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  
  searchFirstName = '';
  searchLastName = '';
  searchEmail = '';
  searchStatus = 'All';

  searchResults: any[] = [];
  hasSearched = false;

  constructor(
    private router: Router,
    private personnelService: PersonnelService,
    private tokenService: TokenService,
    private errorNotificationService: ErrorNotificationService
  ) {}

  performSearch() {
    // API call automatically includes JWT token via AuthHttpInterceptor
    // Error handling automatically managed by ErrorInterceptor
    this.personnelService.searchProfiles(
      this.searchFirstName, 
      this.searchLastName, 
      this.searchEmail, 
      this.searchStatus
    ).subscribe({
      next: (data) => {
        this.searchResults = data;
        this.hasSearched = true;
        
        // Show success feedback if search completed
        if (data.length === 0) {
          this.errorNotificationService.showWarning('No profiles found matching your criteria.');
        } else {
          this.errorNotificationService.showSuccess(`Found ${data.length} profile(s).`, 2000);
        }
      },
      error: (err) => {
        // Error is handled by ErrorInterceptor, but component can add additional context
        console.error('Search failed:', err);
        this.errorNotificationService.logError(err, 'Dashboard.performSearch()');
      }
    });
  }

  resetSearch() {
    this.searchFirstName = '';
    this.searchLastName = '';
    this.searchEmail = '';
    this.searchStatus = 'All';
    this.hasSearched = false;
    this.searchResults = [];
  }

  onLogout() {
    // Use TokenService to remove token (single source of truth for token lifecycle)
    // TokenService uses consistent key 'jwt_token'
    this.tokenService.removeToken();
    
    // Show logout confirmation to user
    this.errorNotificationService.showSuccess('Logged out successfully.', 2000);
    
    // Redirect to login page
    this.router.navigate(['/login']);
  }
}