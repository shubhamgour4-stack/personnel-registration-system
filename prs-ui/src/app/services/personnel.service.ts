import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PersonnelService {
  
  private apiUrl = 'http://localhost:5076/api/Personnel';

  constructor(private http: HttpClient) { }

  // Helper to get the JWT token for the bouncer
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt_token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  searchProfiles(firstName: string, lastName: string, email: string, status: string): Observable<any[]> {
    let params = new HttpParams();
    
    // Only attach parameters if the user actually typed something
    if (firstName) params = params.set('firstName', firstName);
    if (lastName) params = params.set('lastName', lastName);
    if (email) params = params.set('email', email);
    if (status) params = params.set('status', status);

    return this.http.get<any[]>(`${this.apiUrl}/search`, { 
      headers: this.getHeaders(),
      params: params 
    });
  }
  // Gets a single profile by its GUID
  getProfileByGuid(guid: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${guid}`, {
      headers: this.getHeaders()
    });
  }

  // Submits the final completed profile to generate the Pseudo Party ID
  completeProfile(profileData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/complete-profile`, profileData, {
      headers: this.getHeaders()
    });
  }
}