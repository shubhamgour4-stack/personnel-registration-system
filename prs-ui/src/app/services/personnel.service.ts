import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PersonnelService {
  
  private apiUrl = `${environment.apiBaseUrl}/Personnel`;

  constructor(private http: HttpClient) { }

  /**
   * Search for personnel profiles by criteria
   * Authorization header is automatically attached by AuthHttpInterceptor
   */
  searchProfiles(firstName: string, lastName: string, email: string, status: string): Observable<any[]> {
    let params = new HttpParams();
    
    if (firstName) params = params.set('firstName', firstName);
    if (lastName) params = params.set('lastName', lastName);
    if (email) params = params.set('email', email);
    if (status) params = params.set('status', status);

    return this.http.get<any[]>(`${this.apiUrl}/search`, { params });
  }

  /**
   * Get a single profile by GUID
   * Authorization header is automatically attached by AuthHttpInterceptor
   */
  getProfileByGuid(guid: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${guid}`);
  }

  /**
   * Submit a completed profile to generate Pseudo Party ID
   * Authorization header is automatically attached by AuthHttpInterceptor
   */
  completeProfile(profileData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/complete-profile`, profileData);
  }
}