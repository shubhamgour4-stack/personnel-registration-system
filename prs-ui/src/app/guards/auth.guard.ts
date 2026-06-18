import { Injectable, inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { TokenService } from '../services/token.service';
import { ErrorNotificationService } from '../services/error-notification.service';

/**
 * Auth Guard - Route Protection
 * 
 * Prevents unauthenticated users from accessing protected routes.
 * Applied to routes that require JWT authentication.
 * 
 * Verification Flow:
 * 1. User attempts to navigate to protected route
 * 2. AuthGuard checks token validity
 * 3. Check if token exists via TokenService.hasToken()
 * 4. Check if token is not expired via TokenService.isTokenExpired()
 * 5. If valid: Allow navigation to route
 * 6. If invalid: Show error, redirect to /login
 * 
 * Protected Routes (have authGuard):
 * - /dashboard
 * - /mft-management
 * - /profile/:guid
 * 
 * Public Routes (no authGuard):
 * - /login
 * - /register
 * - /
 */
@Injectable({
  providedIn: 'root'
})
export class AuthGuardService {
  constructor(
    private tokenService: TokenService,
    private router: Router,
    private errorNotificationService: ErrorNotificationService
  ) {}

  /**
   * Check if user is authenticated and token is valid
   * 
   * Returns true if:
   * - Token exists in localStorage
   * - Token has not expired
   * 
   * Returns false if:
   * - No token found
   * - Token expired
   */
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    // Check if token exists
    if (!this.tokenService.hasToken()) {
      console.warn(`[AuthGuard] Access denied - No token found. Redirecting to /login`);
      this.errorNotificationService.showError('Please log in to access this page.');
      this.router.navigate(['/login']);
      return false;
    }

    // Check if token has expired
    if (this.tokenService.isTokenExpired()) {
      console.warn(`[AuthGuard] Access denied - Token expired. Redirecting to /login`);
      this.tokenService.removeToken();
      this.errorNotificationService.showError('Your session has expired. Please log in again.');
      this.router.navigate(['/login']);
      return false;
    }

    // Token is valid and not expired - allow navigation
    console.log(`[AuthGuard] Access granted to ${state.url}`);
    return true;
  }
}

/**
 * Standalone Guard Function (Functional API)
 * 
 * Modern Angular 14+ way to define route guards as functions.
 * This function uses `inject()` to access services from Angular's dependency injection system.
 * 
 * Usage in app.routes.ts:
 * { path: 'dashboard', component: Dashboard, canActivate: [authGuard] }
 * 
 * Benefits:
 * - No need to create a class
 * - Cleaner syntax than class-based guards
 * - Services are injected via inject() function
 * - Easier to test and tree-shake
 */
export const authGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): boolean => {
  // Inject services using Angular's inject() function
  // This is the modern way to get services in functional APIs
  const tokenService = inject(TokenService);
  const router = inject(Router);
  const errorNotificationService = inject(ErrorNotificationService);

  // Check if token exists
  if (!tokenService.hasToken()) {
    console.warn(`[AuthGuard] Access denied - No token found. Redirecting to /login`);
    errorNotificationService.showError('Please log in to access this page.');
    router.navigate(['/login']);
    return false;
  }

  // Check if token has expired
  if (tokenService.isTokenExpired()) {
    console.warn(`[AuthGuard] Access denied - Token expired. Redirecting to /login`);
    tokenService.removeToken();
    errorNotificationService.showError('Your session has expired. Please log in again.');
    router.navigate(['/login']);
    return false;
  }

  // Token is valid and not expired - allow navigation
  console.log(`[AuthGuard] Access granted to ${state.url}`);
  return true;
};
