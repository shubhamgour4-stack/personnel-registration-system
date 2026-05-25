import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {

  private apiUrl = 'http://localhost:5076/api/MasterData';

  constructor(private http: HttpClient) { }

  // We don't necessarily need the JWT token here if the MasterDataController 
  // doesn't have the [Authorize] tag, but it's good practice if it does!
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt_token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  getEmploymentStatuses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/employmentstatus`, { headers: this.getHeaders() });
  }

  getGrades(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/grades`, { headers: this.getHeaders() });
  }

  getLineOfServices(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/lineofservice`, { headers: this.getHeaders() });
  }

  getOfficeLocations(countryCode: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/officelocations/${countryCode}`, { headers: this.getHeaders() });
  }
}