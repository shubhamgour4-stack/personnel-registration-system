import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // <--- Required for reading inputs
import { PersonnelService } from '../../services/personnel.service'; // <--- Our new service

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule], // <--- Added FormsModule
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  
  // Variables to hold what the user types
  searchFirstName = '';
  searchLastName = '';
  searchEmail = '';
  searchStatus = 'All';

  // Variable to hold the results from the database
  searchResults: any[] = [];
  hasSearched = false;

  constructor(private router: Router, private personnelService: PersonnelService) {}

  performSearch() {
    this.personnelService.searchProfiles(
      this.searchFirstName, 
      this.searchLastName, 
      this.searchEmail, 
      this.searchStatus
    ).subscribe({
      next: (data) => {
        this.searchResults = data; // Save the DB results
        this.hasSearched = true;   // Show the table
      },
      error: (err) => {
        console.error('Search failed', err);
        alert('Search failed. Are you logged in?');
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
    localStorage.removeItem('jwt_token');
    this.router.navigate(['/login']);
  }
}