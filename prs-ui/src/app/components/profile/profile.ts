import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; // <--- Added ChangeDetectorRef
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PersonnelService } from '../../services/personnel.service';
import { MasterDataService } from '../../services/master-data.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule], 
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  
  guid: string = '';
  profileData: any = null; 
  isLoading: boolean = true; // <--- Strict loading flag

  // Master Data
  employmentStatuses: any[] = [];
  grades: any[] = [];
  linesOfService: any[] = [];
  officeLocations: any[] = [];

  // Form Selections
  selectedStatusId: number | null = null;
  selectedGradeId: number | null = null;
  selectedLosId: number | null = null;
  selectedOfficeId: number | null = null;
  portfolioRequired: string = 'No';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private personnelService: PersonnelService,
    private masterDataService: MasterDataService,
    private cdr: ChangeDetectorRef // <--- Inject the UI updater
  ) {}

  ngOnInit() {
    this.guid = this.route.snapshot.paramMap.get('guid') || '';
    if (this.guid) {
      this.loadProfileData();
    }
  }

  loadProfileData() {
    this.isLoading = true; // Start loading spinner
    
    this.personnelService.getProfileByGuid(this.guid).subscribe({
      next: (data) => {
        this.profileData = data;
        this.loadMasterData(); 
        
        // Turn off spinner and FORCE Angular to draw the screen
        this.isLoading = false;
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        alert('Failed to load profile data.');
      }
    });
  }

  loadMasterData() {
    this.masterDataService.getEmploymentStatuses().subscribe(res => {
      this.employmentStatuses = res;
      this.cdr.detectChanges(); // Update dropdown
    });
    this.masterDataService.getGrades().subscribe(res => {
      this.grades = res;
      this.cdr.detectChanges();
    });
    this.masterDataService.getLineOfServices().subscribe(res => {
      this.linesOfService = res;
      this.cdr.detectChanges();
    });

    if (this.profileData && this.profileData.guid_Country) {
      this.masterDataService.getOfficeLocations(this.profileData.guid_Country).subscribe(res => {
        this.officeLocations = res;
        this.cdr.detectChanges();
      });
    }
  }

  onSave() {
    if (!this.selectedStatusId || !this.selectedGradeId || !this.selectedLosId || !this.selectedOfficeId) {
      alert('Please complete all dropdown selections before saving.');
      return;
    }

    // Build the payload
    const payload = {
      Personnel_Guid_ID: this.profileData.unique_ID, 
      Work_Office_Location_ID: Number(this.selectedOfficeId),
      Grade_ID: Number(this.selectedGradeId),
      Line_Of_Service_ID: Number(this.selectedLosId),
      Employment_Status_ID: Number(this.selectedStatusId),
      // THE FIX: Translate "Yes" -> "Y", and "No" -> "N"
      Portfolio_Required: this.portfolioRequired === 'Yes' ? 'Y' : 'N' 
    };

    this.personnelService.completeProfile(payload).subscribe({
      next: (response) => {
        alert(`Profile Completed!\nGenerated Pseudo Party ID: ${response.pseudoPartyId}`);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        // THE NEW ERROR CATCHER: This will show us the REAL error if it fails again
        console.error("Full Error Object:", err);
        const serverError = err.error?.title || err.error || err.message || "Unknown error";
        
        // Check if it's a validation error object (ASP.NET 400 errors)
        if (err.error?.errors) {
            alert(`Validation Error:\n${JSON.stringify(err.error.errors, null, 2)}`);
        } else {
            alert(`Server Rejected Save:\n${typeof serverError === 'string' ? serverError : JSON.stringify(serverError)}`);
        }
      }
    });
  }
}