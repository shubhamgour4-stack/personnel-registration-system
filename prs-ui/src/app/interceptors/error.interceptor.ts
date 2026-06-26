import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { ErrorNotificationService } from '../services/error-notification.service';

/**
 * Error Response Interceptor
 * 
 * Handles HTTP error responses with appropriate user actions:
 * - 401 (Unauthorized): Token missing/invalid/expired → Logout + Redirect to login
 * - 403 (Forbidden): User lacks required role → Show error message, keep logged in
 * - 5xx / Network Errors: Show error notification, allow user to retry
 * 
 * Execution Order:
 * 1. AuthHttpInterceptor adds token to request
 * 2. Request sent to backend
 * 3. ErrorInterceptor catches any error responses
 * 4. ErrorInterceptor determines action based on HTTP status code
 * 5. Component receives error in subscribe() catch handler
 */
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private tokenService: TokenService,
    private router: Router,
    private errorNotificationService: ErrorNotificationService
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // 401 Unauthorized: Token missing, invalid, or expired
        if (error.status === 401) {
          this.handle401Unauthorized(error);
        }
        // 403 Forbidden: User authenticated but lacks required role/permission
        else if (error.status === 403) {
          this.handle403Forbidden(error);
        }
        // 5xx Server Errors: Backend issue
        else if (error.status >= 500) {
          this.handle5xxServerError(error);
        }
        // 4xx Client Errors (except 401/403): Invalid request from client
        else if (error.status >= 400 && error.status < 500) {
          this.handle4xxClientError(error);
        }
        // 0 Network Error: Request failed to reach backend (CORS, network down, etc.)
        else if (error.status === 0) {
          this.handleNetworkError(error);
        }

        // Always re-throw error so component can handle it in subscribe() catch
        return throwError(() => error);
      })
    );
  }

  /**
   * Handle 401 Unauthorized (Token expired, invalid, or missing)
   * 
   * Actions:
   * 1. Clear stored token from localStorage
   * 2. Show user-friendly error message
   * 3. Redirect to login page
   * 4. User must re-authenticate
   */
  private handle401Unauthorized(error: HttpErrorResponse): void {
    console.warn('[401 Unauthorized]', error);

    // Clear token from localStorage to prevent stale token usage
    this.tokenService.removeToken();

    // Show user-friendly message
    this.errorNotificationService.showError(
      'Your session has expired. Please log in again.'
    );

    // Redirect to login page
    this.router.navigate(['/login']);
  }

  /**
   * Handle 403 Forbidden (User authenticated but lacks permission)
   * 
   * Actions:
   * 1. Log error for debugging
   * 2. Show permission denied message
   * 3. Keep user logged in (do not clear token)
   * 4. User can navigate elsewhere
   * 
   * Example: Regular user tries to access admin-only endpoint
   */
  private handle403Forbidden(error: HttpErrorResponse): void {
    console.warn('[403 Forbidden]', error);

    // Extract error message from backend response if available
    const errorMessage = error.error?.message || error.error?.error || 
      'You do not have permission to access this resource.';

    // Show message but keep user logged in
    this.errorNotificationService.showError(errorMessage);
  }

  /**
   * Handle 5xx Server Errors (Backend failures)
   * 
   * Actions:
   * 1. Log full error for debugging/monitoring
   * 2. Show generic user-friendly message
   * 3. Keep user logged in (do not clear token)
   * 4. User can retry the operation
   * 
   * Example: Database connection failure, unhandled exception in backend
   */
  private handle5xxServerError(error: HttpErrorResponse): void {
    console.error('[5xx Server Error]', error);

    // Extract specific error from backend if available
    const errorMessage = error.error?.message || 
      `Server error: ${error.status}. Please try again later.`;

    this.errorNotificationService.showError(errorMessage);
  }

  /**
   * Handle 4xx Client Errors (except 401/403)
   * 
   * Actions:
   * 1. Log error for debugging
   * 2. Show specific error message from backend or generic message
   * 3. Keep user logged in
   * 4. User can correct input and retry
   * 
   * Examples: 400 Bad Request, 404 Not Found, 422 Validation Error
   */
  private handle4xxClientError(error: HttpErrorResponse): void {
    console.warn(`[4xx Client Error ${error.status}]`, error);

    // Try to extract specific validation/error messages from backend
    let errorMessage = error.error?.message || 
                       error.error?.error || 
                       `Error: ${error.statusText || 'Request failed'}`;

    // Handle validation error array from backend
    if (error.error?.errors && Array.isArray(error.error.errors)) {
      errorMessage = error.error.errors.join(', ');
    }

    this.errorNotificationService.showError(errorMessage);
  }

  /**
   * Handle Network Errors (status 0)
   * 
   * Actions:
   * 1. Log full error for monitoring
   * 2. Show network error message
   * 3. Keep user logged in
   * 4. User can retry (likely need to check internet connection)
   * 
   * Causes: CORS failure, network down, request timeout, browser offline
   */
  private handleNetworkError(error: HttpErrorResponse): void {
    console.error('[Network Error]', error);

    this.errorNotificationService.showError(
      'Network error. Please check your internet connection and try again.'
    );
  }
}
