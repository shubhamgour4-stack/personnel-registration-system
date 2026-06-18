import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { environment } from '../../../environments/environment';
import { TokenService } from '../../services/token.service';
import { ErrorNotificationService } from '../../services/error-notification.service';

interface IngestionHistory {
  fileId: number;
  fileName: string;
  countryCode: string;
  fileTimestamp: string;
  recordsReceived: number;
  recordsProcessed: number;
  recordsFailed: number;
  processingStatus: string;
  startTime: string;
  endTime: string;
}

interface StagingLog {
  stagingId: number;
  rowNumber: number;
  guid: string;
  employmentStatus: string;
  workOffice: string;
  lineOfService: string;
  grade: string;
  portfolioRequired: string;
  validationStatus: string;
  validationMessage: string;
  processingStatus: string;
}

@Component({
  selector: 'app-mft-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './mft-management.component.html',
  styleUrls: ['./mft-management.component.css']
})
export class MftManagementComponent implements OnInit {
  public historyRecords: IngestionHistory[] = [];
  public activeStagingRecords: StagingLog[] = [];
  public displayedStagingRecords: StagingLog[] = [];
  
  public selectedFileId: number | null = null;
  public operationFilterStatus: string = 'ALL';
  public textSearchGuid: string = '';
  public systemProcessingFlag: boolean = false;

  private apiBaseUrl: string = `${environment.apiBaseUrl}/mft`;

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private tokenService: TokenService,
    private errorNotificationService: ErrorNotificationService
  ) {}

  ngOnInit(): void {
    this.fetchIngestionHistory();
  }

  /**
   * Fetch MFT file ingestion history from the server
   * 
   * Authorization Flow:
   * 1. Check if token exists via TokenService.hasToken()
   * 2. Make HTTP GET request to /mft/history
   * 3. AuthHttpInterceptor automatically adds Authorization Bearer header
   * 4. ErrorInterceptor handles any error responses (401, 403, 500, etc.)
   * 5. Component displays results or error message
   */
  public fetchIngestionHistory(): void {
    // Check if token exists before making the request
    // This is a safety check; ErrorInterceptor will also handle 401 if token is invalid
    if (!this.tokenService.hasToken()) {
      this.errorNotificationService.showError('Authentication required. Please log in.');
      return;
    }

    this.systemProcessingFlag = true;

    this.http.get<IngestionHistory[]>(`${this.apiBaseUrl}/history`).subscribe({
      next: (data) => {
        this.historyRecords = data;
        this.systemProcessingFlag = false;
        this.cdr.detectChanges();
        
        // Show success feedback if data was loaded
        if (data.length === 0) {
          this.errorNotificationService.showWarning('No ingestion history found.');
        }
      },
      error: (err) => {
        // Error is also handled by ErrorInterceptor
        // Component adds additional context for debugging
        this.errorNotificationService.logError(err, 'MftManagementComponent.fetchIngestionHistory()');
        this.systemProcessingFlag = false;
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * View staging details for a specific file
   * 
   * Authorization Flow:
   * 1. Check if token exists via TokenService.hasToken()
   * 2. Make HTTP GET request to /mft/staging/{fileId}
   * 3. AuthHttpInterceptor automatically adds Authorization Bearer header
   * 4. ErrorInterceptor handles any error responses (401, 403, 500, etc.)
   * 5. Component displays staging records or error message
   * 
   * @param fileId The file ID to view staging details for
   */
  public viewStagingDashboard(fileId: number): void {
    // Check if token exists before making the request
    if (!this.tokenService.hasToken()) {
      this.errorNotificationService.showError('Authentication required. Please log in.');
      return;
    }

    this.selectedFileId = fileId;
    this.systemProcessingFlag = true;

    this.http.get<StagingLog[]>(`${this.apiBaseUrl}/staging/${fileId}`).subscribe({
      next: (data) => {
        this.activeStagingRecords = data;
        this.applyFilterTransformations();
        this.systemProcessingFlag = false;
        this.cdr.detectChanges();
        
        // Show success feedback
        this.errorNotificationService.showSuccess(
          `Loaded ${data.length} staging record(s).`, 
          2000
        );
      },
      error: (err) => {
        // Error is also handled by ErrorInterceptor
        this.errorNotificationService.logError(err, 'MftManagementComponent.viewStagingDashboard()');
        this.systemProcessingFlag = false;
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * Trigger reprocessing of a specific MFT file (Admin only)
   * 
   * Authorization Flow:
   * 1. Check if token exists via TokenService.hasToken()
   * 2. Make HTTP POST request to /mft/reprocess/{fileId}
   * 3. AuthHttpInterceptor automatically adds Authorization Bearer header
   * 4. Backend enforces [Authorize(Roles = "Admin")] - returns 403 if user lacks role
   * 5. ErrorInterceptor catches 403 and shows "Access Denied" message
   * 6. Component handles response or error
   * 
   * Note: Role checking is currently not enforced by backend (backend JWT lacks role claims).
   * This should be fixed by updating AuthService.GenerateJwtToken() to include role claims.
   * 
   * @param fileId The file ID to reprocess
   */
  public triggerReprocessing(fileId: number): void {
    // Check if token exists before making the request
    if (!this.tokenService.hasToken()) {
      this.errorNotificationService.showError('Authentication required. Please log in.');
      return;
    }

    this.systemProcessingFlag = true;

    this.http.post(`${this.apiBaseUrl}/reprocess/${fileId}`, {}).subscribe({
      next: () => {
        // Show success message and refresh views
        this.errorNotificationService.showSuccess(
          'File reprocessing initiated successfully.',
          2000
        );
        this.fetchIngestionHistory();
        this.viewStagingDashboard(fileId);
      },
      error: (err) => {
        // Error is also handled by ErrorInterceptor
        // Common errors:
        // - 403 Forbidden: User is not an Admin
        // - 401 Unauthorized: Token expired/invalid
        // - 500 Server Error: Backend issue
        this.errorNotificationService.logError(err, 'MftManagementComponent.triggerReprocessing()');
        this.systemProcessingFlag = false;
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * Apply client-side filtering to displayed staging records
   */
  public applyFilterTransformations(): void {
    this.displayedStagingRecords = this.activeStagingRecords.filter(record => {
      const matchStatus = this.operationFilterStatus === 'ALL' || record.validationStatus === this.operationFilterStatus;
      const matchGuid = !this.textSearchGuid || record.guid?.toLowerCase().includes(this.textSearchGuid.toLowerCase().trim());
      return matchStatus && matchGuid;
    });
  }
}