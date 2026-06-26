import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {

  private apiUrl = `${environment.apiBaseUrl}/MasterData`;

  constructor(private http: HttpClient) { }

  /**
   * Get employment status values from master data
   * Authorization header is automatically attached by AuthHttpInterceptor
   */
  getEmploymentStatuses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/employmentstatus`);
  }

  /**
   * Get grade values from master data
   * Authorization header is automatically attached by AuthHttpInterceptor
   */
  getGrades(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/grades`);
  }

  /**
   * Get line of service values from master data
   * Authorization header is automatically attached by AuthHttpInterceptor
   */
  getLineOfServices(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/lineofservice`);
  }

  /**
   * Get office locations for a specific country from master data
   * Authorization header is automatically attached by AuthHttpInterceptor
   * @param countryCode ISO country code
   */
  getOfficeLocations(countryCode: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/officelocations/${countryCode}`);
  }
}