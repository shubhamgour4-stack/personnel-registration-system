import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly TOKEN_KEY = 'jwt_token';

  /**
   * Store JWT token in localStorage
   * @param token The JWT token to store
   */
  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  /**
   * Retrieve JWT token from localStorage
   * @returns The stored JWT token, or null if not found
   */
  getToken(): string | null {
    const rawToken = localStorage.getItem(this.TOKEN_KEY);
    if (!rawToken) {
      return null;
    }

    const trimmedToken = rawToken.trim();
    if (trimmedToken === '' || trimmedToken === 'undefined' || trimmedToken === 'null') {
      return null;
    }

    return trimmedToken;
  }

  /**
   * Check if a token exists in storage
   * @returns true if token exists, false otherwise
   */
  hasToken(): boolean {
    return !!this.getToken();
  }

  /**
   * Remove JWT token from localStorage
   * Called during logout
   */
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  /**
   * Decode JWT token and check if it's expired
   * @returns true if token is expired, false otherwise
   */
  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      // Extract the payload (second part of JWT)
      const parts = token.split('.');
      if (parts.length !== 3) return true;

      // Decode the payload
      const decoded = JSON.parse(atob(parts[1]));
      const expiryTime = decoded.exp * 1000; // Convert to milliseconds
      const currentTime = Date.now();

      // Add 60 second buffer for clock skew
      return currentTime > expiryTime + 60000;
    } catch (error) {
      console.error('Error decoding token:', error);
      return true;
    }
  }

  /**
   * Get token expiry time in seconds (remaining lifetime)
   * @returns seconds until expiry, or 0 if expired/no token
   */
  getTokenExpirySeconds(): number {
    const token = this.getToken();
    if (!token) return 0;

    try {
      const parts = token.split('.');
      if (parts.length !== 3) return 0;

      const decoded = JSON.parse(atob(parts[1]));
      const expiryTime = decoded.exp * 1000; // Convert to milliseconds
      const currentTime = Date.now();
      const remainingMs = expiryTime - currentTime;

      return Math.max(0, Math.floor(remainingMs / 1000));
    } catch (error) {
      console.error('Error decoding token:', error);
      return 0;
    }
  }
}
